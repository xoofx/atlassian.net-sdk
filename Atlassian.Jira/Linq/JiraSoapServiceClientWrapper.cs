using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Xml;

namespace Atlassian.Jira.Linq
{
    /// <summary>
    /// Wraps the auto-generated JiraSoapServiceClient proxy
    /// </summary>
    internal class JiraSoapServiceClientWrapper: IJiraSoapServiceClient
    {
        private readonly JiraSoapServiceClient _client;
        private readonly string _url;

        public JiraSoapServiceClientWrapper(string url)
        {
            if (!url.EndsWith("/"))
            {
                url += "/";
            }

            _url = url;

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
            binding.ReaderQuotas = new XmlDictionaryReaderQuotas() { MaxStringContentLength = 2147483647 } ;
            binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;

            var endpoint = new EndpointAddress(endPointUri);

            _client = new JiraSoapServiceClient(binding, endpoint);
        }

        public string Url
        {
            get
            {
                return _url;
            }
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

        public bool AddBase64EncodedAttachmentsToIssue(string token, string key, string[] fileNames, string[] base64EncodedAttachmentData)
        {
            return _client.addBase64EncodedAttachmentsToIssue(token, key, fileNames, base64EncodedAttachmentData);
        }


        public RemoteComment[] GetCommentsFromIssue(string token, string key)
        {
            return _client.getComments(token, key);
        }


        public void AddComment(string token, string key, RemoteComment comment)
        {
            _client.addComment(token, key, comment);
        }


        public RemoteIssueType[] GetIssueTypes(string token, string projectKey)
        {
            if (String.IsNullOrEmpty(null))
            {
                return _client.getIssueTypes(token);
            }
            else
            {
                return _client.getIssueTypesForProject(token, projectKey);
            }
        }

        public RemotePriority[] GetPriorities(string token)
        {
            return _client.getPriorities(token);
        }

        public RemoteResolution[] GetResolutions(string token)
        {
            return _client.getResolutions(token);
        }

        public RemoteStatus[] GetStatuses(string token)
        {
            return _client.getStatuses(token);
        }

        public RemoteVersion[] GetVersions(string token, string projectKey)
        {
            return _client.getVersions(token, projectKey);
        }
    }
}