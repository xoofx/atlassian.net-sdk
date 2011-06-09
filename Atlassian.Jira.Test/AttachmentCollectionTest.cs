using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Moq;
using Atlassian.Jira.Linq;

namespace Atlassian.Jira.Test
{
    public class AttachmentCollectionTest
    {
        [Fact]
        public void Upload_IfIssueNotCreated_ShouldThrowAnException()
        {
            //arrange
            var attachments = new AttachmentCollection(null, null, null);

            //act,assert
            Assert.Throws(typeof(InvalidOperationException), () => attachments.Upload("foo"));
        }

        [Fact]
        public void Upload_IfIssueCreated_ShouldUpload()
        {
            //arrange
            var mockJiraService = new Mock<IJiraSoapServiceClient>();
            mockJiraService.Setup(j => j.Login("user", "pass")).Returns("token");
            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(f => f.FileReadAllBytes("foo.txt")).Returns(new byte[] { 1, 2, 3 });
            var jira = new Jira(mockJiraService.Object, "user", "pass");
            var attachments = new AttachmentCollection(jira, mockFileSystem.Object, "key");

            //act
            attachments.Upload("foo.txt");

            //assert
            mockJiraService.Verify(j => j.addBase64EncodedAttachmentsToIssue(
                                                "token",
                                                "key",
                                                new string[] { "foo.txt" },
                                                new string[] { "AQID"}));
        }

        [Fact]
        public void Enumerate_IfIssueIsCreatedAndHasNoAttachments_ShouldLoadZeroAttachments()
        {
            //arrange
            var mockJiraService = new Mock<IJiraSoapServiceClient>();
            var jira = new Jira(mockJiraService.Object, "user", "pass");
            var attachments = new AttachmentCollection(jira, null, "Key");

            //act, assert
            Assert.Equal(0, attachments.Count());
        }

        [Fact]
        public void Enumerate_IfIssueIsCreatedAndHasAttachments_ShouldLoadAttachments()
        {
            //arrange
            var mockJiraService = new Mock<IJiraSoapServiceClient>();
            mockJiraService.Setup(j => j.Login("user", "pass")).Returns("thetoken");
            mockJiraService.Setup(j => j.GetAttachmentsFromIssue("thetoken", "Key"))
                .Returns(new RemoteAttachment[1] { new RemoteAttachment() { filename = "attach.txt" }});
            var jira = new Jira(mockJiraService.Object, "user", "pass");
            var attachments = new AttachmentCollection(jira, null, "Key");

            //act, assert
            Assert.Equal(1, attachments.Count());
            Assert.Equal("attach.txt", attachments.First().FileName);
        }

        [Fact]
        public void Enumerate_IfIssueIsNotCreated_ShouldNotAttemptToLoad()
        {
            //arrange
            var mockJiraService = new Mock<IJiraSoapServiceClient>();
            mockJiraService.Setup(j => j.Login(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("should not contact server"));
            mockJiraService.Setup(j => j.GetAttachmentsFromIssue(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("should not contact server"));

            var jira = new Jira(mockJiraService.Object, "user", "pass");
            var attachments = new AttachmentCollection(jira,null,  null);

            //act, assert
            Assert.Equal(0, attachments.Count());
        }

        [Fact]
        public void Refresh_IfIssueNotCreated_ShouldThrowException()
        {
            var mockJiraService = new Mock<IJiraSoapServiceClient>();
            var jira = new Jira(mockJiraService.Object, "user", "pass");

            var attachments = new AttachmentCollection(jira, null, null);

            Assert.Throws(typeof(InvalidOperationException), () => attachments.Refresh());
        }
    }
}
