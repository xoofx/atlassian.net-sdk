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
        public void TestQueryWithZeroResults()
        {
            var jira = new Jira("http://localhost:2990/jira", "admin", "admin");

            jira.Debug = true;

            var issues = from i in jira.IssueSearch()
                         where i.Created == new DateTime(2010,1,1)
                         select i;

            Assert.Equal(0, issues.Count());
        }

        [Fact]
        public void TestQueryWithOneResult()
        {
            var jira = new Jira("http://localhost:2990/jira", "admin", "admin");

            jira.Debug = true;

            var issues = (from i in jira.IssueSearch()
                         where i.Reporter == "admin"
                         select i).ToList();

            Assert.Equal(1, issues.Count);
            Assert.Equal("Sample bug in Test Project", issues[0].Summary);
        }
    }
}
