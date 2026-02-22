using api.Features.BankAccount.Dto;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace api.Features.BankAccount
{
    [ApiController]
    [Route("api")]
    public class BankAccountController : ControllerBase
    {
        private readonly IBankAccountService _bankAccountService;
        public BankAccountController(IBankAccountService bankAccountService)
        {
            _bankAccountService = bankAccountService;
        }

        /// <summary>
        /// Gets all bank accounts of a user
        /// </summary>
        [HttpGet("bank-accounts/my")]
        public async Task<ActionResult<List<BankAccountDto>>> GetUserBankAccounts()
        {
            var accounts = await _bankAccountService.GetAllAsync(); // add userId later
            return Ok(accounts);
        }

        /// <summary>
        /// Gets operation history for a bank account
        /// </summary>
        [HttpGet("bank-accounts/{id}/history")]
        public async Task<ActionResult<List<BankAccountOperationDto>>> GetBankAccountHistory(
            [FromRoute] Guid id
        )
        {
            var history = await _bankAccountService.GetHistoryAsync(id);
            return Ok(history);
        }

        /// <summary>
        /// Gets all bank accounts of a user (for employee)
        /// </summary>
        [HttpGet("user/{id}/bank-accounts")]
        public async Task<ActionResult<List<BankAccountDto>>> GetUserBankAccountsForEmployee(
            [FromRoute] Guid id
        )
        {
            var accounts = await _bankAccountService.GetAllByIdAsync(id);
            return Ok(accounts);
        }

        /// <summary>
        /// Gets all bank accounts (for employee)
        /// </summary>
        [HttpGet("bank-accounts/all")]
        public async Task<ActionResult<List<BankAccountDto>>> GetAllBankAccounts()
        {
            var accounts = await _bankAccountService.GetAllAsync();
            return Ok(accounts);
        }

        /// <summary>
        /// Creates new bank account
        /// </summary>
        [HttpPost("bank-accounts")]
        public async Task<ActionResult<BankAccountDto>> CreateBankAccount([FromBody] Guid id)
        {
            var account = await _bankAccountService.AddAsync(id);
            return Ok(account);
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
            var bankAccount = await _bankAccountService.DepositMoneyAsync(id, dto);
            return Ok(bankAccount);
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
            var bankAccount = await _bankAccountService.WithdrawMoneyAsync(id, dto);
            return Ok(bankAccount);
        }
    }
}
