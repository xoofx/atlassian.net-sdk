using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Atlassian.Jira.Remote;
using Moq;

namespace Atlassian.Jira.Test
{
    public class AttachmentTest
    {
        [Fact]
        public void Download_ShouldSaveAttachmentToSpecifiedLocation()
        {
            //arrange
            var mockWebClient = new Mock<IWebClient>();
            var mockSoapClient = new Mock<IJiraSoapServiceClient>();
            mockSoapClient.Setup(j => j.Url).Returns("http://foo:2990/jira/");

            var jira = new Jira(null, mockSoapClient.Object, null, "token", () => new JiraCredentials("user", "pass"));

            var attachment = (new RemoteAttachment()
            {
                id = "attachID",
                filename = "attach.txt"
            }).ToLocal(jira, mockWebClient.Object);

            //act
            attachment.Download("C:\\foo\\bar.txt");

            //assert
            mockWebClient.Verify(c => c.AddQueryString("os_username", "user"));
            mockWebClient.Verify(c => c.AddQueryString("os_password", "pass"));
            mockWebClient.Verify(c => c.Download("http://foo:2990/jira/secure/attachment/attachID/attach.txt", "C:\\foo\\bar.txt"));
        }

        [Fact]
        public void Download_IfJiraUrlDoesNotEndInSlash_ShouldFixTheUrlBeforeDownloading()
        {
            //arrange
            var mockWebClient = new Mock<IWebClient>();
            var mockSoapClient = new Mock<IJiraSoapServiceClient>();
            mockSoapClient.Setup(j => j.Url).Returns("http://foo:2990/jira");

            var jira = new Jira(null, mockSoapClient.Object, null, "token", () => new JiraCredentials("user", "pass"));

            var attachment = (new RemoteAttachment()
            {
                id = "attachID",
                filename = "attach.txt"
            }).ToLocal(jira, mockWebClient.Object);

            //act
            attachment.Download("C:\\foo\\bar.txt");

            //assert
            mockWebClient.Verify(c => c.Download("http://foo:2990/jira/secure/attachment/attachID/attach.txt", "C:\\foo\\bar.txt"));
        }
    }
}
