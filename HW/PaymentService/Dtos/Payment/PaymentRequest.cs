using System.Text.Json.Serialization;

namespace PaymentService.Dtos.Payment
{
    public class PaymentRequest
    {
        public Guid OrderId { get; set; }
        public int UserId { get; set; }

        [JsonPropertyName("Total")]
        public decimal Amount { get; set; }
    }
}
