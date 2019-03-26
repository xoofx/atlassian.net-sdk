using System;
using Atlassian.Jira.Remote;
using System.Collections.Generic;
using System.Linq;

namespace Atlassian.Jira
{
    /// <summary>
    /// A comment associated with an issue
    /// </summary>
    public class Comment
    {
        private readonly IEnumerable<RemoteCommentProperty> _properties;
        private Dictionary<string, object> _propertiesMap;

        /// <summary>
        /// Create a new Comment.
        /// </summary>
        public Comment() :
            this(new RemoteComment())
        {
        }

        /// <summary>
        /// Create a new Comment from a remote instance object.
        /// </summary>
        /// <param name="remoteComment">The remote comment.</param>
        public Comment(RemoteComment remoteComment)
        {
            Id = remoteComment.id;
            Author = remoteComment.author;
            Body = remoteComment.body;
            CreatedDate = remoteComment.created;
            GroupLevel = remoteComment.groupLevel;
            RoleLevel = remoteComment.roleLevel;
            UpdateAuthor = remoteComment.updateAuthor;
            UpdatedDate = remoteComment.updated;
            Visibility = remoteComment.visibility;
            _properties = remoteComment.properties;
        }

        public string Id { get; private set; }

        public string Author { get; set; }

        public string Body { get; set; }

        public string GroupLevel { get; set; }

        public string RoleLevel { get; set; }

        public DateTime? CreatedDate { get; private set; }

        public string UpdateAuthor { get; private set; }

        public DateTime? UpdatedDate { get; private set; }

        public CommentVisibility Visibility { get; set; }

        public IReadOnlyDictionary<string, object> Properties
        {
            get
            {
                if (_propertiesMap == null)
                {
                    if (_properties == null)
                    {
                        _propertiesMap = new Dictionary<string, object>();
                    }
                    else
                    {
                        _propertiesMap = _properties.ToDictionary(prop => prop.key, prop => prop.value);
                    }
                }

                return _propertiesMap;
            }

        }

        internal RemoteComment ToRemote()
        {
            return new RemoteComment
            {
                author = this.Author,
                body = this.Body,
                groupLevel = this.GroupLevel,
                roleLevel = this.RoleLevel,
                visibility = this.Visibility
            };
        }
    }
}
