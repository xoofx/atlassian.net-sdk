using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Linq;
using Xunit;

namespace Atlassian.Linq.Test
{
    
    public class SomeClassTest
    {
        [Fact]
        public void MyTest()
        {
            var c = new SomeClass();

            Assert.Equal("foo2", c.HelloWorld("foo"));
        }
    }
}
