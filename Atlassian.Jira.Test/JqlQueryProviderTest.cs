using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Atlassian.Jira.Remote;
using Moq;
using System.Linq.Expressions;
using Atlassian.Jira.Linq;

namespace Atlassian.Jira.Test
{
    public class JqlQueryProviderTest
    {
        private Mock<IJiraSoapServiceClient> _soapClient;
        private Mock<IJqlExpressionVisitor> _visitor;

        private Jira CreateJiraInstance()
        {
            _visitor = new Mock<IJqlExpressionVisitor>();
            _soapClient = new Mock<IJiraSoapServiceClient>();

            _visitor.Setup(t => t.Process(It.IsAny<Expression>())).Returns(new JqlData() { Expression = "dummy expression" });

            return new Jira(_visitor.Object, _soapClient.Object, null, "username", "password");
        }

        [Fact]
        public void Count_WithSoap()
        {
            var jira = CreateJiraInstance();
            _soapClient.Setup(r => r.GetIssuesFromJqlSearch(
                                        It.IsAny<string>(),
                                        It.IsAny<string>(),
                                        It.IsAny<int>())).Returns(new RemoteIssue[1] { new RemoteIssue() });

            Assert.Equal(1, jira.Issues.Count());
        }

        [Fact]
        public void Count_WithRest()
        {
            var jira = CreateJiraInstance();
            _visitor.Setup(t => t.Process(It.IsAny<Expression>())).Returns(new JqlData() { Expression = "dummy expression", ProcessCount = true  });
            jira.UseRestApi = true;
            _soapClient.Setup(r => r.GetIssueCountFromJqlSearch(It.IsAny<string>())).Returns(20);
            
            Assert.Equal(20, jira.Issues.Count());
        }

        [Fact]
        public void Skip_WithRest()
        {
            var jira = CreateJiraInstance();
            _visitor.Setup(t => t.Process(It.IsAny<Expression>())).Returns(new JqlData() { Expression = "dummy expression", StartAt = 1, MaxResults = 1 });
            _soapClient.Setup(s => s.GetJsonFromJqlSearch(It.IsAny<string>(), 1, 1, null)).Returns("{issues:[]}");

            jira.UseRestApi = true;

            jira.Issues.Skip(1).FirstOrDefault();

            _soapClient.Verify(s => s.GetJsonFromJqlSearch(It.IsAny<string>(), 1, 1, null));
        }

        [Fact]
        public void Skip_WithSoap_ShouldThrowException()
        {
            var jira = CreateJiraInstance();
            _visitor.Setup(t => t.Process(It.IsAny<Expression>())).Returns(new JqlData() { Expression = "dummy expression", StartAt = 1 });

            Assert.Throws(typeof(InvalidOperationException), () => jira.Issues.Skip(1).FirstOrDefault());
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
