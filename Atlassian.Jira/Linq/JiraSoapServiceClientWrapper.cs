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
            if (!url.EndsWith("/"))
            {
                url += "/";
            }

            var endPointUri = new Uri(url + "rpc/soap/jirasoapservice-v2");

            BasicHttpBinding binding = null;
            if (endPointUri.Scheme == "https")
            {
                binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
            }
            else
            {
                binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
            }
            binding.TransferMode = TransferMode.Buffered;
            binding.UseDefaultWebProxy = true;
            binding.MaxReceivedMessageSize = 2147483647;
            binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;

            var endpoint = new EndpointAddress(endPointUri);

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

        public RemoteIssue CreateIssue(string token, RemoteIssue newIssue)
        {
            
            return _client.createIssue(token, newIssue);
        }

        public RemoteIssue UpdateIssue(string token, string key, RemoteFieldValue[] fields)
        {
            return _client.updateIssue(token, key, fields);
        }

        public RemoteAttachment[] GetAttachmentsFromIssue(string token, string key)
        {
            return _client.getAttachmentsFromIssue(token, key);
        }
    }
}