using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents a remote constant within JIRA. Abstracts the IssueType, Priority, Resolution and Status used on issues
    /// </summary>
    /// <remarks>http://docs.atlassian.com/rpc-jira-plugin/latest/com/atlassian/jira/rpc/soap/beans/AbstractRemoteConstant.html</remarks>
    public class JiraConstant: JiraNamedEntity
    {
        public string Description { get; set; }

        internal JiraConstant(AbstractRemoteConstant remoteConstant):
            base(remoteConstant)
        {
            this.Description = remoteConstant.description;
        }
    }
}
