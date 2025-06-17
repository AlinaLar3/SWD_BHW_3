namespace OrderService.Dtos.Payment
{
    public class PaymentCompleted
    {
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public int Status { get; set; }
    }
}
