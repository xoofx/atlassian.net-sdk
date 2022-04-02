using System;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents the worklog of an issue
    /// </summary>
    public class Worklog
    {
        public string Author { get; set; }
        public JiraUser AuthorUser { get; private set; }
        public string Comment { get; set; }
        public DateTime? StartDate { get; set; }
        public string TimeSpent { get; set; }

        public string Id { get; private set; }

        public long TimeSpentInSeconds { get; private set; }

        public DateTime? CreateDate { get; private set; }

        public DateTime? UpdateDate { get; private set; }

        /// <summary>
        /// Creates a new worklog instance
        /// </summary>
        /// <param name="timeSpent">Specifies a time duration in JIRA duration format, representing the time spent working</param>
        /// <param name="startDate">When the work was started</param>
        /// <param name="comment">An optional comment to describe the work</param>
        public Worklog(string timeSpent, DateTime startDate, string comment = null)
        {
            this.TimeSpent = timeSpent;
            this.StartDate = startDate;
            this.Comment = comment;
        }

        internal Worklog(RemoteWorklog remoteWorklog)
        {
            if (remoteWorklog != null)
            {
                this.Author = remoteWorklog.authorUser?.InternalIdentifier;
                this.AuthorUser = remoteWorklog.authorUser;
                this.Comment = remoteWorklog.comment;
                this.StartDate = remoteWorklog.startDate;
                this.TimeSpent = remoteWorklog.timeSpent;
                Id = remoteWorklog.id;
                CreateDate = remoteWorklog.created;
                TimeSpentInSeconds = remoteWorklog.timeSpentInSeconds;
                UpdateDate = remoteWorklog.updated;
            }
        }

        internal RemoteWorklog ToRemote()
        {
            return new RemoteWorklog()
            {
                authorUser = this.Author == null ? null : new JiraUser() { InternalIdentifier = this.Author },
                comment = this.Comment,
                startDate = this.StartDate,
                timeSpent = this.TimeSpent
            };
        }
    }
}
