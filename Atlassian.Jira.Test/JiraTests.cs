using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Moq;

namespace Atlassian.Jira.Test
{
    public class JiraTests
    {
        [Fact]
        public void WithToken_RetrievesTokenIfEmpty()
        {
            var jira = TestableJira.Create();
            jira.SoapService.Setup(s => s.Login(It.IsAny<string>(), It.IsAny<string>())).Returns("token");

            string innerToken = "";
            jira.WithToken(t => innerToken = t);
            Assert.Equal("token", innerToken);
        }

        [Fact]
        public void WithToken_ReusesTokenIfNotEmpty()
        {
            var jira = TestableJira.Create();
            jira.SoapService.Setup(s => s.Login(It.IsAny<string>(), It.IsAny<string>())).ReturnsInOrder("token", new Exception("Unexpected call to login"));

            string innerToken = "";
            jira.WithToken(t => innerToken = t);
            jira.WithToken(t => innerToken = t);
            Assert.Equal("token", innerToken);
        }

        [Fact]
        public void WithToken_RetrievesNewTokenIfMethodThrowsException()
        {
            var jira = TestableJira.Create();
            jira.SoapService.Setup(s => s.Login(It.IsAny<string>(), It.IsAny<string>())).ReturnsInOrder("token1", "token2");
            
            string innerToken = "";
            jira.WithToken(t => innerToken = t);
            jira.WithToken(t => {
                if(t == "token1")
                    throw new Exception();
                innerToken = t;
            });
            Assert.Equal("token2", innerToken);
        }
    }
}
