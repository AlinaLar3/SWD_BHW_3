namespace OrderService.Dtos.Orders
{
    public class CreateOrderRequest
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }

}
