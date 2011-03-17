using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Atlassian.Jira.Linq;
using Moq;

namespace Atlassian.Jira.Test
{
    public class JqlExpressionTranslatorTest
    {
        private JqlExpressionTranslator _translator;

        private JiraInstance CreateJiraInstance()
        {
            _translator = new JqlExpressionTranslator();
            var remoteService = new Mock<IJiraRemoteService>();
            remoteService.Setup(r => r.GetIssuesFromJql(It.IsAny<string>())).Returns(new List<Issue>());
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
        public void TranslateEqualsOperatorForStringWithFuzzyEquality()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Summary == "Foo"
                          select i).ToArray();


            Assert.Equal("Summary ~ \"Foo\"", _translator.Jql);
        }

        [Fact]
        public void TranslateEqualsOperatorForString()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Assignee == "Foo"
                          select i).ToArray();


            Assert.Equal("Assignee = \"Foo\"", _translator.Jql);
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
        public void TranslateNotEqualsOperatorForStringWithFuzzyEquality()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Summary != "Foo"
                          select i).ToArray();

            Assert.Equal("Summary !~ \"Foo\"", _translator.Jql);
        }

        [Fact]
        public void TranslateNotEqualsOperatorForString()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Assignee != "Foo"
                          select i).ToArray();

            Assert.Equal("Assignee != \"Foo\"", _translator.Jql);
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

        [Fact]
        public void TranslateGreaterThanOperatorWhenUsingComparableFieldWithString()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Priority > "foo"
                          select i).ToArray();

            Assert.Equal("Priority > \"foo\"", _translator.Jql);
        }

        [Fact]
        public void TranslateEqualsOperatorWhenUsingComparableFieldWithString()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Priority == "foo"
                          select i).ToArray();

            Assert.Equal("Priority = \"foo\"", _translator.Jql);
        }

        [Fact]
        public void TranslateGreaterThanOperatorWhenUsingComparableFieldWithInt()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Priority > 1
                          select i).ToArray();

            Assert.Equal("Priority > 1", _translator.Jql);
        }

        [Fact]
        public void TranslateEqualsOperatorWhenUsingComparableFieldWithInt()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Priority == 1
                          select i).ToArray();

            Assert.Equal("Priority = 1", _translator.Jql);
        }

        [Fact]
        public void TranslateOrderBy()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Priority == 1
                          orderby i.Created
                          select i).ToArray();

            Assert.Equal("Priority = 1 order by Created", _translator.Jql);
        }

        [Fact]
        public void TranslateOrderByDescending()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Priority == 1
                          orderby i.Created descending
                          select i).ToArray();

            Assert.Equal("Priority = 1 order by Created desc", _translator.Jql);
        }

        [Fact]
        public void TranslateMultipleOrderBys()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Priority == 1
                          orderby i.Created, i.DueDate
                          select i).ToArray();

            Assert.Equal("Priority = 1 order by Created, DueDate", _translator.Jql);
        }

        [Fact]
        public void TranslateMultipleOrderByDescending()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.IssueSearch()
                          where i.Priority == 1
                          orderby i.Created, i.DueDate descending
                          select i).ToArray();

            Assert.Equal("Priority = 1 order by Created, DueDate desc", _translator.Jql);
        }
    }
}
