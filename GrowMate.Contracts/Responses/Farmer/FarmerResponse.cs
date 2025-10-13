namespace GrowMate.Contracts.Responses.Farmer
{
    /// <summary>
    /// Response model representing farmer information
    /// </summary>
    public class FarmerResponse
    {
        /// <summary>
        /// The unique identifier for the farmer
        /// </summary>
        public int FarmerId { get; set; }

        /// <summary>
        /// The name of the farmer's farm
        /// </summary>
        public string FarmName { get; set; }

        /// <summary>
        /// The physical address of the farm
        /// </summary>
        public string FarmAddress { get; set; }

        /// <summary>
        /// Contact phone number for the farmer
        /// </summary>
        public string ContactPhone { get; set; }
    }
}