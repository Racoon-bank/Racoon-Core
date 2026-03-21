using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using api.Data;
using api.Models;
using api.Websocket;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace api.Broker
{
    public class OperationConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public OperationConsumer(IServiceScopeFactory scopeFactory)
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

            await channel.QueueDeclareAsync("operations", true, false, false);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);

                    var message = JsonSerializer.Deserialize<OperationMessage>(json);

                    if (message == null)
                    {
                        await channel.BasicAckAsync(ea.DeliveryTag, false);
                        return;
                    }

                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var hub = scope.ServiceProvider.GetRequiredService<IHubContext<BankHub>>();

                    var account = await context.BankAccounts.FindAsync(message.AccountId);

                    if (account != null)
                    {
                        account.Balance += message.Amount;

                        context.BankAccountOperations.Add(new BankAccountOperation
                        {
                            Id = Guid.NewGuid(),
                            Amount = Math.Abs(message.Amount),
                            Type = message.Type,
                            CreatedAt = DateTime.UtcNow,
                            BankAccountId = account.Id
                        });

                        await context.SaveChangesAsync();

                        await hub.Clients.Group(account.Id.ToString()).SendAsync("OperationCreated", new
                        {
                            accountId = account.Id
                        });
                    }

                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    await channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            await channel.BasicConsumeAsync("operations", false, consumer);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}