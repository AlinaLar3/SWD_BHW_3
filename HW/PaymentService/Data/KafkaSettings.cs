namespace PaymentService.Data
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; }
        public string GroupId { get; set; }
        public TopicsSettings Topics { get; set; }
    }

    public class TopicsSettings
    {
        public string PaymentEvents { get; set; }
        public string OrderEvents { get; set; }
    }
}
