using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;

namespace Atlassian.Jira.Remote
{
    /// <summary>
    /// Implements the IJiraRestClient interface using RestSharp.
    /// </summary>
    public class JiraRestClient : IJiraRestClient
    {
        private readonly RestClient _restClient;
        private readonly JiraRestClientSettings _clientSettings;

        /// <summary>
        /// Creates a new instance of the JiraRestClient class.
        /// </summary
        /// <param name="url">Url to the JIRA server.</param>
        /// <param name="username">Username used to authenticate.</param>
        /// <param name="password">Password used to authenticate.</param>
        /// <param name="settings">Settings to configure the rest client.</param>
        public JiraRestClient(string url, string username = null, string password = null, JiraRestClientSettings settings = null)
        {
            url = url.EndsWith("/") ? url : url += "/";

            _clientSettings = settings ?? new JiraRestClientSettings();
            _restClient = new RestClient(url)
            {
                Proxy = _clientSettings.Proxy
            };

            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
            {
                this._restClient.Authenticator = new HttpBasicAuthenticator(username, password);
            }
        }

        /// <summary>
        /// Rest sharp client used to issue requests.
        /// </summary>
        public RestClient RestSharpClient
        {
            get
            {
                return _restClient;
            }
        }

        /// <summary>
        /// Url to the JIRA server.
        /// </summary>
        public string Url
        {
            get
            {
                return _restClient.BaseUrl.ToString();
            }
        }

        /// <summary>
        /// Settings to configure the rest client.
        /// </summary>
        public JiraRestClientSettings Settings
        {
            get
            {
                return _clientSettings;
            }
        }

        /// <summary>
        /// Executes an async request and serializes the response to an object.
        /// </summary>
        public async Task<T> ExecuteRequestAsync<T>(Method method, string resource, object requestBody = null, CancellationToken token = default(CancellationToken))
        {
            var result = await ExecuteRequestAsync(method, resource, requestBody, token).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(result.ToString(), Settings.JsonSerializerSettings);
        }

        /// <summary>
        /// Executes an async request and returns the response as JSON.
        /// </summary>
        public async Task<JToken> ExecuteRequestAsync(Method method, string resource, object requestBody = null, CancellationToken token = default(CancellationToken))
        {
            if (method == Method.GET && requestBody != null)
            {
                throw new InvalidOperationException($"GET requests are not allowed to have a request body. Resource: {resource}. Body: {requestBody}");
            }

            var request = new RestRequest();
            request.Method = method;
            request.Resource = resource;
            request.RequestFormat = DataFormat.Json;

            if (requestBody is string)
            {
                request.AddParameter(new Parameter
                {
                    Name = "application/json",
                    Type = ParameterType.RequestBody,
                    Value = requestBody
                });
            }
            else if (requestBody != null)
            {
                request.JsonSerializer = new RestSharpJsonSerializer(JsonSerializer.Create(Settings.JsonSerializerSettings));
                request.AddJsonBody(requestBody);
            }

            LogRequest(request, requestBody);
            var response = await ExecuteRawResquestAsync(request, token).ConfigureAwait(false);
            return GetValidJsonFromResponse(request, response);
        }

        /// <summary>
        /// Executes a request with logging and validation.
        /// </summary>
        public async Task<IRestResponse> ExecuteRequestAsync(IRestRequest request, CancellationToken token = default(CancellationToken))
        {
            LogRequest(request);
            var response = await ExecuteRawResquestAsync(request, token).ConfigureAwait(false);
            GetValidJsonFromResponse(request, response);
            return response;
        }

        /// <summary>
        /// Executes a raw request.
        /// </summary>
        protected virtual Task<IRestResponse> ExecuteRawResquestAsync(IRestRequest request, CancellationToken token)
        {
            return _restClient.ExecuteTaskAsync(request, token);
        }

        private void LogRequest(IRestRequest request, object body = null)
        {
            if (this._clientSettings.EnableRequestTrace)
            {
                Trace.WriteLine(String.Format("[{0}] Request Url: {1}",
                    request.Method,
                    request.Resource));

                if (body != null)
                {
                    Trace.WriteLine(String.Format("[{0}] Request Data: {1}",
                        request.Method,
                        JsonConvert.SerializeObject(body, new JsonSerializerSettings()
                        {
                            Formatting = Formatting.Indented,
                            NullValueHandling = NullValueHandling.Ignore
                        })));
                }
            }
        }

        private JToken GetValidJsonFromResponse(IRestRequest request, IRestResponse response)
        {
            var content = response.Content != null ? response.Content.Trim() : string.Empty;

            if (this._clientSettings.EnableRequestTrace)
            {
                Trace.WriteLine(String.Format("[{0}] Response for Url: {1}\n{2}",
                    request.Method,
                    request.Resource,
                    content));
            }

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                throw new InvalidOperationException($"Error Message: {response.ErrorMessage}");
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new System.Security.Authentication.AuthenticationException(string.Format("Response Content: {0}", content));
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ResourceNotFoundException($"Response Content: {content}");
            }
            else if ((int)response.StatusCode >= 400)
            {
                throw new InvalidOperationException($"Response Status Code: {(int)response.StatusCode}. Response Content: {content}");
            }
            else if (string.IsNullOrWhiteSpace(content))
            {
                return new JObject();
            }
            else if (!content.StartsWith("{") && !content.StartsWith("["))
            {
                throw new InvalidOperationException(String.Format("Response was not recognized as JSON. Content: {0}", content));
            }
            else
            {
                JToken parsedContent;

                try
                {
                    parsedContent = JToken.Parse(content);
                }
                catch (JsonReaderException ex)
                {
                    throw new InvalidOperationException(String.Format("Failed to parse response as JSON. Content: {0}", content), ex);
                }

                if (parsedContent != null && parsedContent.Type == JTokenType.Object && parsedContent["errorMessages"] != null)
                {
                    throw new InvalidOperationException(string.Format("Response reported error(s) from JIRA: {0}", parsedContent["errorMessages"].ToString()));
                }

                return parsedContent;
            }
        }
    }
}
