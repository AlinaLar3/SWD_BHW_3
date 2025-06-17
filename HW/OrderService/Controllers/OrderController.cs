using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Dtos.Orders;
using OrderService.Models;
using System.Text.Json;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public OrdersController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (request.Amount <= 0)
                return BadRequest("Amount must be greater than zero.");

            var order = new Order
            {
                UserId = request.UserId,
                Amount = request.Amount,
                Description = request.Description,
                Status = OrderStatus.NEW
            };

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await _dbContext.Orders.AddAsync(order);
                await _dbContext.SaveChangesAsync();

                var orderCreatedEvent = new
                {
                    OrderId = order.Id,
                    UserId = order.UserId,
                    Total = order.Amount
                };

                var outbox = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = "OrderCreated",
                    OccurredAt = DateTime.UtcNow,
                    Payload = JsonSerializer.Serialize(orderCreatedEvent),
                };

                await _dbContext.OutboxMessages.AddAsync(outbox);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error creating order: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            var orders = await _dbContext.Orders.ToListAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrderById(Guid id)
        {
            var order = await _dbContext.Orders.FindAsync(id);
            return order == null ? NotFound() : Ok(order);
        }
    }
}
