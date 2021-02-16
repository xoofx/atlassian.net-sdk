using Atlassian.Jira.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Threading;
using System.IO;
using Newtonsoft.Json;

namespace Atlassian.Jira.Test.Integration
{
    class TraceReplayer : IJiraRestClient
    {
        private readonly Queue<string> _responses;

        public TraceReplayer(string traceFilePath)
        {
            var lines = File.ReadAllLines(traceFilePath).Where(line => !line.StartsWith("//") && !String.IsNullOrEmpty(line.Trim()));
            _responses = new Queue<string>(lines);
        }

        public RestClient RestSharpClient
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public JiraRestClientSettings Settings
        {
            get
            {
                return new JiraRestClientSettings();
            }
        }

        public string Url
        {
            get
            {
                return "http://testurl";
            }
        }

        public Task<IRestResponse> ExecuteRequestAsync(IRestRequest request, CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<JToken> ExecuteRequestAsync(Method method, string resource, object requestBody = null, CancellationToken token = default(CancellationToken))
        {
            Console.WriteLine($"Method: {method}. Url: {resource}");
            var response = JsonConvert.DeserializeObject(_responses.Dequeue());
            return Task.FromResult(JToken.FromObject(response));
        }

        public Task<T> ExecuteRequestAsync<T>(Method method, string resource, object requestBody = null, CancellationToken token = default(CancellationToken))
        {
            Console.WriteLine($"Method: {method}. Url: {resource}");
            var result = JsonConvert.DeserializeObject<T>(_responses.Dequeue());
            return Task.FromResult(result);

        }

        public byte[] DownloadData(string url)
        {
            throw new NotImplementedException();
        }

        public void Download(string url, string fullFileName)
        {
            throw new NotImplementedException();
        }
    }
}
