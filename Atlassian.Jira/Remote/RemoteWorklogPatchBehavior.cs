using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.IO;
using System.Xml;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace Atlassian.Jira.Remote
{
    internal class RemoteWorklogPatchBehavior : IEndpointBehavior
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

        private class RemoteWorklogMessageInspector : IClientMessageInspector
        {
            private static string _correlationState = "worklog";

            public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
            {
                if(correlationState != null 
                    && String.Equals(correlationState, _correlationState)
                    && !reply.ToString().Contains("<soapenv:Fault>"))
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
                }
            }

            private void UpdateMessage(XmlDocument doc)
            {
                var ns = new XmlNamespaceManager(doc.NameTable);
                ns.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
                foreach(XmlElement multiRefElement in doc.SelectNodes("//soapenv:Body/multiRef", ns))
                {
                    multiRefElement.SetAttribute("xsi:type", "ns2:RemoteWorklog");
                    multiRefElement.SetAttribute("xmlns:ns2", "http://beans.soap.rpc.jira.atlassian.com");
                }
            }

            public object BeforeSendRequest(ref Message request, IClientChannel channel)
            {
                var requestContent = request.ToString();

                if (requestContent.Contains("addWorklogAndAutoAdjustRemainingEstimate")
                    || requestContent.Contains("addWorklogAndRetainRemainingEstimate")
                    || requestContent.Contains("addWorklogWithNewRemainingEstimate")
                    || requestContent.Contains("getWorklogs"))
                {
                    return _correlationState;
                }
                return null;
            }
        }
    }

}
