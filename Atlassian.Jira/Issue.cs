using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Linq;
using System.Reflection;
using System.IO;
using System.Collections;

namespace Atlassian.Jira
{
    /// <summary>
    /// A JIRA issue
    /// </summary>
    public class Issue
    {
        private readonly RemoteIssue _originalIssue;
        private readonly Jira _jira;
        
        private DateTime? _createDate;
        private DateTime? _updateDate;
        private DateTime? _dueDate;
        private List<Version> _affectsVersions = null;
        private List<Version> _fixVersions = null;

        public Issue(): this(null, new RemoteIssue())
        {
        }

        internal Issue(Jira jira, RemoteIssue remoteIssue)
        {
            _originalIssue = remoteIssue;
            _jira = jira;

            Assignee = remoteIssue.assignee;
            Description = remoteIssue.description;
            Environment = remoteIssue.environment;
            Project = remoteIssue.project;
            Reporter = remoteIssue.reporter;
            Status = remoteIssue.status;
            Summary = remoteIssue.summary;
            Type = remoteIssue.type;
            Votes = remoteIssue.votes;

            _createDate = remoteIssue.created;
            _dueDate = remoteIssue.duedate;
            _updateDate = remoteIssue.updated;

            Key = remoteIssue.key;
            Priority = remoteIssue.priority;
            Resolution = remoteIssue.resolution;
        }

        internal RemoteIssue ToRemote()
        {
            var remote = new RemoteIssue()
            {
                assignee = this.Assignee,
                description = this.Description,
                environment = this.Environment,
                project = this.Project,
                reporter = this.Reporter,
                status = this.Status,
                summary = this.Summary,
                type = this.Type,
                votes = this.Votes
            };

            remote.key = this.Key != null ? this.Key.Value : null;
            remote.priority = this.Priority != null? this.Priority.Value : null;
            remote.resolution = this.Resolution != null ? this.Resolution.Value : null;

            return remote;
        }
       
        /// <summary>
        /// Brief one-line summary of the issue
        /// </summary>
        [ContainsEquality]
        public string Summary { get; set; }

        /// <summary>
        /// Detailed description of the issue
        /// </summary>
        [ContainsEquality]
        public string Description { get; set; }

        /// <summary>
        /// Hardware or software environment to which the issue relates
        /// </summary>
        [ContainsEquality]
        public string Environment { get; set; }

        /// <summary>
        /// Person to whom the issue is currently assigned
        /// </summary>
        public string Assignee { get; set; }

        /// <summary>
        /// Unique identifier for this issue
        /// </summary>
        public ComparableTextField Key { get; set; }

        /// <summary>
        /// Importance of the issue in relation to other issues
        /// </summary>
        public ComparableTextField Priority { get; set; }

        /// <summary>
        /// Parent project to which the issue belongs
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        /// Person who entered the issue into the system
        /// </summary>
        public string Reporter { get; set; }
        
        /// <summary>
        /// Record of the issue's resolution, if the issue has been resolved or closed
        /// </summary>
        public ComparableTextField Resolution { get; set; }

        /// <summary>
        /// The stage the issue is currently at in its lifecycle.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The type of the issue
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// Number of votes the issue has
        /// </summary>
        public long? Votes { get; set; }

        /// <summary>
        /// Time and date on which this issue was entered into JIRA
        /// </summary>
        public DateTime? Created 
        {
            get
            {
                return _createDate;
            }
        }

        /// <summary>
        /// Date by which this issue is scheduled to be completed
        /// </summary>
        public DateTime? DueDate 
        {
            get
            {
                return _dueDate;
            }
            set
            {
                _dueDate = value;
            }
        }

        /// <summary>
        /// Time and date on which this issue was last edited
        /// </summary>
        public DateTime? Updated 
        {
            get
            {
                return _updateDate;
            }
        }

