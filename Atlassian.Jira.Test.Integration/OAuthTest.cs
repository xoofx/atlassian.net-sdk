using System;
using System.Threading;
using System.Threading.Tasks;
using Atlassian.Jira.OAuth;
using LTAF;
using Xunit;
using UniTestAssert = Xunit.Assert;

namespace Atlassian.Jira.Test.Integration
{
    public class OAuthTest
    {
        [Fact]
        public async Task CanGenerateRequestToken()
        {
            // Arrange
            var oAuthTokenSettings = new OAuthRequestTokenSettings(
                JiraProvider.HOST,
                JiraProvider.OAUTHCONSUMERKEY,
                JiraProvider.OAUTHCONSUMERSECRET);

            // Act
            var oAuthRequestToken = await OAuthTokenHelper.GenerateRequestTokenAsync(oAuthTokenSettings);

            // Assert
            UniTestAssert.NotNull(oAuthRequestToken);
        }

        [Fact]
        public async Task AccessTokenIsNullIfRequestTokenNotAuthorized()
        {
            // Arrange
            var oAuthTokenSettings = new OAuthRequestTokenSettings(
                JiraProvider.HOST,
                JiraProvider.OAUTHCONSUMERKEY,
                JiraProvider.OAUTHCONSUMERSECRET);
            var oAuthRequestToken = await OAuthTokenHelper.GenerateRequestTokenAsync(oAuthTokenSettings);
            var oAuthAccessTokenSettings = new OAuthAccessTokenSettings(oAuthTokenSettings, oAuthRequestToken);

            // Act
            var accessToken = await OAuthTokenHelper.ObtainAccessTokenAsync(oAuthAccessTokenSettings, CancellationToken.None);

            // Assert
            UniTestAssert.Null(accessToken);
        }

        [Fact]
        public async Task CanGenerateAccesToken()
        {
            // Arrange
            var oAuthTokenSettings = new OAuthRequestTokenSettings(
                JiraProvider.HOST,
                JiraProvider.OAUTHCONSUMERKEY,
                JiraProvider.OAUTHCONSUMERSECRET);
            var oAuthRequestToken = await OAuthTokenHelper.GenerateRequestTokenAsync(oAuthTokenSettings);

            // Login to Jira
            var page = new HtmlPage(new Uri(JiraProvider.HOST));
            page.Navigate("/login.jsp");
            var elements = page.RootElement.ChildElements;
            elements.Find("username").SetText(JiraProvider.USERNAME);
            elements.Find("password").SetText(JiraProvider.PASSWORD);
            elements.Find("submit").Click();

            // Authorize token
            page.Navigate(oAuthRequestToken.AuthorizeUri);
            page.RootElement.ChildElements.Find("approve").Click();

            // Act
            var oAuthAccessTokenSettings = new OAuthAccessTokenSettings(oAuthTokenSettings, oAuthRequestToken);
            var accessToken = await OAuthTokenHelper.ObtainAccessTokenAsync(oAuthAccessTokenSettings, CancellationToken.None);

            // Assert
            UniTestAssert.NotNull(accessToken);
        }
    }
}
