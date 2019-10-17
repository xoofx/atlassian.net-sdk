using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth;

namespace Atlassian.Jira.OAuth
{
    /// <summary>
    /// Helper to create and send request for the OAuth authentification process.
    /// </summary>
    public class OAuthTokenHelper
    {
        /// <summary>
        /// Generate a request token for the OAuth authentification process.
        /// </summary>
        /// <param name="oAuthRequestTokenSettings"> The request token settings.</param>
        /// <param name="cancellationToken">Cancellation token for this operation.</param>
        /// <returns>The <see cref="OAuthRequestToken" /> containing the request token, the consumer token and the authorize url.</returns>
        public static async Task<OAuthRequestToken> GenerateRequestTokenAsync(OAuthRequestTokenSettings oAuthRequestTokenSettings, CancellationToken cancellationToken = default(CancellationToken))
        {
            var authenticator = OAuth1Authenticator.ForRequestToken(
                oAuthRequestTokenSettings.ConsumerKey,
                oAuthRequestTokenSettings.ConsumerSecret,
                oAuthRequestTokenSettings.CallbackUrl);

            authenticator.SignatureMethod = ConvertToRestSharpSignatureMethod(oAuthRequestTokenSettings.SignatureMethod);

            RestClient client = new RestClient(oAuthRequestTokenSettings.Url)
            {
                Authenticator = authenticator
            };

            var requestTokenResponse = await client.ExecutePostTaskAsync(new RestRequest(oAuthRequestTokenSettings.RequestTokenUrl), cancellationToken).ConfigureAwait(false);

            if (requestTokenResponse.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var requestTokenQuery = HttpUtility.ParseQueryString(requestTokenResponse.Content.Trim());

            var oauthToken = requestTokenQuery["oauth_token"];
            var authorizeUri = $"{oAuthRequestTokenSettings.Url}/{oAuthRequestTokenSettings.AuthorizeTokenUrl}?oauth_token={oauthToken}";

            return new OAuthRequestToken(
                authorizeUri,
                oauthToken,
                requestTokenQuery["oauth_token_secret"],
                requestTokenQuery["oauth_callback_confirmed"]);
        }

        /// <summary>
        /// Obtain the access token from an authorized request token.
        /// </summary>
        /// <param name="oAuthAccessTokenSettings">The settings to obtain the access token.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The access token from Jira.
        /// Return null if the token was not returned by Jira or the token secret for the request token and the access token don't match.</returns>
        public static async Task<string> ObtainAccessTokenAsync(OAuthAccessTokenSettings oAuthAccessTokenSettings, CancellationToken cancellationToken)
        {
            var client = new RestClient(oAuthAccessTokenSettings.Url)
            {
                Authenticator = OAuth1Authenticator.ForAccessToken(
                    oAuthAccessTokenSettings.ConsumerKey,
                    oAuthAccessTokenSettings.ConsumerSecret,
                    oAuthAccessTokenSettings.OAuthRequestToken,
                    oAuthAccessTokenSettings.OAuthTokenSecret,
                    ConvertToRestSharpSignatureMethod(oAuthAccessTokenSettings.SignatureMethod))
            };

            var accessTokenResponse = await client.ExecutePostTaskAsync(new RestRequest(oAuthAccessTokenSettings.AccessTokenUrl), cancellationToken).ConfigureAwait(false);

            if (accessTokenResponse.StatusCode != HttpStatusCode.OK)
            {
                // The token has not been authorize or something went wrong
                return null;
            }

            var accessTokenQuery = HttpUtility.ParseQueryString(accessTokenResponse.Content.Trim());

            if (oAuthAccessTokenSettings.OAuthTokenSecret != accessTokenQuery["oauth_token_secret"])
            {
                // The request token secret and access token secret do not match.
                return null;
            }

            return accessTokenQuery["oauth_token"];
        }

        /// <summary>
        /// Convert a <see cref="JiraOAuthSignatureMethod"/> to <see cref="OAuthSignatureMethod"/> .
        /// </summary>
        /// <param name="signatureMethod">The signature method to convert.</param>
        /// <returns>The signature method from RestSharp.</returns>
        public static OAuthSignatureMethod ConvertToRestSharpSignatureMethod(JiraOAuthSignatureMethod signatureMethod)
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
