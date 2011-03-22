using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira.Linq
{
    /// <summary>
    /// Abstracts calling the remote JIRA server
    /// </summary>
    public interface IJiraRemoteService
    {
        /// <summary>
        /// Invokes the getIssuesFromJql RPC call
        /// </summary>
        /// <param name="jql">JQL statement to execute</param>
        /// <returns>List of Issues</returns>
        IList<Issue> GetIssuesFromJql(string jql);
    }
}
