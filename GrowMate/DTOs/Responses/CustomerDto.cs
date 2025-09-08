namespace GrowMate.DTOs.Responses
{
    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public string? ShippingAddress { get; set; }
        public decimal? WalletBalance { get; set; }
    }
}