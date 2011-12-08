using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Remote;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Collections.ObjectModel;
using System.Dynamic;

namespace Atlassian.Jira
{
    /// <summary>
    /// A JIRA issue
    /// </summary>
    public class Issue : IRemoteIssueFieldProvider
    {
        private readonly Jira _jira;


        private ComparableString _key;
        private string _project;
        private RemoteIssue _originalIssue;
        private DateTime? _createDate;
        private DateTime? _updateDate;
        private DateTime? _dueDate;
        private ProjectVersionCollection _affectsVersions = null;
        private ProjectVersionCollection _fixVersions = null;
        private ProjectComponentCollection _components = null;
        private CustomFieldCollection _customFields = null;


        public Issue(Jira jira, string projectKey)
            : this(jira, new RemoteIssue() { project = projectKey })
        {
        }

        internal Issue(Jira jira, RemoteIssue remoteIssue)
        {
            _jira = jira;
           
            Initialize(remoteIssue);
        }

        private void Initialize(RemoteIssue remoteIssue)
        {
            _originalIssue = remoteIssue;

            _project = remoteIssue.project;
            _key = remoteIssue.key;
            _createDate = remoteIssue.created;
            _dueDate = remoteIssue.duedate;
            _updateDate = remoteIssue.updated;

            Assignee = remoteIssue.assignee;
            Description = remoteIssue.description;
            Environment = remoteIssue.environment;
            Reporter = remoteIssue.reporter;
            Summary = remoteIssue.summary;
            Votes = remoteIssue.votes;

            // named entities
            Status = String.IsNullOrEmpty(remoteIssue.status) ? null : new IssueStatus(_jira, remoteIssue.status);
            Priority = String.IsNullOrEmpty(remoteIssue.priority) ? null : new IssuePriority(_jira, remoteIssue.priority);
            Resolution = String.IsNullOrEmpty(remoteIssue.resolution) ? null : new IssueResolution(_jira, remoteIssue.resolution);
            Type = String.IsNullOrEmpty(remoteIssue.type)? null: new IssueType(_jira, remoteIssue.type);

            // collections
            _affectsVersions = _originalIssue.affectsVersions == null ? new ProjectVersionCollection("versions", _jira, Project)
                : new ProjectVersionCollection("versions", _jira, Project, _originalIssue.affectsVersions.Select(v => new ProjectVersion(v)).ToList());

            _fixVersions = _originalIssue.fixVersions == null ? new ProjectVersionCollection("fixVersions", _jira, Project)
                : new ProjectVersionCollection("fixVersions", _jira, Project, _originalIssue.fixVersions.Select(v => new ProjectVersion(v)).ToList());

            _components = _originalIssue.components == null ? new ProjectComponentCollection("components", _jira, Project)
                : new ProjectComponentCollection("components", _jira, Project, _originalIssue.components.Select(c => new ProjectComponent(c)).ToList());

            _customFields = _originalIssue.customFieldValues == null ? new CustomFieldCollection(_jira, Project)
                : new CustomFieldCollection(_jira, Project, _originalIssue.customFieldValues.Select(f => new CustomField(f.customfieldId, _jira) { Values = f.values }).ToList());
  
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
        public ComparableString Key 
        {
            get
            {
                return _key;
            }
        }

        /// <summary>
        /// Importance of the issue in relation to other issues
        /// </summary>
        public IssuePriority Priority { get; set; }

        /// <summary>
        /// Parent project to which the issue belongs
        /// </summary>
        public string Project 
        {
            get
            {
                return _project;
            }
        }

        /// <summary>
        /// Person who entered the issue into the system
        /// </summary>
        public string Reporter { get; set; }
        
        /// <summary>
        /// Record of the issue's resolution, if the issue has been resolved or closed
        /// </summary>
        public IssueResolution Resolution { get; set; }

        /// <summary>
        /// The stage the issue is currently at in its lifecycle.
        /// </summary>
        public IssueStatus Status { get; set; }

        /// <summary>
        /// The type of the issue
        /// </summary>
        public IssueType Type { get; set; }
        
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
        /// The custom fields associated with this issue
        /// </summary>
        public CustomFieldCollection CustomFields
        {
            get
            {
                return _customFields;
            }
        }

        /// <summary>
        /// Gets or sets the value of a custom field
        /// </summary>
        /// <param name="customFieldName">Custom field name</param>
        /// <returns>Value of the custom field</returns>
        public string this[string customFieldName]
        {
            get
            {
                var customField = _customFields[customFieldName];

                if(customField != null && customField.Values != null && customField.Values.Count() > 0)
                {
                    return customField.Values[0];
                }
                return null;
            }
            set
            {
                var customField = _customFields[customFieldName];

                if (customField != null)
                {
                    customField.Values = new string[] { value };
                }
                else
                {
                    _customFields.Add(customFieldName, new string[] { value });
                }
            }
        }

        /// <summary>
        /// Saves field changes to server
        /// </summary>
        public void SaveChanges()
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                var token = _jira.GetAuthenticationToken();
                var remoteIssue = this.ToRemote();

                remoteIssue = _jira.RemoteSoapService.CreateIssue(token, remoteIssue);
                
                Initialize(remoteIssue);
            }
            else
            {
                SaveRemoteFields(((IRemoteIssueFieldProvider)this).GetRemoteFields());
            }
        }

