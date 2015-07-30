using Atlassian.Jira.Remote;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Atlassian.Jira.Test
{
    public class IssueTypeTest
    {
        public class IsSubTask
        {
            [Fact]
            public void WillReturnSubTaskFromRemote()
            {
                // Arrange
                var jira = TestableJira.Create();
                jira.SoapService.Setup(j => j.GetIssuesFromJqlSearch(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                                .Returns(new RemoteIssue[]
                                {
                                    new RemoteIssue() { id = "123", key = "mykey", type = "2", project = "myproject"  }
                                });
                jira.SoapService.Setup(s => s.GetSubTaskIssueTypes(It.IsAny<string>()))
                                .Returns(new RemoteIssueType[] {
                                    new RemoteIssueType() { id = "2", name = "SubTask", subTask = true }
                                });

                // Act
                var issue = jira.GetIssuesFromJql("JQL").First();

                // Assert
                Assert.True(issue.Type.IsSubTask);
            }

            [Fact]
            public void WillReturnFalseSubTaskIssueTypeIsNotFound()
            {
                // Arrange
                var jira = TestableJira.Create();
                jira.SoapService
                    .Setup(j => j.GetIssuesFromJqlSearch(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(new RemoteIssue[] {
                        new RemoteIssue() { id = "123", key = "mykey", type = "666", project = "myproject" }
                    });
                jira.SoapService
                    .Setup(j => j.GetSubTaskIssueTypes(It.IsAny<string>()))
                    .Returns(new RemoteIssueType[0]);

                // Act
                var issue = jira.GetIssuesFromJql("JQL").First();

                // Assert
                Assert.False(issue.Type.IsSubTask);
            }

            [Fact]
            public void WillThrowExceptionIfIssueTypeIsSetByUser()
            {
                // Arrange
                var jira = TestableJira.Create();

                // Act
                var issue = jira.CreateIssue("FOO");
                issue.Type = "Task";

                // Assert
                var ex = Assert.Throws(typeof(InvalidOperationException), () => issue.Type.IsSubTask);
                Assert.Equal("Unable to retrieve remote issue type information. This is not supported if the issue type has been set by user code.", ex.Message);
            }
        }
    }
}
