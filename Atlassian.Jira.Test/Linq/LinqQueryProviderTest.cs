using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Atlassian.Jira.Linq;
using Moq;
using System.Linq.Expressions;

namespace Atlassian.Jira.Test
{
    public class LinqQueryProviderTest
    {
        private Mock<IJiraSoapServiceClient> _soapClient;

        private Jira CreateJiraInstance()
        {
            var translator = new Mock<IJqlExpressionVisitor>();
            _soapClient = new Mock<IJiraSoapServiceClient>();

            translator.Setup(t => t.Process(It.IsAny<Expression>())).Returns(new JqlData() { Expression = "dummy expression" });
            
            return new Jira(translator.Object, _soapClient.Object, null, "username", "password");
        }

        [Fact]
        public void Count()
        {
            var jira = CreateJiraInstance();
            _soapClient.Setup(r => r.GetIssuesFromJqlSearch(
                                        It.IsAny<string>(),
                                        It.IsAny<string>(),
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
                                        It.IsAny<int>())).Returns(new RemoteIssue[] 
                                        { 
                                            new RemoteIssue() { summary = "foo"}, 
                                            new RemoteIssue() 
                                        });

            Assert.Equal("foo", jira.Issues.First().Summary);
        }
    }
}
