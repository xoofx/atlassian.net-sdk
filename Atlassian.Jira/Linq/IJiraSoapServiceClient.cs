using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira.Linq
{
    /// <summary>
    /// Abstract the proxy SOAP client implementation
    /// </summary>
    public interface IJiraSoapServiceClient
    {
        string Login(string username, string password);
        RemoteIssue[] GetIssuesFromJqlSearch(string token, string jqlSearch, int maxNumResults);
        RemoteIssue CreateIssue(string token, RemoteIssue newIssue);
        RemoteIssue UpdateIssue(string token, string key, RemoteFieldValue[] fields);
    }
}
