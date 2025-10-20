namespace GrowMate.Contracts.Responses.Payment
{
    public class PaymentResponse
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionReference { get; set; }
        public string SourceType { get; set; }
        public string Status { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? Notes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class PaymentDetailResponse
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionReference { get; set; }
        public string SourceType { get; set; }
        public string Status { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? Notes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Related data
        public OrderSummaryResponse? Order { get; set; }
        public List<AdoptionSummaryResponse>? Adoptions { get; set; }
    }

    public class OrderSummaryResponse
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int SellerId { get; set; }
        public string SellerName { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdoptionSummaryResponse
    {
        public int AdoptionId { get; set; }
        public int TreeId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string TreeName { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
