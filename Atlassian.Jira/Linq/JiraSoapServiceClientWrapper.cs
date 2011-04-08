using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Atlassian.Jira.Linq
{
    /// <summary>
    /// Wraps the auto-generated JiraSoapServiceClient proxy
    /// </summary>
    internal class JiraSoapServiceClientWrapper: IJiraSoapServiceClient
    {
        private readonly JiraSoapServiceClient _client;

        public JiraSoapServiceClientWrapper(string url)
        {
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
            binding.TransferMode = TransferMode.Buffered;
            binding.UseDefaultWebProxy = true;
            binding.MaxReceivedMessageSize = 2147483647;
            binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;

            if (!url.EndsWith("/"))
            {
                url += "/";
            }


            var endpoint = new EndpointAddress(new Uri(url + "rpc/soap/jirasoapservice-v2"));

            _client = new JiraSoapServiceClient(binding, endpoint);
        }

        public string Login(string username, string password)
        {
            return _client.login(username, password);
        }

        public RemoteIssue[] GetIssuesFromJqlSearch(string token, string jqlSearch, int maxNumResults)
        {
            return _client.getIssuesFromJqlSearch(token, jqlSearch, maxNumResults);
        }
    }
}
