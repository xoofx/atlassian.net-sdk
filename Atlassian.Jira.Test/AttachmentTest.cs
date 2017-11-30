using Atlassian.Jira.Remote;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Atlassian.Jira.Test
{
    //public class AttachmentTest
    //{
    //    [Fact]
    //    public void Download_ShouldThrowExceptionIfUserAndPasswordAreMissing()
    //    {
    //        //arrange
    //        var mockWebClient = new Mock<IWebClient>();
    //        //var mockSoapClient = new Mock<IJiraSoapClient>();

    //        var jira = new Jira(null, null, new JiraCredentials("user"), "token");

    //        var attachment = (new RemoteAttachment()
    //        {
    //            id = "attachID",
    //            filename = "attach.txt"
    //        }).ToLocal(jira, mockWebClient.Object);

    //        //act
    //        Assert.Throws<InvalidOperationException>(() => attachment.Download("C:\\foo\\bar.txt"));
    //    }

    //    [Fact]
    //    public void Download_ShouldSaveAttachmentToSpecifiedLocation()
    //    {
    //        //arrange
    //        var mockWebClient = new Mock<IWebClient>();
    //        var mockSoapClient = new Mock<IJiraSoapClient>();
    //        mockSoapClient.Setup(j => j.Url).Returns("http://foo:2990/jira/");

    //        var jira = new Jira(null, mockSoapClient.Object, null, new JiraCredentials("user", "pass"), "token");

    //        var attachment = (new RemoteAttachment()
    //        {
    //            id = "attachID",
    //            filename = "attach.txt"
    //        }).ToLocal(jira, mockWebClient.Object);

    //        //act
    //        attachment.Download("C:\\foo\\bar.txt");

    //        //assert
    //        mockWebClient.Verify(c => c.AddQueryString("os_username", "user"));
    //        mockWebClient.Verify(c => c.AddQueryString("os_password", "pass"));
    //        mockWebClient.Verify(c => c.Download("http://foo:2990/jira/secure/attachment/attachID/attach.txt", "C:\\foo\\bar.txt"));
    //    }

    //    [Fact]
    //    public void Download_ShouldUriEncodeUserAndPassword()
    //    {
    //        // Arrange
    //        var mockWebClient = new Mock<IWebClient>();
    //        var mockSoapClient = new Mock<IJiraSoapClient>();
    //        mockSoapClient.Setup(j => j.Url).Returns("http://foo:2990/jira/");

    //        var jira = new Jira(null, mockSoapClient.Object, null, new JiraCredentials("my<user#with&chars", "my<pass#with&chars"), "token");

    //        var attachment = (new RemoteAttachment()
    //        {
    //            id = "attachID",
    //            filename = "attach.txt"
    //        }).ToLocal(jira, mockWebClient.Object);

    //        // Act
    //        attachment.Download("C:\\foo\\bar.txt");

    //        // Assert
    //        mockWebClient.Verify(c => c.AddQueryString("os_username", "my%3Cuser%23with%26chars"));
    //        mockWebClient.Verify(c => c.AddQueryString("os_password", "my%3Cpass%23with%26chars"));
    //        mockWebClient.Verify(c => c.Download("http://foo:2990/jira/secure/attachment/attachID/attach.txt", "C:\\foo\\bar.txt"));
    //    }

    //    [Fact]
    //    public void Download_IfJiraUrlDoesNotEndInSlash_ShouldFixTheUrlBeforeDownloading()
    //    {
    //        //arrange
    //        var mockWebClient = new Mock<IWebClient>();
    //        var mockSoapClient = new Mock<IJiraSoapClient>();
    //        mockSoapClient.Setup(j => j.Url).Returns("http://foo:2990/jira");

    //        var jira = new Jira(null, mockSoapClient.Object, null, new JiraCredentials("user", "pass"), "token");

    //        var attachment = (new RemoteAttachment()
    //        {
    //            id = "attachID",
    //            filename = "attach.txt"
    //        }).ToLocal(jira, mockWebClient.Object);

    //        //act
    //        attachment.Download("C:\\foo\\bar.txt");

    //        //assert
    //        mockWebClient.Verify(c => c.Download("http://foo:2990/jira/secure/attachment/attachID/attach.txt", "C:\\foo\\bar.txt"));
    //    }
    //}
}
