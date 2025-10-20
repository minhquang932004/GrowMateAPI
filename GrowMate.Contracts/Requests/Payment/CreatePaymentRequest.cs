namespace GrowMate.Contracts.Requests.Payment
{
    public class CreatePaymentRequest
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public string PaymentMethod { get; set; } // "VNPAY", "SEPAY", "CASH", etc.
        public string TransactionReference { get; set; }
        public string SourceType { get; set; } // "WEB", "MOBILE", "API", etc.
        public string Status { get; set; } = "COMPLETED";
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string? Notes { get; set; }
    }

    public class UpdatePaymentRequest
    {
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionReference { get; set; }
        public string? SourceType { get; set; }
        public string? Status { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdatePaymentStatusRequest
    {
        public string Status { get; set; }
        public string? Notes { get; set; }
    }
}
