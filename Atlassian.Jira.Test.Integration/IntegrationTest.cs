using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class IntegrationTest
    {
        [Fact]
        public void TestSimpleQuery()
        {
            var jira = new Jira("http://localhost:2990/jira", "admin", "admin");

            jira.Debug = true;

            var issues = from i in jira.IssueSearch()
                         where i.Assignee == "admin"
                         select i;

            Assert.Equal(0, issues.Count());
        }
    }
}
