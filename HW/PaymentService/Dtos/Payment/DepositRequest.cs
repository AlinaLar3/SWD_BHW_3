namespace PaymentService.Dtos.Payment
{
    public class DepositRequest
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
    }
}
