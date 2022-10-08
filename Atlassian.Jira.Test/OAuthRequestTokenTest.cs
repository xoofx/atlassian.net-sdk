using Atlassian.Jira.OAuth;
using Newtonsoft.Json;
using Xunit;

namespace Atlassian.Jira.Test
{
    public class OAuthRequestTokenTest
    {
        [Fact]
        public void OAuthRequestToken_CanDeserialize()
        {
            // Arrange
            var requestToken = new OAuthRequestToken(
                "authorize_uri",
                "oauth_token",
                "oauth_token_secret",
                "oauth_callback_confirmation");
            var json = JsonConvert.SerializeObject(requestToken);

            // Act
            var deserializedRequestToken = JsonConvert.DeserializeObject<OAuthRequestToken>(json);

            // Assert
            Assert.Equal(requestToken.AuthorizeUri, deserializedRequestToken.AuthorizeUri);
            Assert.Equal(requestToken.OAuthToken, deserializedRequestToken.OAuthToken);
            Assert.Equal(requestToken.OAuthTokenSecret, deserializedRequestToken.OAuthTokenSecret);
            Assert.Equal(requestToken.OAuthCallbackConfirmation, deserializedRequestToken.OAuthCallbackConfirmation);
        }
    }
}
