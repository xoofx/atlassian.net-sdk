using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// A comment associated with an issue
    /// </summary>
    public class Comment
    {
        private readonly RemoteComment _remoteComment;

        public Comment():
            this(new RemoteComment())
        {
        }

        internal Comment(RemoteComment remoteComment)
        {
            _remoteComment = remoteComment;
        }

        public string Author
        {
            get
            {
                return _remoteComment.author;
            }
            set
            {
                _remoteComment.author = value;
            }
        }

        public string Body
        {
            get
            {
                return _remoteComment.body;
            }
            set
            {
                _remoteComment.body = value;
            }
        }

        public DateTime? CreatedDate
        {
            get
            {
                return _remoteComment.created;
            }
        }

        public string GroupLevel
        {
            get
            {
                return _remoteComment.groupLevel;
            }
        }

        public string Id
        {
            get
            {
                return _remoteComment.id;
            }
        }

        public string RoleLevel
        {
            get
            {
                return _remoteComment.roleLevel;
            }
        }

        public string UpdateAuthor
        {
            get
            {
                return _remoteComment.updateAuthor;
            }
        }

        public DateTime? UpdatedDate
        {
            get
            {
                return _remoteComment.updated;
            }
        }

        internal RemoteComment toRemote()
        {
            return _remoteComment;
        }
    }
}
