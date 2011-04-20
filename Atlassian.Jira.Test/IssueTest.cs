using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Atlassian.Jira.Linq;

namespace Atlassian.Jira.Test
{
    public class IssueTest
    {
        [Fact]
        public void ToRemote_IfKeyIsSet_ShouldPopulateKey()
        {
            var issue = new Issue() { Key = "key1" };

            Assert.Equal("key1", issue.ToRemote().key);
        }

        [Fact]
        public void ToRemote_IfPriorityIsSet_ShouldPopulate()
        {
            var issue = new Issue() { Priority = "High" };

            Assert.Equal("High", issue.ToRemote().priority);
        }

        [Fact]
        public void ToRemote_IfResolutionIsSet_ShouldPopulate()
        {
            var issue = new Issue() { Resolution = "Fixed" };

            Assert.Equal("Fixed", issue.ToRemote().resolution);
        }

        [Fact]
        public void ToRemote_IfNothingSet_ShouldNotPopulate()
        {
            var issue = new Issue();

            Assert.Null(issue.ToRemote().key);
            Assert.Null(issue.ToRemote().priority);
        }

        [Fact]
        public void GetUpdatedFields_ReturnEmptyIfNothingChanged()
        {
            var issue = new Issue();

            Assert.Equal(0, issue.GetUpdatedFields().Length);
        }

        [Fact]
        public void GetUpdatedFields_ReturnFieldThatChanged()
        {
            var issue = new Issue();
            issue.Summary = "foo";

            Assert.Equal(1, issue.GetUpdatedFields().Length);
        }
    }
}
