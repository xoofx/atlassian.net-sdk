using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlassian.Jira
{
    [Flags]
    public enum JiraUserStatus
    {
        Active = 0,
        Inactive = 1
    }
}
