using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class SoapTests : BaseIntegrationTest
    {
#if SOAP
        [Fact]
        // Access token is only available in SOAP API.
        void WithAccessTokenInsteadOfUserAndPassword()
        {
            // get access token for user
            var accessToken = _jira.GetAccessToken();

            // create a new jira instance using access token only
            var jiraAccessToken = Jira.CreateSoapClient(HOST, accessToken, new JiraCredentials(null));

            // create and query issues
            var summaryValue = "Test Summary from JIRA with access token " + _random.Next(int.MaxValue);
            var issue = new Issue(jiraAccessToken, "TST")
            {
                Type = "1",
                Summary = summaryValue,
                Assignee = "admin"
            };
            issue.SaveChanges();

            var issues = (from i in jiraAccessToken.Issues
                          where i.Key == issue.Key
                          select i).ToArray();

            Assert.Equal(1, issues.Count());
        }
#endif
    }
}
