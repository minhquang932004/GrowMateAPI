namespace GrowMate.Contracts.Requests.Payment
{
    public class CreateSepayQrRequest
    {
        public int OrderId { get; set; }
        public int? ExpiresMinutes { get; set; } = 15;
    }
}


