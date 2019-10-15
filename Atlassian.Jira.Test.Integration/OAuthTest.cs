using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Atlassian.Jira.OAuth;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class OAuthTest : BaseIntegrationTest
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
            Assert.NotNull(oAuthRequestToken);
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
            Assert.Null(accessToken);
        }
    }
}
