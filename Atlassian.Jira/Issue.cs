using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Collections.ObjectModel;
using System.Dynamic;
using Atlassian.Jira.Remote;
using Atlassian.Jira.Linq;

namespace Atlassian.Jira
{
    /// <summary>
    /// A JIRA issue
    /// </summary>
    public class Issue : IRemoteIssueFieldProvider
    {
        private readonly Jira _jira;
        private readonly string _parentIssueKey;

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

        public Issue(Jira jira, string projectKey, string parentIssueKey = null)
            : this(jira, new RemoteIssue() { project = projectKey }, parentIssueKey)
        {
        }

        internal Issue(Jira jira, RemoteIssue remoteIssue, string parentIssueKey = null)
        {
            _jira = jira;
            _parentIssueKey = parentIssueKey;
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
                : new CustomFieldCollection(_jira, Project, _originalIssue.customFieldValues.Select(f => new CustomField(f.customfieldId, Project, _jira) { Values = f.values }).ToList());
  
        }

        /// <summary>
        /// The JIRA server that created this issue
        /// </summary>
        public Jira Jira
        {
            get
            {
                return _jira;
            }
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
        [RemoteFieldName("issuetype")]
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
                var remoteIssue = this.ToRemote();

                _jira.WithToken(token =>
                {
                    if (String.IsNullOrEmpty(_parentIssueKey))
                    {
                        remoteIssue = _jira.RemoteSoapService.CreateIssue(token, remoteIssue);
                    }
                    else
                    {
                        remoteIssue = _jira.RemoteSoapService.CreateIssueWithParent(token, remoteIssue, _parentIssueKey);
                    }
                });

                Initialize(remoteIssue);
            }
            else
            {
                UpdateRemoteFields(((IRemoteIssueFieldProvider)this).GetRemoteFields());
            }
        }

