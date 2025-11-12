namespace GrowMate.Contracts.Requests.Auth
{
    /// <summary>
    /// Request payload carrying the Google authorization code exchanged on the frontend.
    /// </summary>
    public class GoogleAuthCodeRequest
    {
        /// <summary>
        /// The short-lived authorization code returned by Google Identity Services.
        /// </summary>
        public string Code { get; set; } = string.Empty;
    }
}

