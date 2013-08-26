using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Moq;
using System.ServiceModel;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira.Test
{
    public class JiraTests
    {
        public class GetIssuesFromFilter
        {
            [Fact]
            public void IfFilterNotFoundShouldThrowException()
            {
                var jira = TestableJira.Create();

                Assert.Throws(typeof(InvalidOperationException), () => jira.GetIssuesFromFilter("foo"));
            }

            [Fact]
            public void RetrievesFilterIdFromServer()
            {
                var jira = TestableJira.Create();
                jira.SoapService.Setup(s => s.GetFavouriteFilters(It.IsAny<string>()))
                                             .Returns(new RemoteFilter[1] { new RemoteFilter() { name="thefilter", id="123"}});
                jira.GetIssuesFromFilter("thefilter", 100, 200);

                jira.SoapService.Verify(s => s.GetIssuesFromFilterWithLimit(It.IsAny<string>(), "123", 100, 200));
            }

            [Fact]
            public void UsesDefaultsIfNoneProvided()
            {
                var jira = TestableJira.Create();
                jira.SoapService.Setup(s => s.GetFavouriteFilters(It.IsAny<string>()))
                                             .Returns(new RemoteFilter[1] { new RemoteFilter() { name = "thefilter", id = "123" } });
                jira.GetIssuesFromFilter("thefilter");

                jira.SoapService.Verify(s => s.GetIssuesFromFilterWithLimit(It.IsAny<string>(), "123", 0, 20));
            }
        }

        public class WithToken_Anonymous
        {
            [Fact]
            public void DoesNotRetrieveToken()
            {
                var jira = TestableJira.CreateAnonymous();
                jira.SoapService.Setup(s => s.Login(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception("Unexpected call to login"));

                string innerToken = null;
                jira.WithToken(t => innerToken = t);
                Assert.Equal(String.Empty, innerToken);
            }

            [Fact]
            public void DoesNotRetrieveTokenIfMethodThrowsException()
            {
                var jira = TestableJira.CreateAnonymous();
                jira.SoapService.Setup(s => s.Login(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception("Unexpected call to login"));

                string innerToken = null;
                 Assert.Throws(typeof(InvalidOperationException), () =>
                    jira.WithToken(t =>
                    {
                        innerToken = t;
                        throw new InvalidOperationException();
                    }));
                jira.WithToken(t => innerToken = t);
                Assert.Equal(String.Empty, innerToken);
            }
        }

        public class GetAccessToken 
        {
            [Fact]
            public void WillThrowExceptionIfNoCredentialsProviderExists()
            {
                // Arrange
                var soapService = new Mock<IJiraSoapServiceClient>();
                var jira = new Jira(null, soapService.Object, null, null, null); 

                // Act
                Assert.Throws<InvalidOperationException>(() => jira.GetAccessToken());
            }

            [Fact]
            public void WillThrowExceptionIfCredentialsProviderReturnsNull()
            {
                // Arrange
                var soapService = new Mock<IJiraSoapServiceClient>();
                var jira = new Jira(null, soapService.Object, null, null, () => null); 

                // Act
                Assert.Throws<InvalidOperationException>(() => jira.GetAccessToken());
            }
        }

        public class WithToken_UserAndPassword
        {
            [Fact]
            public void RetrievesTokenIfEmpty()
            {
                var jira = TestableJira.Create();
                jira.SoapService.Setup(s => s.Login(It.IsAny<string>(), It.IsAny<string>())).Returns("token");

                string innerToken = "";
                jira.WithToken(t => innerToken = t);
                Assert.Equal("token", innerToken);
            }

            [Fact]
            public void ReusesTokenIfNotEmpty()
            {
                var jira = TestableJira.Create();
                jira.SoapService.Setup(s => s.Login(It.IsAny<string>(), It.IsAny<string>())).ReturnsInOrder("token", new Exception("Unexpected call to login"));

                string innerToken = "";
                jira.WithToken(t => innerToken = t);
                jira.WithToken(t => innerToken = t);
                Assert.Equal("token", innerToken);
            }

            [Fact]
            public void RetrievesNewTokenIfMethodThrowsAuthException()
            {
                var jira = TestableJira.Create(null, new JiraCredentials("user", "pass"));
                jira.SoapService.Setup(s => s.Login(It.IsAny<string>(), It.IsAny<string>())).ReturnsInOrder("token1", "token2");

                string innerToken = "";
                jira.WithToken(t => innerToken = t); // retrieves original token
                jira.WithToken(t =>
                {
                    if (t == "token1")
                        throw new FaultException("com.atlassian.jira.rpc.exception.RemoteAuthenticationException: Invalid username or password.");
                    innerToken = t;
                });
                Assert.Equal("token2", innerToken);
            }

            [Fact]
            public void DoesNotRetrieveNewTokenIfMethodThrowsNonAuthException()
            {
                var jira = TestableJira.Create();
                jira.SoapService.Setup(s => s.Login(It.IsAny<string>(), It.IsAny<string>())).ReturnsInOrder("token1", new Exception("Unexpected call to login"));

                string innerToken = "";
                jira.WithToken(t => innerToken = t);

                Assert.Throws(typeof(InvalidOperationException), () =>
                    jira.WithToken(t =>
                    {
                        throw new InvalidOperationException();
                    }));
            }
        }

        public class DeleteIssue
        {
            [Fact]
            public void WillThrowExceptionIfIssueHasNotBeenInitializedWithAKey()
            {
                // Arrange
                var jira = TestableJira.Create();
                var issue = jira.CreateIssue("TST");

                // Act, Assert
                Assert.Throws<InvalidOperationException>(() => jira.DeleteIssue(issue));
            }

            [Fact]
            public void WillDeleteIssueFromServer()
            {
                // Arrange
                var jira = TestableJira.Create();
                var issue = (new RemoteIssue() { key = "TST-1" }).ToLocal(jira);

                // Act
                jira.DeleteIssue(issue);

                // Assert
                jira.SoapService.Verify(service => service.DeleteIssue(It.IsAny<string>(), "TST-1"));
            }
        }
    }
}
