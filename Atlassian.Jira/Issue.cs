using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Remote;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Collections.ObjectModel;

namespace Atlassian.Jira
{
    /// <summary>
    /// A JIRA issue
    /// </summary>
    public class Issue: IRemoteIssueFieldProvider
    {
        private readonly RemoteIssue _originalIssue;
        private readonly Jira _jira;
        
        private DateTime? _createDate;
        private DateTime? _updateDate;
        private DateTime? _dueDate;
        private ProjectVersionCollection _affectsVersions = null;
        private ProjectVersionCollection _fixVersions = null;
        private ProjectComponentCollection _components = null;

        public Issue(Jira jira)
            :this(jira, new RemoteIssue())
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

            _affectsVersions = _originalIssue.affectsVersions == null? new ProjectVersionCollection()
                                : new ProjectVersionCollection(_originalIssue.affectsVersions.Select(v => new ProjectVersion(v)).ToList());

            _fixVersions = _originalIssue.fixVersions == null ? new ProjectVersionCollection()
                                : new ProjectVersionCollection(_originalIssue.fixVersions.Select(v => new ProjectVersion(v)).ToList());

            _components = _originalIssue.components == null ? new ProjectComponentCollection()
                                : new ProjectComponentCollection(_originalIssue.components.Select(c => new ProjectComponent(c)).ToList());
        }
       
        /// <summary>
        /// Brief one-line summary of the issue
        /// </summary>
        [JqlContainsEquality]
        public string Summary { get; set; }

        /// <summary>
        /// Detailed description of the issue
        /// </summary>
        [JqlContainsEquality]
        public string Description { get; set; }

        /// <summary>
        /// Hardware or software environment to which the issue relates
        /// </summary>
        [JqlContainsEquality]
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
        /// The components associated with this issue
        /// </summary>
        [JqlFieldName("component")]
        public ProjectComponentCollection Components
        {
            get
            {
                return _components;
            }
        }

        /// <summary>
        /// The versions that are affected by this issue
        /// </summary>
        [JqlFieldName("AffectedVersion")]
        public ProjectVersionCollection AffectsVersions
        {
            get
            {
                return _affectsVersions;
            }
        }

        /// <summary>
        /// The versions in which this issue is fixed
        /// </summary>
        [JqlFieldName("FixVersion")]
        public ProjectVersionCollection FixVersions
        {
            get
            {
                return _fixVersions;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public Issue SaveChanges()
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                return _jira.CreateIssue(this);
            }
            else
            {
                return _jira.UpdateIssue(this);
            }
        }


        /// <summary>
        /// Retrieve attachment metadata from server for this issue
        /// </summary>
        public ReadOnlyCollection<Attachment> GetAttachments()
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to retrieve attachments from server, issue has not been created.");
            }

            return _jira.GetAttachmentsForIssue(_originalIssue.key).ToList().AsReadOnly();
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
        public ReadOnlyCollection<Comment> GetComments()
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to retrieve comments from server, issue has not been created.");
            }
            return _jira.GetCommentsForIssue(_originalIssue.key).ToList().AsReadOnly();
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

        /// <summary>
        /// Add labels to this issue
        /// </summary>
        /// <param name="labels">Label(s) to add</param>
        public void AddLabels(params string[] labels)
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to add label to issue, issue has not been created.");
            }

            var fields = new RemoteFieldValue[] { 
                            new RemoteFieldValue() { 
                                id="labels",
                                values = labels
                            }
                        };

            _jira.UpdateIssue(_originalIssue.key, fields);
        }

        /// <summary>
        /// Gets the RemoteFields representing the fields that were updated
        /// </summary>
        RemoteFieldValue[] IRemoteIssueFieldProvider.GetRemoteFields(string fieldName)
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

                if (typeof(IRemoteIssueFieldProvider).IsAssignableFrom(localProperty.PropertyType))
                {
                    var fieldsProvider = (IRemoteIssueFieldProvider)localProperty.GetValue(this, null);

                    if (fieldsProvider != null)
                    {
                        //https://jira.atlassian.com/browse/JRA-12300
                        fields.AddRange(fieldsProvider.GetRemoteFields(remoteProperty.Name == "affectsVersions" ? "versions" : remoteProperty.Name));
                    }
                }
                else
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

        private static string GetStringValueForProperty(object container, PropertyInfo property)
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
    }
}
