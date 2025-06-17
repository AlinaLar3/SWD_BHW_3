using System;
using System.ComponentModel.DataAnnotations;

namespace PaymentService.Models
{
    public class InboxMessage
    {
        [Key]
        public int Id { get; set; }
        public string MessageId { get; set; } 
        public string Topic { get; set; }
        public string Payload { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}
