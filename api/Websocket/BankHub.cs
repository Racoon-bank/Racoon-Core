using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using Microsoft.AspNetCore.SignalR;

namespace api.Websocket
{
    public class BankHub : Hub
    {
        public async Task SubscribeToAccount(string accountId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, accountId);
        }

        public async Task UnsubscribeFromAccount(string accountId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, accountId);
        }
    }
}