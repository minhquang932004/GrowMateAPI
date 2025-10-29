namespace GrowMate.Contracts.Responses.Payment
{
    public class PaymentQrResponse
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public string GatewayOrderCode { get; set; }
        public string QrContent { get; set; }
        public string QrImageUrl { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? TransactionReference { get; set; }
        public string Status { get; set; }
    }
}


