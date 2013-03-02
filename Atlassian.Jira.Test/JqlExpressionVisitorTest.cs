using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Linq;
using Atlassian.Jira.Remote;
using Moq;
using Xunit;

namespace Atlassian.Jira.Test
{
    public class JqlExpressionTranslatorTest
    {
        private JqlExpressionVisitor _translator;

        private Jira CreateJiraInstance()
        {
            _translator = new JqlExpressionVisitor();
            var soapClient = new Mock<IJiraSoapServiceClient>();

            soapClient.Setup(r => r.GetIssuesFromJqlSearch(
                                        It.IsAny<string>(),
                                        It.IsAny<string>(),
                                        It.IsAny<int>())).Returns(new RemoteIssue[0]);

            return new Jira(_translator, soapClient.Object, null, "username", "password");
        }

        [Fact]
        public void EqualsOperatorForNonString()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                         where i.Votes == 5
                         select i).ToArray();


            Assert.Equal("Votes = 5", _translator.Jql);
        }

        [Fact]
        public void EqualsOperatorForStringWithFuzzyEquality()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Summary == "Foo"
                          select i).ToArray();


            Assert.Equal("Summary ~ \"Foo\"", _translator.Jql);
        }

        [Fact]
        public void EqualsOperatorForString()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Assignee == "Foo"
                          select i).ToArray();


            Assert.Equal("Assignee = \"Foo\"", _translator.Jql);
        }



        [Fact]
        public void NotEqualsOperatorForNonString()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Votes != 5
                          select i).ToArray();


            Assert.Equal("Votes != 5", _translator.Jql);
        }

        [Fact]
        public void NotEqualsOperatorForStringWithFuzzyEquality()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Summary != "Foo"
                          select i).ToArray();

            Assert.Equal("Summary !~ \"Foo\"", _translator.Jql);
        }

        [Fact]
        public void NotEqualsOperatorForString()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Assignee != "Foo"
                          select i).ToArray();

            Assert.Equal("Assignee != \"Foo\"", _translator.Jql);
        }

        [Fact]
        public void GreaterThanOperator()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Votes > 5
                          select i).ToArray();


            Assert.Equal("Votes > 5", _translator.Jql);
        }

        [Fact]
        public void GreaterThanEqualsOperator()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Votes >= 5
                          select i).ToArray();

            Assert.Equal("Votes >= 5", _translator.Jql);
        }

        [Fact]
        public void LessThanOperator()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Votes < 5
                          select i).ToArray();

            Assert.Equal("Votes < 5", _translator.Jql);
        }

        [Fact]
        public void LessThanOrEqualsOperator()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Votes <= 5
                          select i).ToArray();

            Assert.Equal("Votes <= 5", _translator.Jql);
        }

        [Fact]
        public void AndKeyWord()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Votes > 5 && i.Votes < 10 
                          select i).ToArray();

            Assert.Equal("(Votes > 5 and Votes < 10)", _translator.Jql);
        }

        [Fact]
        public void OrKeyWord()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Votes > 5 || i.Votes < 10
                          select i).ToArray();

            Assert.Equal("(Votes > 5 or Votes < 10)", _translator.Jql);
        }

        [Fact]
        public void AssociativeGrouping()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Votes > 5 && (i.Votes < 10 || i.Votes == 20)
                          select i).ToArray();

            Assert.Equal("(Votes > 5 and (Votes < 10 or Votes = 20))", _translator.Jql);
        }

        [Fact]
        public void IsOperatorForEmptyString()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Summary == ""
                          select i).ToArray();

            Assert.Equal("Summary is empty", _translator.Jql);
        }

        [Fact]
        public void IsNotOperatorForEmptyString()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Summary != ""
                          select i).ToArray();

            Assert.Equal("Summary is not empty", _translator.Jql);
        }

        [Fact]
        public void IsOperatorForNull()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Summary == null
                          select i).ToArray();

            Assert.Equal("Summary is null", _translator.Jql);
        }

        [Fact]
        public void GreaterThanOperatorWhenUsingComparableFieldWithString()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Priority > "foo"
                          select i).ToArray();

            Assert.Equal("Priority > \"foo\"", _translator.Jql);
        }

        [Fact]
        public void EqualsOperatorWhenUsingComparableFieldWithString()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Priority == "foo"
                          select i).ToArray();

            Assert.Equal("Priority = \"foo\"", _translator.Jql);
        }

        

        [Fact]
        public void OrderBy()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Priority == "1"
                          orderby i.Created
                          select i).ToArray();

            Assert.Equal("Priority = \"1\" order by Created", _translator.Jql);
        }

        [Fact]
        public void OrderByDescending()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Priority == "1"
                          orderby i.Created descending
                          select i).ToArray();

            Assert.Equal("Priority = \"1\" order by Created desc", _translator.Jql);
        }

        [Fact]
        public void MultipleOrderBys()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Priority == "1"
                          orderby i.Created, i.DueDate
                          select i).ToArray();

            Assert.Equal("Priority = \"1\" order by Created, DueDate", _translator.Jql);
        }

        [Fact]
        public void MultipleOrderByDescending()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Priority == "1"
                          orderby i.Created, i.DueDate descending
                          select i).ToArray();

            Assert.Equal("Priority = \"1\" order by Created, DueDate desc", _translator.Jql);
        }

        [Fact]
        public void NewDateTime()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Created > new DateTime(2011,1,1)
                          select i).ToArray();

            Assert.Equal("Created > \"2011/01/01\"", _translator.Jql);
        }

        [Fact]
        public void MultipleDateTimes()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Created > new DateTime(2011, 1, 1) && i.Created < new DateTime(2012,1,1) 
                          select i).ToArray();

            Assert.Equal("(Created > \"2011/01/01\" and Created < \"2012/01/01\")", _translator.Jql);
        }

        [Fact]
        public void LocalStringVariables()
        {
            var jira = CreateJiraInstance();
            var user = "farmas";

            var issues = (from i in jira.Issues
                          where i.Assignee == user
                          select i).ToArray();

            Assert.Equal("Assignee = \"farmas\"", _translator.Jql);
        }

        [Fact]
        public void LocalDateVariables()
        {
            var jira = CreateJiraInstance();
            var date = new DateTime(2011, 1, 1);

            var issues = (from i in jira.Issues
                          where i.Created >  date
                          select i).ToArray();

            Assert.Equal("Created > \"2011/01/01\"", _translator.Jql);
        }

        [Fact]
        public void DateTimeWithLiteralString()
        {
            var jira = CreateJiraInstance();
            var date = new DateTime(2011, 1, 1);

            var issues = (from i in jira.Issues
                          where i.Created > new LiteralDateTime(date.ToString("yyyy/MM/dd HH:mm"))
                          select i).ToArray();

            Assert.Equal("Created > \"2011/01/01 00:00\"", _translator.Jql);
        }

        [Fact]
        public void DateTimeNow()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Created > DateTime.Now
                          select i).ToArray();

            Assert.Equal("Created > \"" + DateTime.Now.ToString("yyyy/MM/dd") + "\"", _translator.Jql);
        }

        [Fact]
        public void TakeWithConstant()
        {
            var jira = CreateJiraInstance();

            var issues = (from i in jira.Issues
                          where i.Assignee == "foo"
                          select i).Take(50).ToArray();

            Assert.Equal(50, _translator.NumberOfResults);
        }

        [Fact]
        public void TakeWithLocalVariable()
        {
            var jira = CreateJiraInstance();
            var take = 100;

            var issues = (from i in jira.Issues
                          where i.Assignee == "foo"
                          select i).Take(take).ToArray();

            Assert.Equal(100, _translator.NumberOfResults);
        }

        [Fact]
        public void VersionsEqual()
        {
            var jira = CreateJiraInstance();
            var issues = (from i in jira.Issues
                          where i.FixVersions == "1.0" && i.AffectsVersions == "2.0"
                          select i).ToArray();

            Assert.Equal("(FixVersion = \"1.0\" and AffectedVersion = \"2.0\")", _translator.Jql);
        }

        [Fact]
        public void ComponentEqual()
        {
            var jira = CreateJiraInstance();
            var issues = (from i in jira.Issues
                          where i.Components == "foo"
                          select i).ToArray();

            Assert.Equal("component = \"foo\"", _translator.Jql);
        }

        [Fact]
        public void VersionsNotEqual()
        {
            var jira = CreateJiraInstance();
            var issues = (from i in jira.Issues
                          where i.FixVersions != "1.0" && i.AffectsVersions != "2.0"
                          select i).ToArray();

            Assert.Equal("(FixVersion != \"1.0\" and AffectedVersion != \"2.0\")", _translator.Jql);
        }

        [Fact]
        public void ComponentNotEqual()
        {
            var jira = CreateJiraInstance();
            var issues = (from i in jira.Issues
                          where i.Components != "foo"
                          select i).ToArray();

            Assert.Equal("component != \"foo\"", _translator.Jql);
        }

        [Fact]
        public void CustomFieldEqual()
        {
            var jira = CreateJiraInstance();
            var issues = (from i in jira.Issues
                          where i["Foo"] == "foo" && i["Bar"] == new DateTime(2012,1,1) && i["Baz"] == new LiteralMatch("baz")
                          select i).ToArray();

            Assert.Equal("((\"Foo\" ~ \"foo\" and \"Bar\" = \"2012/01/01\") and \"Baz\" = \"baz\")", _translator.Jql);
        }

        [Fact]
        public void CustomFieldNotEqual()
        {
            var jira = CreateJiraInstance();
            var issues = (from i in jira.Issues
                          where i["Foo"] != "foo" && i["Bar"] != new DateTime(2012,1,1) && i["Baz"] != new LiteralMatch("baz")
                          select i).ToArray();

            Assert.Equal("((\"Foo\" !~ \"foo\" and \"Bar\" != \"2012/01/01\") and \"Baz\" != \"baz\")", _translator.Jql);
        }

        [Fact]
        public void CustomFieldGreaterThan()
        {
            var jira = CreateJiraInstance();
            var issues = (from i in jira.Issues
                          where i["Foo"] > "foo" && i["Bar"] > new DateTime(2012, 1, 1)
                          select i).ToArray();

            Assert.Equal("(\"Foo\" > \"foo\" and \"Bar\" > \"2012/01/01\")", _translator.Jql);
        }       
    }
}
