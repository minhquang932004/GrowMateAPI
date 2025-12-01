namespace GrowMate.Contracts.Requests.Payment
{
    public class CreateSepayQrRequest
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
    }
}


