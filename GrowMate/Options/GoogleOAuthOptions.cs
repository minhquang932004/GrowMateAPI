namespace GrowMateWebAPIs
{
    /// <summary>
    /// Strongly-typed configuration for Google OAuth.
    /// </summary>
    public sealed class GoogleOAuthOptions
    {
        public string ClientId { get; set; } = string.Empty;

        public string ClientSecret { get; set; } = string.Empty;
    }
}

