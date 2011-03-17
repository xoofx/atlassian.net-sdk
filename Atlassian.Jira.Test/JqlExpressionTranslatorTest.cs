using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Atlassian.Jira.Linq;
using Moq;

namespace Atlassian.Jira.Test
{

    /*
     * if its a text field it should use ~ operator instead of =, and !~ instead of != 
     * 
     * */
    public class JqlExpressionTranslatorTest
    {
        private JqlExpressionTranslator _translator;

        private JiraInstance CreateJiraInstance()
        {
            _translator = new JqlExpressionTranslator();
            var remoteService = new Mock<IJiraRemoteService>();
            var provider = new JiraQueryProvider(_translator, remoteService.Object);

            return new JiraInstance(provider);

        }

        [Fact]
        public void TranslateEqualsOperatorForNonString()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                         where i.Votes == 5
                         select i).ToArray();


            Assert.Equal("Votes = 5", _translator.Jql);
        }

        [Fact]
        public void TranslateEqualsOperatorForString()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Summary == "Foo"
                          select i).ToArray();


            Assert.Equal("Summary ~ \"Foo\"", _translator.Jql);
        }

        [Fact]
        public void TranslateNotEqualsOperatorForNonString()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Votes != 5
                          select i).ToArray();


            Assert.Equal("Votes != 5", _translator.Jql);
        }

        [Fact]
        public void TranslateNotEqualsOperatorForString()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Summary != "Foo"
                          select i).ToArray();


            Assert.Equal("Summary !~ \"Foo\"", _translator.Jql);
        }

        [Fact]
        public void TranslateGreaterThanOperator()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Votes > 5
                          select i).ToArray();


            Assert.Equal("Votes > 5", _translator.Jql);
        }

        [Fact]
        public void TranslateGreaterThanEqualsOperator()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Votes >= 5
                          select i).ToArray();

            Assert.Equal("Votes >= 5", _translator.Jql);
        }

        [Fact]
        public void TranslateLessThanOperator()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Votes < 5
                          select i).ToArray();

            Assert.Equal("Votes < 5", _translator.Jql);
        }

        [Fact]
        public void TranslateLessThanOrEqualsOperator()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Votes <= 5
                          select i).ToArray();

            Assert.Equal("Votes <= 5", _translator.Jql);
        }

        [Fact]
        public void TranslateAndKeyWord()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Votes > 5 && i.Votes < 10 
                          select i).ToArray();

            Assert.Equal("(Votes > 5 and Votes < 10)", _translator.Jql);
        }

        [Fact]
        public void TranslateOrKeyWord()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Votes > 5 || i.Votes < 10
                          select i).ToArray();

            Assert.Equal("(Votes > 5 or Votes < 10)", _translator.Jql);
        }

        [Fact]
        public void TranslateAssociativeGrouping()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Votes > 5 && (i.Votes < 10 || i.Votes == 20)
                          select i).ToArray();

            Assert.Equal("(Votes > 5 and (Votes < 10 or Votes = 20))", _translator.Jql);
        }

        [Fact]
        public void TranslateIsOperatorForEmptyString()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Summary == ""
                          select i).ToArray();

            Assert.Equal("Summary is empty", _translator.Jql);
        }

        [Fact]
        public void TranslateIsNotOperatorForEmptyString()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Summary != ""
                          select i).ToArray();

            Assert.Equal("Summary is not empty", _translator.Jql);
        }

        [Fact]
        public void TranslateIsOperatorForNull()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Summary == null
                          select i).ToArray();

            Assert.Equal("Summary is null", _translator.Jql);
        }
    }
}
