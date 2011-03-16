using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Moq;
using Atlassian.Jira.Linq;

namespace Atlassian.Jira.Linq.Test
{
    public class JqlExpressionTranslatorTest
    {
        [Fact]
        public void TranslateWhere()
        {
            //arrange
            var translator = new JqlExpressionTranslator();
            var provider = new JiraQueryProvider(translator);

            var jira = new JiraInstance(provider);

            //act
            var issues = (from i in jira.IssueSearch()
                         where i.Summary == "Foo"
                         select i).ToArray();


            //assert
            Assert.Equal("foo", translator.Jql);

        }
    }
}
