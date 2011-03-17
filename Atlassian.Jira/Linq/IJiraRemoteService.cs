using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira.Linq
{
    public interface IJiraRemoteService
    {
        IList<Issue> GetIssuesFromJql(string p);
    }
}
