using System;
using System.ComponentModel.DataAnnotations;

namespace PaymentService.Models // или PaymentService.Models
{
    public class OutboxMessage
    {
        [Key]
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; }
        public DateTime OccurredAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
