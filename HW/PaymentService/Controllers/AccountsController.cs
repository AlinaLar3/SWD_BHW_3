using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Dtos.Payment;
using PaymentService.Models;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    public class AccountsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AccountsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("create/{userId}")]
        public async Task<IActionResult> CreateAccount(int userId)
        {
            var existingAccount = await _context.Accounts.FindAsync(userId);
            if (existingAccount != null)
                return BadRequest("Account already exists.");

            var account = new Account
            {
                UserId = userId,
                Balance = 0m
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Ok(account);
        }

        [HttpGet("balance/{userId}")]
        public async Task<IActionResult> GetBalance(int userId)
        {
            var account = await _context.Accounts.FindAsync(userId);
            if (account == null)
                return NotFound("Account not found.");

            return Ok(new { userId = account.UserId, balance = account.Balance });
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE \"Accounts\" SET \"Balance\" = \"Balance\" + {0} WHERE \"UserId\" = {1}",
                    request.Amount, request.UserId);

                if (rowsAffected == 0)
                    return NotFound("Account not found.");

                await transaction.CommitAsync();

                var account = await _context.Accounts.FindAsync(request.UserId);
                return Ok(new { message = "Deposit successful.", balance = account.Balance });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
