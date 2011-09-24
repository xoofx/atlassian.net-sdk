using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Xml;

namespace Atlassian.Jira.Remote
{
    /// <summary>
    /// Factory for JiraSoapServiceClient proxy
    /// </summary>
    public static class JiraSoapServiceClientFactory
    {
        /// <summary>
        /// Creates and configures a JiraSoapServiceClient
        /// </summary>
        /// <param name="jiraBaseUrl">Base url to JIRA server</param>
        /// <returns>JiraSoapServiceClient</returns>
        public static JiraSoapServiceClient Create(string jiraBaseUrl)
        {
            if (!jiraBaseUrl.EndsWith("/"))
            {
                jiraBaseUrl += "/";
            }

            var endPointUri = new Uri(jiraBaseUrl + "rpc/soap/jirasoapservice-v2");

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
            binding.ReaderQuotas = new XmlDictionaryReaderQuotas() { MaxStringContentLength = 2147483647 };
            binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;

            var endpoint = new EndpointAddress(endPointUri);

            return new JiraSoapServiceClient(binding, endpoint);
        }
    }
}
