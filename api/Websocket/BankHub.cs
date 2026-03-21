using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace api.Websocket
{
    public class BankHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public BankHub(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task SubscribeToAccount(string accountId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roles = Context.User?.FindAll(ClaimTypes.Role).Select(role => role.Value).ToList();

            if (userId == null)
                throw new HubException("Unathorized");

            if (!Guid.TryParse(accountId, out var accountGuid))
                throw new HubException("Invalid accountId");
            var account = await _context.BankAccounts.FirstOrDefaultAsync(a => a.Id == accountGuid);
            if (account == null)
                throw new HubException("Account not found");

            var isOwner = account.UserId == Guid.Parse(userId);
            var isEmployee = roles != null && roles.Contains("Employee");
            if (!isOwner && !isEmployee)
                throw new HubException("Access denied");

            await Groups.AddToGroupAsync(Context.ConnectionId, accountId);
        }

        public async Task UnsubscribeFromAccount(string accountId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, accountId);
        }
    }
}