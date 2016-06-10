using Atlassian.Jira.Linq;
using Atlassian.Jira.Remote;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;

namespace Atlassian.Jira.Test
{
    public class JqlExpressionTranslatorTest
    {
        private JqlExpressionVisitor _translator;

        private JiraQueryable<Issue> CreateQueryable()
        {

            _translator = new JqlExpressionVisitor();

            var jira = Jira.CreateRestClient();
            var issues = new Mock<IIssueService>();
            var provider = new JiraQueryProvider(_translator, issues.Object);

            issues.SetupIssues(jira, new RemoteIssue());

            return new JiraQueryable<Issue>(provider);
        }

        [Fact]
        public void EqualsOperatorForNonString()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Votes == 5
                          select i).ToArray();

            Assert.Equal("Votes = 5", _translator.Jql);
        }

        [Fact]
        public void EqualsOperatorForStringWithFuzzyEquality()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Summary == "Foo"
                          select i).ToArray();

            Assert.Equal("Summary ~ \"Foo\"", _translator.Jql);
        }

        [Fact]
        public void EqualsOperatorForString()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Assignee == "Foo"
                          select i).ToArray();

            Assert.Equal("Assignee = \"Foo\"", _translator.Jql);
        }

        [Fact]
        public void NotEqualsOperatorForNonString()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Votes != 5
                          select i).ToArray();

            Assert.Equal("Votes != 5", _translator.Jql);
        }

        [Fact]
        public void NotEqualsOperatorForStringWithFuzzyEquality()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Summary != "Foo"
                          select i).ToArray();

            Assert.Equal("Summary !~ \"Foo\"", _translator.Jql);
        }

        [Fact]
        public void NotEqualsOperatorForString()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Assignee != "Foo"
                          select i).ToArray();

            Assert.Equal("Assignee != \"Foo\"", _translator.Jql);
        }

        [Fact]
        public void GreaterThanOperator()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Votes > 5
                          select i).ToArray();

            Assert.Equal("Votes > 5", _translator.Jql);
        }

        [Fact]
        public void GreaterThanEqualsOperator()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Votes >= 5
                          select i).ToArray();

            Assert.Equal("Votes >= 5", _translator.Jql);
        }

        [Fact]
        public void LessThanOperator()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Votes < 5
                          select i).ToArray();

            Assert.Equal("Votes < 5", _translator.Jql);
        }

        [Fact]
        public void LessThanOrEqualsOperator()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Votes <= 5
                          select i).ToArray();

            Assert.Equal("Votes <= 5", _translator.Jql);
        }

        [Fact]
        public void AndKeyWord()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Votes > 5 && i.Votes < 10
                          select i).ToArray();

            Assert.Equal("(Votes > 5 and Votes < 10)", _translator.Jql);
        }

        [Fact]
        public void OrKeyWord()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Votes > 5 || i.Votes < 10
                          select i).ToArray();

            Assert.Equal("(Votes > 5 or Votes < 10)", _translator.Jql);
        }

        [Fact]
        public void AssociativeGrouping()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Votes > 5 && (i.Votes < 10 || i.Votes == 20)
                          select i).ToArray();

            Assert.Equal("(Votes > 5 and (Votes < 10 or Votes = 20))", _translator.Jql);
        }

        [Fact]
        public void IsOperatorForEmptyString()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Summary == ""
                          select i).ToArray();

            Assert.Equal("Summary is empty", _translator.Jql);
        }

        [Fact]
        public void IsNotOperatorForEmptyString()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Summary != ""
                          select i).ToArray();

            Assert.Equal("Summary is not empty", _translator.Jql);
        }

        [Fact]
        public void IsOperatorForNull()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Summary == null
                          select i).ToArray();

            Assert.Equal("Summary is null", _translator.Jql);
        }

        [Fact]
        public void GreaterThanOperatorWhenUsingComparableFieldWithString()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Priority > "foo"
                          select i).ToArray();

            Assert.Equal("Priority > \"foo\"", _translator.Jql);
        }

        [Fact]
        public void EqualsOperatorWhenUsingComparableFieldWithString()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Priority == "foo"
                          select i).ToArray();

            Assert.Equal("Priority = \"foo\"", _translator.Jql);
        }

        [Fact]
        public void OrderBy()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Priority == "1"
                          orderby i.Created
                          select i).ToArray();

            Assert.Equal("Priority = \"1\" order by Created asc", _translator.Jql);
        }

        [Fact]
        public void OrderByDescending()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Priority == "1"
                          orderby i.Created descending
                          select i).ToArray();

            Assert.Equal("Priority = \"1\" order by Created desc", _translator.Jql);
        }

        [Fact]
        public void MultipleOrderBys()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Priority == "1"
                          orderby i.Created, i.DueDate
                          select i).ToArray();

            Assert.Equal("Priority = \"1\" order by Created asc, DueDate asc", _translator.Jql);
        }

        [Fact]
        public void MultipleOrderByDescending()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Priority == "1"
                          orderby i.Created, i.DueDate descending
                          select i).ToArray();

            Assert.Equal("Priority = \"1\" order by Created asc, DueDate desc", _translator.Jql);
        }

        [Fact]
        public void NewDateTime()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Created > new DateTime(2011, 1, 1)
                          select i).ToArray();

            Assert.Equal("Created > \"2011/01/01\"", _translator.Jql);
        }

        [Fact]
        public void MultipleDateTimes()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Created > new DateTime(2011, 1, 1) && i.Created < new DateTime(2012, 1, 1)
                          select i).ToArray();

            Assert.Equal("(Created > \"2011/01/01\" and Created < \"2012/01/01\")", _translator.Jql);
        }

        [Fact]
        public void LocalStringVariables()
        {
            var queryable = CreateQueryable();
            var user = "farmas";

            var issues = (from i in queryable
                          where i.Assignee == user
                          select i).ToArray();

            Assert.Equal("Assignee = \"farmas\"", _translator.Jql);
        }

        [Fact]
        public void LocalDateVariables()
        {
            var queryable = CreateQueryable();
            var date = new DateTime(2011, 1, 1);

            var issues = (from i in queryable
                          where i.Created > date
                          select i).ToArray();

            Assert.Equal("Created > \"2011/01/01\"", _translator.Jql);
        }

        [Fact]
        public void DateTimeWithLiteralString()
        {
            var queryable = CreateQueryable();
            var date = new DateTime(2011, 1, 1);

            var issues = (from i in queryable
                          where i.Created > new LiteralDateTime(date.ToString("yyyy/MM/dd HH:mm"))
                          select i).ToArray();

            Assert.Equal("Created > \"2011/01/01 00:00\"", _translator.Jql);
        }

        [Fact]
        // https://bitbucket.org/farmas/atlassian.net-sdk/issue/31
        public void DateTimeFormattedAsEnUs()
        {
            var currentCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
                var queryable = CreateQueryable();
                var date = new DateTime(2011, 1, 1);

                var issues = (from i in queryable
                              where i.Created > date
                              select i).ToArray();

                Assert.Equal("Created > \"2011/01/01\"", _translator.Jql);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = currentCulture;
            }
        }

        [Fact]
        public void DateTimeNow()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Created > DateTime.Now
                          select i).ToArray();

            Assert.Equal("Created > \"" + DateTime.Now.ToString("yyyy/MM/dd") + "\"", _translator.Jql);
        }

        [Fact]
        public void TakeWithConstant()
        {
            var queryable = CreateQueryable();

            var issues = (from i in queryable
                          where i.Assignee == "foo"
                          select i).Take(50).ToArray();

            Assert.Equal(50, _translator.NumberOfResults);
        }

        [Fact]
        public void TakeWithLocalVariable()
        {
            var queryable = CreateQueryable();
            var take = 100;

            var issues = (from i in queryable
                          where i.Assignee == "foo"
                          select i).Take(take).ToArray();

            Assert.Equal(100, _translator.NumberOfResults);
        }

        [Fact]
        public void VersionsEqual()
        {
            var queryable = CreateQueryable();
            var issues = (from i in queryable
                          where i.FixVersions == "1.0" && i.AffectsVersions == "2.0"
                          select i).ToArray();

            Assert.Equal("(FixVersion = \"1.0\" and AffectedVersion = \"2.0\")", _translator.Jql);
        }

        [Fact]
        public void ComponentEqual()
        {
            var queryable = CreateQueryable();
            var issues = (from i in queryable
                          where i.Components == "foo"
                          select i).ToArray();

            Assert.Equal("component = \"foo\"", _translator.Jql);
        }

        [Fact]
        public void VersionsNotEqual()
        {
            var queryable = CreateQueryable();
            var issues = (from i in queryable
                          where i.FixVersions != "1.0" && i.AffectsVersions != "2.0"
                          select i).ToArray();

            Assert.Equal("(FixVersion != \"1.0\" and AffectedVersion != \"2.0\")", _translator.Jql);
        }

        [Fact]
        public void ComponentNotEqual()
        {
            var queryable = CreateQueryable();
            var issues = (from i in queryable
                          where i.Components != "foo"
                          select i).ToArray();

            Assert.Equal("component != \"foo\"", _translator.Jql);
        }

        [Fact]
        public void CustomFieldEqual()
        {
            var queryable = CreateQueryable();
            var issues = (from i in queryable
                          where i["Foo"] == "foo" && i["Bar"] == new DateTime(2012, 1, 1) && i["Baz"] == new LiteralMatch("baz")
                          select i).ToArray();

            Assert.Equal("((\"Foo\" ~ \"foo\" and \"Bar\" = \"2012/01/01\") and \"Baz\" = \"baz\")", _translator.Jql);
        }

        [Fact]
        public void CustomFieldNotEqual()
        {
            var queryable = CreateQueryable();
            var issues = (from i in queryable
                          where i["Foo"] != "foo" && i["Bar"] != new DateTime(2012, 1, 1) && i["Baz"] != new LiteralMatch("baz")
                          select i).ToArray();

            Assert.Equal("((\"Foo\" !~ \"foo\" and \"Bar\" != \"2012/01/01\") and \"Baz\" != \"baz\")", _translator.Jql);
        }

        [Fact]
        public void CustomFieldGreaterThan()
        {
            var queryable = CreateQueryable();
            var issues = (from i in queryable
                          where i["Foo"] > "foo" && i["Bar"] > new DateTime(2012, 1, 1)
                          select i).ToArray();

            Assert.Equal("(\"Foo\" > \"foo\" and \"Bar\" > \"2012/01/01\")", _translator.Jql);
        }

        [Fact]
        public void MultipleSeparateWheres()
        {
            var queryable = CreateQueryable();

            var issues = from i in queryable
                         where i.Votes == 5
                         select i;

            issues = from i in issues
                     where i.Status == "Open" && i.Assignee == "admin"
                     select i;

            issues = from i in issues
                     where i.Priority == "1"
                     select i;

            var issuesArray = issues.ToArray();

            Assert.Equal("Votes = 5 and (Status = \"Open\" and Assignee = \"admin\") and Priority = \"1\"", _translator.Jql);
        }
    }
}
