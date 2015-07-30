using Atlassian.Jira.Linq;
using Atlassian.Jira.Remote;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xunit;

namespace Atlassian.Jira.Test
{
    public class JqlQueryProviderTest
    {
        private Mock<IJiraServiceClient> _soapClient;

        private Jira CreateJiraInstance()
        {
            var translator = new Mock<IJqlExpressionVisitor>();
            _soapClient = new Mock<IJiraServiceClient>();

            translator.Setup(t => t.Process(It.IsAny<Expression>())).Returns(new JqlData() { Expression = "dummy expression" });

            return new Jira(translator.Object, _soapClient.Object, null);
        }

        [Fact]
        public void Count()
        {
            var jira = CreateJiraInstance();
            _soapClient.Setup(r => r.GetIssuesFromJqlSearch(
                                        It.IsAny<string>(),
                                        It.IsAny<string>(),
                                        It.IsAny<int>(),
                                        It.IsAny<int>())).Returns(new RemoteIssue[1] { new RemoteIssue() });

            Assert.Equal(1, jira.Issues.Count());
        }

        [Fact]
        public void First()
        {
            var jira = CreateJiraInstance();
            _soapClient.Setup(r => r.GetIssuesFromJqlSearch(
                                        It.IsAny<string>(),
                                        It.IsAny<string>(),
                                        It.IsAny<int>(),
                                        It.IsAny<int>())).Returns(new RemoteIssue[]
                                        {
                                            new RemoteIssue() { summary = "foo"},
                                            new RemoteIssue()
                                        });

            Assert.Equal("foo", jira.Issues.First().Summary);
        }
    }
}
