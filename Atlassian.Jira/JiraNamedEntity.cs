using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Linq;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents a named entity within JIRA. Abstracts the IssueType, Priority, Resolution and Status used on issues
    /// </summary>
    public class JiraNamedEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        internal JiraNamedEntity(AbstractRemoteConstant remoteConstant)
        {
            this.Id = remoteConstant.id;
            this.Name = remoteConstant.name;
            this.Description = remoteConstant.description;
        }
    }
}
