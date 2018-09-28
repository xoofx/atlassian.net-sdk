using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// The resolution of the issue as defined in JIRA
    /// </summary>
    [SuppressMessage("N/A", "CS0660", Justification = "Operator overloads are used for LINQ to JQL provider.")]
    [SuppressMessage("N/A", "CS0661", Justification = "Operator overloads are used for LINQ to JQL provider.")]
    public class IssueTransition : JiraNamedEntity
    {
        private IssueStatus _to;
        private bool _hasScreen;
        private bool _isGlobal;
        private bool _isInitial;
        private bool _isConditional;

        /// <summary>
        /// Creates an instance of the IssueResolution based on a remote entity.
        /// </summary>
        public IssueTransition(RemoteTransition remoteEntity)
            : base(remoteEntity)
        {
            _to = remoteEntity.to == null ? null : new IssueStatus(remoteEntity.to);
            _hasScreen = remoteEntity.hasScreen;
            _isGlobal = remoteEntity.isGlobal;
            _isInitial = remoteEntity.isInitial;
            _isConditional = remoteEntity.isConditional;
        }

        /// <summary>
        /// Creates an instance of the IssueResolution with the given id and name.
        /// </summary>
        public IssueTransition(string id, string name = null)
            : base(id, name)
        {
        }

        protected override async Task<IEnumerable<JiraNamedEntity>> GetEntitiesAsync(Jira jira, CancellationToken token)
        {
            var results = await jira.Resolutions.GetResolutionsAsync(token).ConfigureAwait(false);
            return results as IEnumerable<JiraNamedEntity>;
        }

        /// <summary>
        /// Allows assignation by name
        /// </summary>
        public static implicit operator IssueTransition(string name)
        {
            if (name != null)
            {
                int id;
                if (int.TryParse(name, out id))
                {
                    return new IssueTransition(name /*as id*/);
                }
                else
                {
                    return new IssueTransition(null, name);
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Operator overload to simplify LINQ queries
        /// </summary>
        /// <remarks>
        /// Allows calls in the form of issue.Priority == "High"
        /// </remarks>
        public static bool operator ==(IssueTransition entity, string name)
        {
            if ((object)entity == null)
            {
                return name == null;
            }
            else if (name == null)
            {
                return false;
            }
            else
            {
                return entity.Name == name;
            }
        }

        /// <summary>
        /// Operator overload to simplify LINQ queries
        /// </summary>
        /// <remarks>
        /// Allows calls in the form of issue.Priority != "High"
        /// </remarks>
        public static bool operator !=(IssueTransition entity, string name)
        {
            if ((object)entity == null)
            {
                return name != null;
            }
            else if (name == null)
            {
                return true;
            }
            else
            {
                return entity.Name != name;
            }
        }

        public IssueStatus To
        {
            get { return _to; }
        }

        public bool HasScreen
        {
            get { return _hasScreen; }
        }

        public bool IsGlobal
        {
            get { return _isGlobal; }
        }

        public bool IsInitial
        {
            get { return _isInitial; }
        }

        public bool IsConditional
        {
            get { return _isConditional; }
        }
    }
}