        /// <summary>
        /// Transition an issue through a workflow
        /// </summary>
        /// <param name="actionName">The workflow action to transition to</param>
        public void WorkflowTransition(string actionName)
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to execute workflow transition, issue has not been created.");
            }

            var action = this.GetAvailableActions().FirstOrDefault(a => a.Name.Equals(actionName, StringComparison.OrdinalIgnoreCase));
            if (action == null)
            {
                throw new InvalidOperationException(String.Format("Worflow action with name '{0}' not found.", actionName));
            }
            
            _jira.WithToken(token =>
            {
                var remoteIssue = _jira.RemoteSoapService.ProgressWorkflowAction(
                                                                token,
                                                                _originalIssue.key,
                                                                action.Id,
                                                                ((IRemoteIssueFieldProvider)this).GetRemoteFields());
                Initialize(remoteIssue);
            });
        }

        private void UpdateRemoteFields(RemoteFieldValue[] remoteFields)
        {
            var remoteIssue = _jira.WithToken(token =>
            {
                return _jira.RemoteSoapService.UpdateIssue(token, this.Key.Value, remoteFields);
            });
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

            return _jira.WithToken(token =>
            {
                return _jira.RemoteSoapService.GetAttachmentsFromIssue(token, _originalIssue.key)
                    .Select(a => new Attachment(_jira, new WebClientWrapper(), a)).ToList().AsReadOnly();
            });
        }

        /// <summary>
        /// Add one or more attachments to this issue
        /// </summary>
        /// <param name="filePaths">Full paths of files to upload</param>
        public void AddAttachment(params string[] filePaths)
        {
            var attachments = filePaths.Select(f => new UploadAttachmentInfo(Path.GetFileName(f), _jira.FileSystem.FileReadAllBytes(f))).ToArray();

            AddAttachment(attachments);
        }

        /// <summary>
        /// Add an attachment to this issue
        /// </summary>
        /// <param name="name">Attachment name with extension</param>
        /// <param name="data">Attachment data</param>
        public void AddAttachment(string name, byte[] data)
        {
            AddAttachment(new UploadAttachmentInfo(name, data));
        }

        /// <summary>
        /// Add one or more attachments to this issue
        /// </summary>
        public void AddAttachment(params UploadAttachmentInfo[] attachments)
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to upload attachments to server, issue has not been created.");
            }

            var content = new List<string>();
            var names = new List<string>();
            
            foreach (var a in attachments)
            {
                names.Add(a.Name);
                content.Add(Convert.ToBase64String(a.Data));
            }

            _jira.WithToken(token =>
            {
                _jira.RemoteSoapService.AddBase64EncodedAttachmentsToIssue(
                    token, 
                    _originalIssue.key, 
                    names.ToArray(), 
                    content.ToArray());
            });
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

            return _jira.WithToken(token =>
            {
                return _jira.RemoteSoapService.GetCommentsFromIssue(token, _originalIssue.key).Select(c => new Comment(c)).ToList().AsReadOnly();   
            });
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

            _jira.WithToken(token =>
            {
                _jira.RemoteSoapService.AddComment(token, _originalIssue.key, newComment.toRemote());
            });
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

            UpdateRemoteFields(fields);
        }
       
        /// <summary>
        ///  Adds a worklog to this issue.
        /// </summary>
        /// <param name="timespent">Specifies a time duration in JIRA duration format, representing the time spent working on the worklog</param>
        /// <param name="worklogStrategy">How to handle the remaining estimate, defaults to AutoAdjustRemainingEstimate</param>
        /// <param name="newEstimate">New estimate (only used if worklogStrategy set to NewRemainingEstimate)</param>
        /// <returns>Worklog as constructed by server</returns>
        public Worklog AddWorklog(string timespent, 
                                  WorklogStrategy worklogStrategy = WorklogStrategy.AutoAdjustRemainingEstimate,
                                  string newEstimate = null)
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

            RemoteWorklog remoteWorklog = null;

            _jira.WithToken(token =>
            {
                switch (worklogStrategy)
                {
                    case WorklogStrategy.RetainRemainingEstimate:
                        remoteWorklog = _jira.RemoteSoapService.AddWorklogAndRetainRemainingEstimate(token, _originalIssue.key, worklog);
                        break;
                    case WorklogStrategy.NewRemainingEstimate:
                        remoteWorklog = _jira.RemoteSoapService.AddWorklogWithNewRemainingEstimate(token, _originalIssue.key, worklog, newEstimate);
                        break;                    
                    default:
                        remoteWorklog = _jira.RemoteSoapService.AddWorklogAndAutoAdjustRemainingEstimate(token, _originalIssue.key, worklog);
                        break;
                }
            });

            return new Worklog(remoteWorklog);
        }

        /// <summary>
        /// Retrieve worklogs for current issue
        /// </summary>
        public ReadOnlyCollection<Worklog> GetWorklogs()
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to retrieve worklog, issue has not been saved to server.");
            }

            return _jira.WithToken(token =>
            {
                return _jira.RemoteSoapService.GetWorkLogs(token, _originalIssue.key).Select(w => new Worklog(w)).ToList().AsReadOnly();
            });
        }

        /// <summary>
        /// Updates all fields from server
        /// </summary>
        public void Refresh()
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to refresh, issue has not been saved to server.");
            }

            var remoteIssue = _jira.WithToken(token =>
            {
                return _jira.RemoteSoapService.GetIssuesFromJqlSearch(token, "key = " + _originalIssue.key, 1).First();
            });
            Initialize(remoteIssue);
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
                remote.status = Status.Id ?? Status.Load(_jira, Project).Id;
            }

            if (Resolution != null)
            {
                remote.resolution = Resolution.Id ?? Resolution.Load(_jira, Project).Id;
            }

            if (Priority != null)
            {
                remote.priority = Priority.Id ?? Priority.Load(_jira, Project).Id;
            }

            if (Type != null)
            {
                remote.type = Type.Id ?? Type.Load(_jira, Project).Id;
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
        /// Gets the workflow actions that the issue can be transitioned to
        /// </summary>
        /// <returns></returns>
        public IEnumerable<JiraNamedEntity> GetAvailableActions()
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to retrieve actions, issue has not been saved to server.");
            }

            return _jira.WithToken(token =>
            {
                return _jira.RemoteSoapService.GetAvailableActions(token, _originalIssue.key).Select(a => new JiraNamedEntity(a));
            });
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
                        var remoteFieldName = remoteProperty.Name;

                        var remoteFieldNameAttr = localProperty.GetCustomAttributes(typeof(RemoteFieldNameAttribute), true).OfType<RemoteFieldNameAttribute>().FirstOrDefault();
                        if (remoteFieldNameAttr != null)
                        {
                            remoteFieldName = remoteFieldNameAttr.Name;
                        }

                        fields.Add(new RemoteFieldValue()
                        {
                            id = remoteFieldName,
                            values = new string[1] { localStringValue }
                        });
                    }
                }
            }

            return fields.ToArray();
        }

        private string GetStringValueForProperty(object container, PropertyInfo property)
        {
            var value = property.GetValue(container, null);

            if (property.PropertyType == typeof(DateTime?))
            {
                var dateValue = (DateTime?)value;
                return dateValue.HasValue ? dateValue.Value.ToString("d/MMM/yy") : null;
            }
            else if (typeof(JiraNamedEntity).IsAssignableFrom(property.PropertyType))
            {
                var jiraNamedEntity = property.GetValue(container, null) as JiraNamedEntity;
                if (jiraNamedEntity != null)
                {
                    return jiraNamedEntity.Id ?? jiraNamedEntity.Load(_jira, this.Project).Id;
                }
                return null;
            }
            else
            {
                return value != null ? value.ToString() : null;
            }
        }

        
    }
}
