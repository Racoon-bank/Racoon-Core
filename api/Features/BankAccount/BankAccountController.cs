using api.Features.BankAccount.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using api.Extensions;
using api.Features.Transfers;

namespace api.Features.BankAccount
{
    [ApiController]
    [Route("api/bank-accounts")]
    public class BankAccountController : ControllerBase
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly ITransferService _transferService;
        public BankAccountController(IBankAccountService bankAccountService, ITransferService transferService)
        {
            _bankAccountService = bankAccountService;
            _transferService = transferService;
        }

        /// <summary>
        /// Gets all bank accounts of a user
        /// </summary>
        [HttpGet("my")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<List<BankAccountDto>>> GetUserBankAccounts()
        {
            var userId = User.GetId();
            var accounts = await _bankAccountService.GetAllByIdAsync(new Guid(userId));
            return Ok(accounts);
        }

        /// <summary>
        /// Gets operation history for a bank account
        /// </summary>
        [HttpGet("{id}/history")]
        [Authorize]
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
        [HttpGet("/api/user/{id}/bank-accounts")]
        [Authorize(Roles = "Employee")]
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
        [HttpGet("all")]
        [Authorize(Roles = "Employee")]
        public async Task<ActionResult<List<BankAccountDto>>> GetAllBankAccounts()
        {
            var accounts = await _bankAccountService.GetAllAsync();
            return Ok(accounts);
        }

        /// <summary>
        /// Creates new bank account
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<BankAccountDto>> CreateBankAccount([FromBody] CreateBankAccountDto dto)
        {
            var userId = User.GetId();
            var account = await _bankAccountService.AddAsync(new Guid(userId), dto);
            return Ok(account);
        }

        /// <summary>
        /// Deletes a bank account
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> DeleteBankAccount([FromRoute] Guid id)
        {
            var userId = User.GetId();
            await _bankAccountService.DeleteAsync(id, userId);
            return Ok();
        }

        /// <summary>
        /// Changes visibility of bank account
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> ChangeBankAccountVisibility([FromRoute] Guid id)
        {
            var userId = User.GetId();
            var account = await _bankAccountService.ChangeVisibility(id, new Guid(userId));
            return Ok(account);
        }

        /// <summary>
        /// Deposits money to a bank account
        /// </summary>
        [HttpPut("{id}/deposit")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<BankAccountDto>> AddMoneyToBankAccount(
            [FromRoute] Guid id,
            [FromBody] MoneyOperationDto dto
        )
        {
            var userId = User.GetId();
            await _bankAccountService.DepositMoneyAsync(id, dto, userId);
            return Ok();
        }

        /// <summary>
        /// Withdraws money from a bank account
        /// </summary>
        [HttpPut("{id}/withdraw")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<BankAccountDto>> WithdrawMoneyToBankAccount(
            [FromRoute] Guid id,
            [FromBody] MoneyOperationDto dto
        )
        {
            var userId = User.GetId();
            await _bankAccountService.WithdrawMoneyAsync(id, dto, userId);
            return Ok();
        }

        /// <summary>
        /// Transfer money from one bank account to another one
        /// </summary>
        [HttpPut("transfer")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<BankAccountDto>> TransferMoney(
            [FromBody] TransferDto dto
        )
        {
            var userId = User.GetId();
            var bankAccount = await _transferService.TransferMoneyAsync(dto, userId);
            return Ok(bankAccount);
        }

        /// <summary>
        /// Apply credit to a bank account
        /// </summary>
        [HttpPut("/internal/bank-accounts/{id}/apply-credit")]
        [Extensions.ServiceKey]
        public async Task<ActionResult<BankAccountDto>> ApplyCredit(
            [FromRoute] Guid id,
            [FromBody] MoneyOperationDto dto
        )
        {
            await _bankAccountService.ApplyCredit(id, dto);
            return Ok();
        }

        /// <summary>
        /// Pay credit from a bank account
        /// </summary>
        [HttpPut("/internal/bank-accounts/{id}/pay-credit")]
        [Extensions.ServiceKey]
        public async Task<ActionResult<BankAccountDto>> PayCredit(
            [FromRoute] Guid id,
            [FromBody] MoneyOperationDto dto
        )
        {
            await _bankAccountService.PayCredit(id, dto);
            return Ok();
        }
    }
}