        /// <summary>
        /// The versions that are affected by this issue
        /// </summary>
        public IList<Version> AffectsVersions
        {
            get
            {
                if (_affectsVersions == null)
                {
                    _affectsVersions = new List<Version>();
                    if (_originalIssue.affectsVersions != null)
                    {
                        foreach (var version in _originalIssue.affectsVersions)
                        {
                            _affectsVersions.Add(new Version(version));
                        }
                    }
                }
                return _affectsVersions;
            }
        }

        /// <summary>
        /// The versions in which this issue is fixed
        /// </summary>
        public IList<Version> FixVersions
        {
            get
            {
                if (_fixVersions == null)
                {
                    _fixVersions = new List<Version>();
                    if (_originalIssue.fixVersions != null)
                    {
                        foreach (var version in _originalIssue.fixVersions)
                        {
                            _fixVersions.Add(new Version(version));
                        }
                    }
                }
                return _fixVersions;
            }
        }

        /// <summary>
        /// Retrieve attachment metadata from server for this issue
        /// </summary>
        public IList<Attachment> GetAttachments()
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to retrieve attachments from server, issue has not been created.");
            }

            return new List<Attachment>(_jira.GetAttachmentsForIssue(_originalIssue.key)).AsReadOnly();
        }

        /// <summary>
        /// Add attachment to this issue
        /// </summary>
        /// <param name="filePath">Full path of file to upload</param>
        public void AddAttachment(string filePath)
        {
            this.AddAttachment(Path.GetFileName(filePath), _jira.FileSystem.FileReadAllBytes(filePath));
        }

        /// <summary>
        /// Add attachment to this issue
        /// </summary>
        /// <param name="name">Attachment name with extension</param>
        /// <param name="data">Attachment data</param>
        public void AddAttachment(string name, byte[] data)
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to upload attachments to server, issue has not been created.");
            }

            string content = Convert.ToBase64String(data);

            _jira.AddAttachmentsToIssue(_originalIssue.key, new string[] { name }, new string[]{ content });
        }

        /// <summary>
        /// Retrieve comments from server for this issue
        /// </summary>
        public IList<Comment> GetComments()
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to retrieve comments from server, issue has not been created.");
            }
            return new List<Comment>(_jira.GetCommentsForIssue(_originalIssue.key)).AsReadOnly();
        }

        /// <summary>
        /// Add a comment to this issue
        /// </summary>
        /// <param name="comment">Comment text to add</param>
        public void AddComment(string comment)
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to add comment to issue, issue has not been created.");
            }

            var newComment = new Comment() { Author = _jira.UserName, Body = comment };
            _jira.AddCommentToIssue(_originalIssue.key, newComment);
        }

        private string GetStringValueForProperty(object container, PropertyInfo property)
        {
            var value = property.GetValue(container, null);

            if (property.PropertyType == typeof(DateTime?))
            {
                var dateValue = (DateTime?)value;
                return dateValue.HasValue ? dateValue.Value.ToString("d/MMM/yy") : null;
            }
            else
            {
                return value != null ? value.ToString() : null;
            }
        }

        internal RemoteFieldValue[] GetUpdatedFields()
        {
            var fields = new List<RemoteFieldValue>();

            var remoteFields = typeof(RemoteIssue).GetProperties();
            foreach (var localProperty in typeof(Issue).GetProperties())
            {
                var remoteProperty = remoteFields.FirstOrDefault(i => i.Name.Equals(localProperty.Name, StringComparison.OrdinalIgnoreCase));
                if (remoteProperty == null)
                {
                    continue;
                }

                if (!typeof(IList<Version>).IsAssignableFrom(localProperty.PropertyType))
                {
                    var localStringValue = GetStringValueForProperty(this, localProperty);
                    var remoteStringValue = GetStringValueForProperty(_originalIssue, remoteProperty);

                    if (remoteStringValue != localStringValue)
                    {
                        fields.Add(new RemoteFieldValue()
                        {
                            id = remoteProperty.Name,
                            values = new string[1] { localStringValue }
                        });
                    }
                }
            }

            return fields.ToArray();
        }
    }
}
