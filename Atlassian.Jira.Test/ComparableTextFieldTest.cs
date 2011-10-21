using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Atlassian.Jira.Test
{
    public class ComparableTextFieldTest
    {
        [Fact]
        public void RefereceIsNull_EqualsOperators()
        {
            ComparableTextField field = null;
            Assert.True(field == null);
            Assert.False(field != null);
        }

        [Fact]
        public void StringEqualsOperator()
        {
            var field = new ComparableTextField();
            Assert.False(field == "foo");

            field.Value = "foo";
            Assert.True(field == "foo");
        }

        [Fact]
        public void StringNotEqualsOperator()
        {
            var field = new ComparableTextField();
            Assert.True(field != "foo");

            field.Value = "foo";
            Assert.False(field != "foo");
        }

        [Fact]
        public void StringGreaterThanOperator()
        {
            var field = new ComparableTextField();
            Assert.False(field > "foo");
        }

        [Fact]
        public void StringLessThanOperator()
        {
            var field = new ComparableTextField();
            Assert.False(field < "foo");
        }

        [Fact]
        public void StringLessThanOrEqualsOperator()
        {
            var field = new ComparableTextField();
            Assert.False(field <= "foo");
        }
    }
}
