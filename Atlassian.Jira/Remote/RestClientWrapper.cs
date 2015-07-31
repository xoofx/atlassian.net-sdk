using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;

namespace Atlassian.Jira.Remote
{
    /// <summary>
    /// Class that wraps a client to execute requests.
    /// </summary>
    public class RestClientWrapper
    {
        private readonly RestClient _restClient;
        private readonly string _url;
        private readonly RestClientSettings _clientSettings;
        private readonly JsonSerializerSettings _serializerSettings;

        /// <summary>
        /// Creates a new client wrapper.
        /// </summary>
        /// <param name="baseUrl">Base url of server.</param>
        /// <param name="settings">Settings object to configure the client.</param>
        public RestClientWrapper(string baseUrl, RestClientSettings settings = null)
            : this(baseUrl, null, null, settings)
        {
        }

        /// <summary>
        /// Creates a new client wrapper.
        /// </summary>
        /// <param name="baseUrl">Base url of server.</param>
        /// <param name="username">Username used to authenticate with server.</param>
        /// <param name="password">Password used to authenticate with server.</param>
        /// <param name="settings">Settings object to configure the client.</param>
        public RestClientWrapper(string baseUrl, string username, string password, RestClientSettings settings = null)
        {
            this._clientSettings = settings ?? new RestClientSettings();
            this._serializerSettings = new JsonSerializerSettings();
            this._serializerSettings.NullValueHandling = NullValueHandling.Ignore;
            this._url = baseUrl.EndsWith("/") ? baseUrl : baseUrl += "/";
            this._restClient = new RestClient(this._url);

            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
            {
                this._restClient.Authenticator = new HttpBasicAuthenticator(username, password);
            }
        }

        /// <summary>
        /// Server's base url.
        /// </summary>
        public string BaseUrl
        {
            get
            {
                return this._url;
            }
        }

        /// <summary>
        /// Settings to serialize and deserialize JSON content.
        /// </summary>
        public JsonSerializerSettings SerializerSettings
        {
            get
            {
                return this._serializerSettings;
            }
        }

        /// <summary>
        /// Executes a request and returns the response as JSON.
        /// </summary>
        /// <param name="method">Request method.</param>
        /// <param name="resource">Request resource url.</param>
        /// <param name="requestBody">Request body to be serialized.</param>
        public JToken ExecuteRequest(Method method, string resource, object requestBody = null)
        {
            var request = new RestRequest();
            request.Method = method;
            request.Resource = resource;

            if (requestBody != null)
            {
                request.RequestFormat = DataFormat.Json;
                request.JsonSerializer = new RestSharpJsonSerializer(JsonSerializer.Create(this._serializerSettings));
                request.AddJsonBody(requestBody);
            }

            LogRequest(request, requestBody);
            var response = this._restClient.Execute(request);
            EnsureValidResponse(response);

            return response.StatusCode != HttpStatusCode.NoContent ? JToken.Parse(response.Content) : new JObject();
        }

        /// <summary>
        /// Executes a request and serializes the response to an object.
        /// </summary>
        /// <typeparam name="T">Type to serialize the reponse.</typeparam>
        /// <param name="method">Request method.</param>
        /// <param name="resource">Request resource url.</param>
        /// <param name="requestBody">Request body to be serialized.</param>
        public T ExecuteRequest<T>(Method method, string resource, object requestBody = null)
        {
            var response = ExecuteRequest(method, resource, requestBody);
            return JsonConvert.DeserializeObject<T>(response.ToString(), _serializerSettings);

        }

        /// <summary>
        /// Executes a request.
        /// </summary>
        /// <param name="request">Request object.</param>
        public IRestResponse ExecuteRequest(IRestRequest request)
        {
            var response = this._restClient.Execute(request);
            EnsureValidResponse(response);
            return response;
        }

        private void LogRequest(RestRequest request, object body = null)
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

        private void EnsureValidResponse(IRestResponse response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError
                || response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new InvalidOperationException(response.Content);
            }
        }
    }
}
