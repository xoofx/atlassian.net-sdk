using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Atlassian.Jira.Linq;
using Moq;

namespace Atlassian.Jira.Test
{
    public class LinqQueryProviderTest
    {
        private Jira CreateJiraInstance()
        {
            var translator = new Mock<IJqlExpressionTranslator>();
            var soapClient = new Mock<IJiraSoapServiceClient>();

            soapClient.Setup(r => r.GetIssuesFromJqlSearch(
                                        It.IsAny<string>(),
                                        It.IsAny<string>(),
                                        It.IsAny<int>())).Returns(new RemoteIssue[0]);

            return new Jira(translator.Object, soapClient.Object, null, "username", "password");
        }

        [Fact]
        public void HandleNonQueriableMethods()
        {
            var jira = CreateJiraInstance();

            var issues = from i in jira.Issues
                         where i.Votes == 5
                         select i;

            Assert.Equal(0, issues.Count());
        }
    }
}
