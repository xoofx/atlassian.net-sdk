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
        public void ToRemote_IfKeyNotSet_ShouldNotPopulateKey()
        {
            var issue = new Issue();

            Assert.Null(issue.ToRemote().key);
        }
    }
}
