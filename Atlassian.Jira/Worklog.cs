using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents the worklog of an issue
    /// </summary>
    public class Worklog
    {
        private readonly string _author;
        private readonly string _comment;
        private readonly DateTime? _created;
        private readonly string _id;
        private readonly DateTime? _startDate;
        private readonly string _timeSpent;
        private readonly long _timeSpentInSeconds;
        private readonly DateTime? _updated;

        public string Author 
        {
            get { return _author; }
        }

        public string Comment
        {
            get { return _comment; }
        }

        public DateTime? CreateDate
        {
            get { return _created; }
        }

        public string Id
        {
            get { return _id; }
        }

        public DateTime? StartDate
        {
            get { return _startDate; }
        }

        public string TimeSpent
        {
            get { return _timeSpent; }
        }

        public long TimeSpentInSeconds
        {
            get { return _timeSpentInSeconds; }
        }

        public DateTime? UpdateDate
        {
            get { return _updated; }
        }

        public Worklog(RemoteWorklog remoteWorklog)
        {
            _author = remoteWorklog.author;
            _comment = remoteWorklog.comment;
            _created = remoteWorklog.created;
            _id = remoteWorklog.id;
            _startDate = remoteWorklog.startDate;
            _timeSpent = remoteWorklog.timeSpent;
            _timeSpentInSeconds = remoteWorklog.timeSpentInSeconds;
            _updated = remoteWorklog.updated;
        }
    }
}
