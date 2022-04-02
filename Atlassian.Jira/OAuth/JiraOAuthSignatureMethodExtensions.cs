using RestSharp.Authenticators.OAuth;

namespace Atlassian.Jira.OAuth
{
    /// <summary>
    /// Provides extension methods for <see cref="JiraOAuthSignatureMethod"/>.
    /// </summary>
    public static class JiraOAuthSignatureMethodExtensions
    {
        /// <summary>
        /// Converts to <see cref="OAuthSignatureMethod"/>.
        /// </summary>
        /// <param name="signatureMethod">The signature method.</param>
        /// <returns>The RestSharp signature method.</returns>
        public static OAuthSignatureMethod ToOAuthSignatureMethod(this JiraOAuthSignatureMethod signatureMethod)
        {
            switch (signatureMethod)
            {
                case JiraOAuthSignatureMethod.HmacSha1:
                    return OAuthSignatureMethod.HmacSha1;
                case JiraOAuthSignatureMethod.HmacSha256:
                    return OAuthSignatureMethod.HmacSha256;
                case JiraOAuthSignatureMethod.PlainText:
                    return OAuthSignatureMethod.PlainText;
                case JiraOAuthSignatureMethod.RsaSha1:
                    return OAuthSignatureMethod.RsaSha1;
                default:
                    return OAuthSignatureMethod.RsaSha1;
            }
        }
    }
}
