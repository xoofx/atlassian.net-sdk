using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Atlassian.Jira.Test
{
    public class ComparableStringTest
    {
        [Fact]
        public void RefereceIsNull_EqualsOperators()
        {
            ComparableString field = null;
            Assert.True(field == null);
            Assert.False(field != null);
        }

        [Fact]
        public void StringEqualsOperator()
        {
            Assert.False(new ComparableString("bar") == "foo");
            Assert.True(new ComparableString("foo") == "foo");
        }

        [Fact]
        public void StringNotEqualsOperator()
        {
            Assert.True(new ComparableString("bar") != "foo");
            Assert.False(new ComparableString("foo") != "foo");
        }

        [Fact]
        public void StringGreaterThanOperator()
        {
            Assert.True(new ComparableString("TST-23") > "TST-1");
        }

        [Fact]
        public void StringLessThanOperator()
        {
            Assert.True(new ComparableString("TST-1") < "TST-2");
        }

        [Fact]
        public void StringLessThanOrEqualsOperator()
        {
            Assert.True(new ComparableString("TST-1") <= "TST-2");
        }
    }
}
