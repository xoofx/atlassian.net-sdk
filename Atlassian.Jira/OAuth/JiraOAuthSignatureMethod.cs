namespace Atlassian.Jira.OAuth
{
    /// <summary>
    /// Possible values for OAuth signature method.
    /// </summary>
    public enum JiraOAuthSignatureMethod
    {
        HmacSha1,
        HmacSha256,
        PlainText,
        RsaSha1
    }
}
