using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Linq;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents a named entity within JIRA. Abstracts the Version and Component used on issues
    /// </summary>
    /// <remarks>http://docs.atlassian.com/rpc-jira-plugin/latest/com/atlassian/jira/rpc/soap/beans/AbstractNamedRemoteEntity.html</remarks>
    public class JiraNamedEntity
    {
        private readonly AbstractNamedRemoteEntity _remoteEntity;
        public string Id { get; set; }
        public string Name { get; set; }

        internal JiraNamedEntity(AbstractNamedRemoteEntity remoteEntity)
        {
            _remoteEntity = remoteEntity;
            this.Id = remoteEntity.id;
            this.Name = remoteEntity.name;
        }
    }
}
