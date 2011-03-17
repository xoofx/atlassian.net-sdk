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

        [Fact]
        public void IntEqualsOperator()
        {
            var field = new ComparableTextField();
            Assert.False(field == 1);

            field.Id = 1;
            Assert.True(field == 1);
        }

        [Fact]
        public void IntNotEqualsOperator()
        {
            var field = new ComparableTextField();
            Assert.True(field != 1);

            field.Id = 1;
            Assert.False(field != 1);
        }

        [Fact]
        public void IntGreaterThanOperator()
        {
            var field = new ComparableTextField();
            Assert.False(field > 1);
        }

        [Fact]
        public void IntLessThanOperator()
        {
            var field = new ComparableTextField();
            Assert.False(field < 1);
        }

        [Fact]
        public void IntLessThanOrEqualsOperator()
        {
            var field = new ComparableTextField();
            Assert.False(field <= 1);
        }

    }
}
