namespace SkyPointSocial.Core.ClientModels.Auth
{
    /// <summary>
    /// Model for OAuth-based login (Google or Microsoft)
    /// </summary>
    public class OAuthLoginClientModel
    {
        /// <summary>
        /// OAuth provider name ("Google" or "Microsoft")
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Access token received from OAuth provider
        /// </summary>
        public string AccessToken { get; set; }
    }
}