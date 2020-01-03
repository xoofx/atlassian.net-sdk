using System.Collections.Generic;
using System.Linq;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// An issue transition as defined in JIRA.
    /// </summary>
    public class IssueTransition : JiraNamedEntity
    {
        /// <summary>
        /// Creates an instance of the IssueTransition based on a remote entity.
        /// </summary>
        public IssueTransition(RemoteTransition remoteEntity)
            : base(remoteEntity)
        {
            To = remoteEntity.to == null ? null : new IssueStatus(remoteEntity.to);
            HasScreen = remoteEntity.hasScreen;
            IsGlobal = remoteEntity.isGlobal;
            IsInitial = remoteEntity.isInitial;
            IsConditional = remoteEntity.isConditional;
            Fields = remoteEntity.fields?.ToDictionary(x => x.Key, x => new IssueFieldEditMetadata(x.Value));
        }

        /// <summary>
        /// Creates an instance of the IssueTransition with the given id and name.
        /// </summary>
        public IssueTransition(string id, string name = null)
            : base(id, name)
        {
        }

        public IssueStatus To { get; private set; }

        public bool HasScreen { get; private set; }

        public bool IsGlobal { get; private set; }

        public bool IsInitial { get; private set; }

        public bool IsConditional { get; private set; }

        public Dictionary<string, IssueFieldEditMetadata> Fields { get; private set; }
    }
}
