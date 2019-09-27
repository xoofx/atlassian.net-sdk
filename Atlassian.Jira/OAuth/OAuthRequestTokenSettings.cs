namespace Atlassian.Jira.OAuth
{
    /// <summary>
    /// Request token settings to help generate the request token.
    /// </summary>
    public class OAuthRequestTokenSettings
    {
        public string Url;
        public string ConsumerKey;
        public string ConsumerSecret;
        public string CallbackUrl;
        public JiraOAuthSignatureMethod SignatureMethod;
        public string RequestTokenUrl;
        public string AuthorizeTokenUrl;

        /// <summary>
        /// Creates a Request token settings to generate a request token.
        /// </summary>
        /// <param name="url">The url of the Jira instance to request to.</param>
        /// <param name="consumerKey">The consumer key provided by the Jira application link.</param>
        /// <param name="consumerSecret">The consumer public key in XML format.</param>
        /// <param name="callbackUrl">The callback url for the request token.</param>
        /// <param name="signatureMethod">The signature method used to sign the request.</param>
        /// <param name="requestTokenUrl">The relative path to the url to request the token to Jira.</param>
        /// <param name="authorizeTokenUrl">The relative path to the url to authorize the token.</param>
        public OAuthRequestTokenSettings(
            string url,
            string consumerKey,
            string consumerSecret,
            string callbackUrl = null,
            JiraOAuthSignatureMethod signatureMethod = JiraOAuthSignatureMethod.RsaSha1,
            string requestTokenUrl = "plugins/servlet/oauth/request-token",
            string authorizeTokenUrl = "plugins/servlet/oauth/authorize")
        {
            Url = url;
            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
            CallbackUrl = callbackUrl;
            SignatureMethod = signatureMethod;
            RequestTokenUrl = requestTokenUrl;
            AuthorizeTokenUrl = authorizeTokenUrl;
        }
    }
}
