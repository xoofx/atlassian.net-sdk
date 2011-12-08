using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Xml;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.IO;

namespace Atlassian.Jira.Remote
{
    public class RemoteWorklogMessageInspector : IClientMessageInspector
    {
        public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            if(reply.ToString().Contains("addWorklogAndAutoAdjustRemainingEstimate"))
            {
                var memoryStream = new MemoryStream();
                var writer = XmlWriter.Create(memoryStream);
                reply.WriteMessage(writer);
                writer.Flush();

                memoryStream.Position = 0;
                var doc = new XmlDocument();
                doc.Load(memoryStream);

                UpdateMessage(doc);

                memoryStream.SetLength(0);
                writer = XmlWriter.Create(memoryStream);
                doc.WriteTo(writer);
                writer.Flush();

                memoryStream.Position = 0;
                var reader = XmlReader.Create(memoryStream);
                reply = Message.CreateMessage(reader, int.MaxValue, reply.Version);

                //var newReply = Message.CreateMessage(reply.Version, null);
                //newReply.Headers.CopyHeadersFrom(reply);
                //newReply.Properties.CopyProperties(reply.Properties);

            }   
        }

        private void UpdateMessage(XmlDocument doc)
        {
            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
            var multiRefElement = doc.SelectSingleNode("//soapenv:Body/multiRef", ns) as XmlElement;
            multiRefElement.SetAttribute("xsi:type", "ns2:RemoteWorklog");
            multiRefElement.SetAttribute("xmlns:ns2", "http://beans.soap.rpc.jira.atlassian.com");
        }

        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel)
        {
            return null;
        }
    }

    public class RemoteWorklogCustomBehavior : IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(new RemoteWorklogMessageInspector());
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }
    }



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
            var jiraSoapServiceClient = new JiraSoapServiceClient(binding, endpoint);

            jiraSoapServiceClient.Endpoint.Behaviors.Add(new RemoteWorklogCustomBehavior());

            return jiraSoapServiceClient;
        }
    }
}
