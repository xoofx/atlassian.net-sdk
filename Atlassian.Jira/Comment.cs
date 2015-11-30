using Atlassian.Jira.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    /// <summary>
    /// A comment associated with an issue
    /// </summary>
    public class Comment
    {
        private readonly RemoteComment _remoteComment;

        /// <summary>
        /// Create a new Comment.
        /// </summary>
        public Comment() :
            this(new RemoteComment())
        {
        }

        /// <summary>
        /// Create a new Comment from a remote intance object.
        /// </summary>
        /// <param name="remoteComment">The remote comment.</param>
        public Comment(RemoteComment remoteComment)
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

        public string Id
        {
            get
            {
                return _remoteComment.id;
            }
        }

        public string GroupLevel
        {
            get
            {
                return _remoteComment.groupLevel;
            }
            set
            {
                _remoteComment.groupLevel = value;
            }
        }

        public string RoleLevel
        {
            get
            {
                return _remoteComment.roleLevel;
            }
            set
            {
                _remoteComment.roleLevel = value;
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
