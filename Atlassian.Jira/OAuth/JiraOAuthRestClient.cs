using System;
using System.Threading;
using System.Threading.Tasks;
using Atlassian.Jira.Remote;
using RestSharp;
using RestSharp.Authenticators;

namespace Atlassian.Jira.OAuth
{
    /// <summary>
    /// Reimplements the <see cref="JiraRestClient"/> to use the OAuth protocol.
    /// </summary>
    public class JiraOAuthRestClient : JiraRestClient
    {
        /// <summary>
        /// Create a <see cref="JiraRestClient"/> using OAuth protocol.
        /// </summary>
        /// <param name="url">The url of the Jira instance to request to.</param>
        /// <param name="consumerKey">The consumer key provided by the Jira application link.</param>
        /// <param name="consumerSecret">The consumer public key in XML format.</param>
        /// <param name="oAuthAccessToken">The OAuth access token obtained from Jira.</param>
        /// <param name="oAuthSignatureMethod">The signature method used to sign the request.</param>
        /// <param name="settings">The settings used to configure the rest client.</param>
        /// <remarks>
        /// Although RestSharp asks for it, the access token secret is not needed for
        /// protected resource calls (see https://oauth.net/core/1.0a/#anchor12).
        /// </remarks>
        public JiraOAuthRestClient(
            string url,
            string consumerKey,
            string consumerSecret,
            string oAuthAccessToken,
            JiraOAuthSignatureMethod oAuthSignatureMethod = JiraOAuthSignatureMethod.RsaSha1,
            JiraRestClientSettings settings = null)
            : base(
                url,
                OAuth1Authenticator.ForProtectedResource(
                    consumerKey,
                    consumerSecret,
                    oAuthAccessToken,
                    accessTokenSecret: string.Empty,
                    OAuthTokenHelper.ConvertToRestSharpSignatureMethod(oAuthSignatureMethod)),
                settings)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Replace the request query with a collection of parameters.
        /// </summary>
        protected override Task<IRestResponse> ExecuteRawResquestAsync(IRestRequest request, CancellationToken token)
        {
            Uri fullPath = new Uri(RestSharpClient.BaseUrl, request.Resource);

            // If there no parameters, we do nothing
            if (!string.IsNullOrEmpty(fullPath.Query))
            {
                request.Parameters.AddRange(QueryParametersHelper.GetQueryParametersFromPath(fullPath.Query));

                request.Resource = request.Resource.Replace(fullPath.Query, "");
            }

            return base.ExecuteRawResquestAsync(request, token);
        }
    }
}
