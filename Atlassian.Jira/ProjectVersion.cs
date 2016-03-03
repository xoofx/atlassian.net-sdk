using Atlassian.Jira.Remote;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Atlassian.Jira
{
    /// <summary>
    /// A version associated with a project
    /// </summary>
    public class ProjectVersion : JiraNamedEntity
    {
        private RemoteVersion _remoteVersion;

        internal ProjectVersion(Jira jira, RemoteVersion remoteVersion)
            : base(jira, remoteVersion.id)
        {
            if (jira == null)
            {
                throw new ArgumentNullException("jira");
            }

            _name = remoteVersion.name;
            _remoteVersion = remoteVersion;
        }

        internal RemoteVersion RemoteVersion
        {
            get
            {
                return _remoteVersion;
            }
        }

        /// <summary>
        /// Whether this version has been archived
        /// </summary>
        public bool IsArchived
        {
            get
            {
                return _remoteVersion.archived;
            }
            set
            {
                _remoteVersion.archived = value;
            }
        }

        /// <summary>
        /// Whether this version has been released
        /// </summary>
        public bool IsReleased
        {
            get
            {
                return _remoteVersion.released;
            }
            set
            {
                _remoteVersion.released = value;
            }
        }

        /// <summary>
        /// The released date for this version (null if not yet released)
        /// </summary>
        public DateTime? ReleasedDate
        {
            get
            {
                return _remoteVersion.releaseDate;
            }
            set
            {
                _remoteVersion.releaseDate = value;
            }
        }

        /// <summary>
        /// The release description for this version (null if not available)
        /// </summary>
        public string Description
        {
            get
            {
                return _remoteVersion.description;
            }
            set
            {
                _remoteVersion.description = value;
            }
        }

        /// <summary>
        /// Save field changes to the server.
        /// </summary>
        public void SaveChanges()
        {
            try
            {
                SaveChangesAsync().Wait();
            }
            catch (AggregateException ex)
            {
                throw ex.Flatten().InnerException;
            }
        }

        /// <summary>
        /// Save field changes to the server.
        /// </summary>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task SaveChangesAsync(CancellationToken token = default(CancellationToken))
        {
            return Jira.RestClient.UpdateVersionAsync(_remoteVersion, token).ContinueWith(task =>
            {
                _remoteVersion = task.Result;
            });
        }
    }
}