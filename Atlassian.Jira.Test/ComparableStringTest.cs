using System;
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

        public class WithDate
        {
            [Fact]
            public void StringEqualsOperator()
            {
                Assert.False(new ComparableString("2012/05/01") == new DateTime(2012, 4, 1));
                Assert.True(new ComparableString("2012/04/01") == new DateTime(2012, 4, 1));
            }

            [Fact]
            public void StringNotEqualsOperator()
            {
                Assert.True(new ComparableString("2012/05/01") != new DateTime(2012, 4, 1));
                Assert.False(new ComparableString("2012/04/01") != new DateTime(2012, 4, 1));
            }

            [Fact]
            public void StringGreaterThanOperator()
            {
                Assert.True(new ComparableString("2012/01/10") > new DateTime(2012, 1, 1));
            }

            [Fact]
            public void StringLessThanOperator()
            {
                Assert.True(new ComparableString("2012/01/10") < new DateTime(2012, 1, 11));
            }

            [Fact]
            public void StringLessThanOrEqualsOperator()
            {
                Assert.True(new ComparableString("2012/01/10") <= new DateTime(2012, 1, 10));
            }
        }

        public class WithString
        {
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
}