        private void SaveRemoteFields(RemoteFieldValue[] remoteFields)
        {
            var token = _jira.GetAuthenticationToken();
            var remoteIssue = _jira.RemoteSoapService.UpdateIssue(token, this.Key.Value, remoteFields);
            Initialize(remoteIssue);
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

            SaveRemoteFields(fields);
        }

        public void AddWorklog(string timespent)
        {
            if(String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to add worklog to issue, issue has not been saved to server.");
            }

            var worklog = new RemoteWorklog()
            {
                startDate = DateTime.Now,
                timeSpent = timespent
            };

            var token = _jira.GetAuthenticationToken();
            _jira.RemoteSoapService.AddWorklogAndAutoAdjustRemainingEstimate(token, _originalIssue.key, worklog);
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
                summary = this.Summary,
                votes = this.Votes,
                duedate = this.DueDate
            };

            remote.key = this.Key != null ? this.Key.Value : null;

            if (Status != null)
            {
                remote.status = Status.Id ?? Status.Load(_jira, Project);
            }

            if (Resolution != null)
            {
                remote.resolution = Resolution.Id ?? Resolution.Load(_jira, Project);
            }

            if (Priority != null)
            {
                remote.priority = Priority.Id ?? Priority.Load(_jira, Project);
            }

            if (Type != null)
            {
                remote.type = Type.Id ?? Type.Load(_jira, Project);
            }

            if (this.AffectsVersions.Count > 0)
            {
                remote.affectsVersions = this.AffectsVersions.Select(v => v.RemoteVersion).ToArray();
            }

            if(this.FixVersions.Count > 0)
            {
                remote.fixVersions = this.FixVersions.Select(v => v.RemoteVersion).ToArray();
            }

            if (this.Components.Count > 0)
            {
                remote.components = this.Components.Select(c => c.RemoteComponent).ToArray();
            }

            if (this.CustomFields.Count > 0)
            {
                remote.customFieldValues = this.CustomFields.Select(f => new RemoteCustomFieldValue()
                {
                    customfieldId = f.Id,
                    values = f.Values
                }).ToArray();
            }

            return remote;
        }

        /// <summary>
        /// Gets the RemoteFields representing the fields that were updated
        /// </summary>
        RemoteFieldValue[] IRemoteIssueFieldProvider.GetRemoteFields()
        {
            var fields = new List<RemoteFieldValue>();

            var remoteFields = typeof(RemoteIssue).GetProperties();
            foreach (var localProperty in typeof(Issue).GetProperties())
            {
                if (typeof(IRemoteIssueFieldProvider).IsAssignableFrom(localProperty.PropertyType))
                {
                    var fieldsProvider = localProperty.GetValue(this, null) as IRemoteIssueFieldProvider;

                    if (fieldsProvider != null)
                    {
                        fields.AddRange(fieldsProvider.GetRemoteFields());
                    }
                }
                else
                {
                    var remoteProperty = remoteFields.FirstOrDefault(i => i.Name.Equals(localProperty.Name, StringComparison.OrdinalIgnoreCase));
                    if (remoteProperty == null)
                    {
                        continue;
                    }

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
