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
        private Jira CreateJiraInstance()
        {
            var translator = new Mock<IJqlExpressionVisitor>();
            var soapClient = new Mock<IJiraSoapServiceClient>();

            translator.Setup(t => t.Process(It.IsAny<Expression>())).Returns(new JqlData() { Expression = "dummy expression" });
            soapClient.Setup(r => r.GetIssuesFromJqlSearch(
                                        It.IsAny<string>(),
                                        It.IsAny<string>(),
                                        It.IsAny<int>())).Returns(new RemoteIssue[1] { new RemoteIssue() });

            return new Jira(translator.Object, soapClient.Object, null, "username", "password");
        }

        [Fact]
        public void HandleNonQueriableMethods()
        {
            var jira = CreateJiraInstance();

            var issues = from i in jira.Issues
                         where i.Votes == 5
                         select i;

            Assert.Equal(1, issues.Count());
        }
    }
}
