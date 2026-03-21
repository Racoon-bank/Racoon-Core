using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using api.Data;
using api.Enum;
using api.Exceptions;
using api.Models;
using api.Websocket;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace api.Broker
{
    public class TransferConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public TransferConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "admin",
                Password = "admin123"
            };
            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync("transfers", true, false, false);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var message = JsonSerializer.Deserialize<TransferMessage>(json);

                    if (message == null)
                    {
                        await channel.BasicAckAsync(ea.DeliveryTag, false);
                        return;
                    }

                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var hub = scope.ServiceProvider.GetRequiredService<IHubContext<BankHub>>();

                    using var transaction = await context.Database.BeginTransactionAsync();

                    var fromAccount = await context.BankAccounts.FindAsync(message.FromAccountId);
                    var toAccount = await context.BankAccounts.FindAsync(message.ToAccountId);

                    if (fromAccount == null)
                        throw new BankAccountNotFoundException(message.FromAccountId);
                    if (toAccount == null)
                        throw new BankAccountNotFoundException(message.ToAccountId);
                    if (fromAccount.Balance < message.Amount)
                        throw new InsufficientFundsException();

                    fromAccount.Balance -= message.Amount;
                    toAccount.Balance += message.ConvertedAmount;

                    var now = DateTime.UtcNow;

                    context.BankAccountOperations.AddRange(
                        new BankAccountOperation
                        {
                            Id = Guid.NewGuid(),
                            Amount = message.Amount,
                            Type = OperationType.TransferOut,
                            CreatedAt = now,
                            BankAccountId = fromAccount.Id
                        },
                        new BankAccountOperation
                        {
                            Id = Guid.NewGuid(),
                            Amount = message.ConvertedAmount,
                            Type = OperationType.TransferIn,
                            CreatedAt = now,
                            BankAccountId = toAccount.Id
                        }
                    );

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    await hub.Clients.Group(fromAccount.Id.ToString())
                        .SendAsync("OperationCreated", new { accountId = fromAccount.Id });

                    await hub.Clients.Group(toAccount.Id.ToString())
                        .SendAsync("OperationCreated", new { accountId = toAccount.Id });

                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                    await channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            await channel.BasicConsumeAsync("transfers", false, consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}