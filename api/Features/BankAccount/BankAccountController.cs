using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Features.BankAccount.Dto;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace api.Features.BankAccount
{
    [ApiController]
    [Route("api")]
    public class BankAccountController : ControllerBase
    {
        public BankAccountController() { }

        /// <summary>
        /// Gets all bank accounts of a user
        /// </summary>
        [HttpGet("bank-accounts/my")]
        public async Task<ActionResult<List<BankAccountDto>>> GetUserBankAccounts()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets operation history for a bank account
        /// </summary>
        [HttpGet("bank-accounts/{id}/history")]
        public async Task<ActionResult<List<BankAccountOperationDto>>> GetBankAccountHistory(
            [FromRoute] Guid id
        )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets all bank accounts of a user (for employee)
        /// </summary>
        [HttpGet("user/{id}/bank-accounts")]
        public async Task<ActionResult<List<BankAccountDto>>> GetUserBankAccountsForEmployee(
            [FromRoute] Guid id
        )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets all bank accounts of a user (for employee)
        /// </summary>
        [HttpGet("bank-accounts/all")]
        public async Task<ActionResult<List<BankAccountDto>>> GetAllBankAccounts()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates new bank account
        /// </summary>
        [HttpPost("bank-accounts")]
        public async Task<ActionResult<BankAccountDto>> CreateBankAccount()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes a bank account
        /// </summary>
        [HttpDelete("bank-accounts/{id}")]
        public async Task<IActionResult> DeleteBankAccount([FromRoute] Guid id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deposits money to a bank account
        /// </summary>
        [HttpPut("bank-accounts/{id}/deposit")]
        public async Task<ActionResult<BankAccountDto>> AddMoneyToBankAccount(
            [FromRoute] Guid id,
            [FromBody] MoneyOperationDto dto
        )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Withdraws money to a bank account
        /// </summary>
        [HttpPut("bank-accounts/{id}/withdraw")]
        public async Task<ActionResult<BankAccountDto>> WithdrawMoneyToBankAccount(
            [FromRoute] Guid id,
            [FromBody] MoneyOperationDto dto
        )
        {
            throw new NotImplementedException();
        }
    }
}
