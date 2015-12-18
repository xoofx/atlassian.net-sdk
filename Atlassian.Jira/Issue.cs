using Atlassian.Jira.Linq;
using Atlassian.Jira.Remote;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        private DateTime? _resolutionDate;
        private ProjectVersionCollection _affectsVersions = null;
        private ProjectVersionCollection _fixVersions = null;
        private ProjectComponentCollection _components = null;
        private CustomFieldValueCollection _customFields = null;
        private IssueStatus _status;
        private string _parentIssueKey;

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
            _resolutionDate = remoteIssue.resolutionDateReadOnly;

            Assignee = remoteIssue.assignee;
            Description = remoteIssue.description;
            Environment = remoteIssue.environment;
            Reporter = remoteIssue.reporter;
            Summary = remoteIssue.summary;
            Votes = remoteIssue.votes;

            if (!String.IsNullOrEmpty(remoteIssue.parentKey))
            {
                _parentIssueKey = remoteIssue.parentKey;
            }

            // named entities
            _status = String.IsNullOrEmpty(remoteIssue.status) ? null : new IssueStatus(_jira, remoteIssue.status);
            Priority = String.IsNullOrEmpty(remoteIssue.priority) ? null : new IssuePriority(_jira, remoteIssue.priority);
            Resolution = String.IsNullOrEmpty(remoteIssue.resolution) ? null : new IssueResolution(_jira, remoteIssue.resolution);
            Type = String.IsNullOrEmpty(remoteIssue.type) ? null : new IssueType(_jira, remoteIssue.type);

            // collections
            _affectsVersions = _originalIssue.affectsVersions == null ? new ProjectVersionCollection("versions", _jira, Project)
                : new ProjectVersionCollection("versions", _jira, Project, _originalIssue.affectsVersions.Select(v => new ProjectVersion(v)).ToList());

            _fixVersions = _originalIssue.fixVersions == null ? new ProjectVersionCollection("fixVersions", _jira, Project)
                : new ProjectVersionCollection("fixVersions", _jira, Project, _originalIssue.fixVersions.Select(v => new ProjectVersion(v)).ToList());

            _components = _originalIssue.components == null ? new ProjectComponentCollection("components", _jira, Project)
                : new ProjectComponentCollection("components", _jira, Project, _originalIssue.components.Select(c => new ProjectComponent(c)).ToList());

            _customFields = _originalIssue.customFieldValues == null ? new CustomFieldValueCollection(this)
                : new CustomFieldValueCollection(this, _originalIssue.customFieldValues.Select(f => new CustomFieldValue(f.customfieldId, this) { Values = f.values }).ToList());
        }

        internal RemoteIssue OriginalRemoteIssue
        {
            get
            {
                return this._originalIssue;
            }
        }

        /// <summary>
        /// The parent key if this issue is a subtask.
        /// </summary>
        /// <remarks>
        /// Only available if issue was retrieved using REST API.
        /// </remarks>
        public string ParentIssueKey
        {
            get { return _parentIssueKey; }
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
        /// Gets the internal identifier assigned by JIRA.
        /// </summary>
        public string JiraIdentifier
        {
            get
            {
                if (String.IsNullOrEmpty(this._originalIssue.key))
                {
                    throw new InvalidOperationException("Unable to retrieve JIRA id, issue has not been created.");
                }

                return this._originalIssue.id;
            }
        }

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
        public IssueStatus Status
        {
            get
            {
                return _status;
            }
        }

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
        /// Time and date on which this issue was resolved.
        /// </summary>
        /// <remarks>
        /// Only available if issue was retrieved using REST API, use GetResolutionDate
        /// method for SOAP clients.
        /// </remarks>
        public DateTime? ResolutionDate
        {
            get
            {
                return _resolutionDate;
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
        public CustomFieldValueCollection CustomFields
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
        public ComparableString this[string customFieldName]
        {
            get
            {
                var customField = _customFields[customFieldName];

                if (customField != null && customField.Values != null && customField.Values.Count() > 0)
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
                    customField.Values = new string[] { value.Value };
                }
                else
                {
                    _customFields.Add(customFieldName, new string[] { value.Value });
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
                        remoteIssue = _jira.RemoteService.CreateIssue(token, remoteIssue);
                    }
                    else
                    {
                        remoteIssue = _jira.RemoteService.CreateIssueWithParent(token, remoteIssue, _parentIssueKey);
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
        /// Transition an issue through a workflow action.
        /// </summary>
        /// <param name="actionName">The workflow action to transition to.</param>
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
                var remoteIssue = _jira.RemoteService.ProgressWorkflowAction(
                                                                token,
                                                                this.ToRemote(),
                                                                action.Id,
                                                                ((IRemoteIssueFieldProvider)this).GetRemoteFields());
                Initialize(remoteIssue);
            });
        }

        /// <summary>
        /// Creates a link between this issue and the issue specified.
        /// </summary>
        /// <param name="inwardIssueKey">Key of the issue to link.</param>
        /// <param name="linkName">Name of the issue link type.</param>
        /// <param name="comment">Comment to add to this issue.</param>
        public void LinkToIssue(string inwardIssueKey, string linkName, string comment = null)
        {
            try
            {
                this.LinkToIssueAsync(inwardIssueKey, linkName, comment, CancellationToken.None);
            }
            catch (AggregateException ex)
            {
                throw ex.Flatten().InnerException;
            }
        }

        /// <summary>
        /// Creates a link between this issue and the issue specified.
        /// </summary>
        /// <param name="inwardIssueKey">Key of the issue to link.</param>
        /// <param name="linkName">Name of the issue link type.</param>
        /// <param name="comment">Comment to add to this issue.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task LinkToIssueAsync(string inwardIssueKey, string linkName, string comment, CancellationToken token)
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to link issue, issue has not been created.");
            }

            return this.Jira.RestClient.LinkIssuesAsync(this.Key.Value, inwardIssueKey, linkName, comment, token);
        }

        /// <summary>
        /// Gets the issue links associated with this issue.
        /// </summary>
        public IEnumerable<IssueLink> GetIssueLinks()
        {
            try
            {
                return this.GetIssueLinksAsync(CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.Flatten().InnerException;
            }
        }

        /// <summary>
        /// Gets the issue links associated with this issue.
        /// </summary>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task<IEnumerable<IssueLink>> GetIssueLinksAsync(CancellationToken token)
        {
            return this.Jira.RestClient.GetIssueLinksAsync(this, token);
        }

        /// <summary>
        /// Transition an issue through a workflow action.
        /// </summary>
        /// <param name="actionName">The workflow action to transition to.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task WorkflowTransitionAsync(string actionName, CancellationToken token)
        {
            return this.WorkflowTransitionAsync(actionName, null, token);
        }

        /// <summary>
        /// Transition an issue through a workflow action.
        /// </summary>
        /// <param name="actionName">The workflow action to transition to.</param>
        /// <param name="additionalUpdates">Additional updates to perform when transitioning the issue.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task WorkflowTransitionAsync(string actionName, WorkflowTransitionUpdates additionalUpdates, CancellationToken token)
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to execute workflow transition, issue has not been created.");
            }

            return this.GetAvailableActionsAsync(token).ContinueWith(actionsTask =>
            {
                var action = actionsTask.Result.FirstOrDefault(a => a.Name.Equals(actionName, StringComparison.OrdinalIgnoreCase));

                if (action == null)
                {
                    throw new InvalidOperationException(String.Format("Worflow action with name '{0}' not found.", actionName));
                }

                return this._jira.RestClient.ExecuteIssueWorkflowActionAsync(this, action.Id, additionalUpdates, token).ContinueWith(issueTask =>
                {
                    Initialize(issueTask.Result.OriginalRemoteIssue);
                });
            }).Unwrap();
        }

        private void UpdateRemoteFields(RemoteFieldValue[] remoteFields)
        {
            var remoteIssue = _jira.WithToken(token =>
            {
                return _jira.RemoteService.UpdateIssue(token, this.ToRemote(), remoteFields);
            });
            Initialize(remoteIssue);
        }

        /// <summary>
        /// Returns the issues that are marked as sub tasks of this issue.
        /// </summary>
        /// <param name="maxIssues">Maximum number of issues to retrieve.</param>
        /// <param name="startAt">Index of the first issue to return (0-based).</param>
        public IPagedQueryResult<Issue> GetSubTaks(int? maxIssues = null, int startAt = 0)
        {
            try
            {
                return this.GetSubTasksAsync(maxIssues, startAt).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.Flatten().InnerException;
            }
        }

        /// <summary>
        /// Returns the issues that are marked as sub tasks of this issue.
        /// </summary>
        /// <param name="maxIssues">Maximum number of issues to retrieve.</param>
        /// <param name="startAt">Index of the first issue to return (0-based).</param>
        public Task<IPagedQueryResult<Issue>> GetSubTasksAsync(int? maxIssues = null, int startAt = 0)
        {
            return this.GetSubTasksAsync(maxIssues ?? this.Jira.MaxIssuesPerRequest, startAt, CancellationToken.None);
        }

        /// <summary>
        /// Returns the issues that are marked as sub tasks of this issue.
        /// </summary>
        /// <param name="maxIssues">Maximum number of issues to retrieve.</param>
        /// <param name="startAt">Index of the first issue to return (0-based).</param>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task<IPagedQueryResult<Issue>> GetSubTasksAsync(int maxIssues, int startAt, CancellationToken token)
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to retrieve subtasks from server, issue has not been created.");
            }

            var jql = String.Format("parent = {0}", this.Key.Value);
            return this.Jira.RestClient.GetIssuesFromJqlAsync(jql, maxIssues, startAt, token).ContinueWith(task =>
            {
                return task.Result as IPagedQueryResult<Issue>;
            });
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
                return _jira.RemoteService.GetAttachmentsFromIssue(token, _originalIssue.key)
                    .Select(a => new Attachment(_jira, new WebClientWrapper(), a)).ToList().AsReadOnly();
            });
        }

        /// <summary>
        /// Retrieve attachment metadata from server for this issue
        /// </summary>
        public Task<IEnumerable<Attachment>> GetAttachmentsAsync(CancellationToken token)
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to retrieve attachments from server, issue has not been created.");
            }

            return this.Jira.RestClient.GetAttachmentsFromIssueAsync(this.Key.Value, token);
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
                _jira.RemoteService.AddBase64EncodedAttachmentsToIssue(
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
                return _jira.RemoteService.GetCommentsFromIssue(token, _originalIssue.key).Select(c => new Comment(c)).ToList().AsReadOnly();
            });
        }

        /// <summary>
        /// Get the comments for this issue.
        /// </summary>
        /// <param name="maxComments">Maximum number of comments to retrieve.</param>
        /// <param name="startAt">Index of the first comment to return (0-based).</param>
        public Task<IPagedQueryResult<Comment>> GetCommentsAsync(int? maxComments = null, int startAt = 0)
        {
            return this.GetCommentsAsync(maxComments ?? this.Jira.MaxIssuesPerRequest, startAt, CancellationToken.None);
        }

        /// <summary>
        /// Get the comments for this issue.
        /// </summary>
        /// <param name="maxComments">Maximum number of comments to retrieve.</param>
        /// <param name="startAt">Index of the first comment to return (0-based).</param>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task<IPagedQueryResult<Comment>> GetCommentsAsync(int maxComments, int startAt, CancellationToken token)
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to retrieve comments from server, issue has not been created.");
            }

            return this.Jira.RestClient.GetCommentsFromIssueAsync(this.Key.Value, maxComments, startAt, token);
        }

        /// <summary>
        /// Add a comment to this issue.
        /// </summary>
        /// <param name="comment">Comment text to add.</param>
        public void AddComment(string comment)
        {
            var credentials = _jira.GetCredentials();
            var newComment = new Comment() { Author = credentials.UserName, Body = comment };

            this.AddComment(newComment);
        }

        /// <summary>
        /// Add a comment to this issue.
        /// </summary>
        /// <param name="comment">Comment object to add.</param>
        public void AddComment(Comment comment)
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to add comment to issue, issue has not been created.");
            }

            if (String.IsNullOrEmpty(comment.Author))
            {
                throw new InvalidOperationException("Unable to add comment due to missing author field. You can specify a provider for credentials when constructing the Jira instance.");
            }

            _jira.WithToken(token =>
            {
                _jira.RemoteService.AddComment(token, _originalIssue.key, comment.toRemote());
            });
        }

        /// <summary>
        /// Add a comment to this issue.
        /// </summary>
        /// <param name="comment">Comment text to add.</param>
        public Task AddCommentAsync(string comment)
        {
            var credentials = _jira.GetCredentials();
            return this.AddCommentAsync(new Comment() { Author = credentials.UserName, Body = comment }, CancellationToken.None);
        }

        /// <summary>
        /// Add a comment to this issue.
        /// </summary>
        /// <param name="comment">Comment object to add.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task AddCommentAsync(Comment comment, CancellationToken token)
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to add comment to issue, issue has not been created.");
            }

            if (String.IsNullOrEmpty(comment.Author))
            {
                throw new InvalidOperationException("Unable to add comment due to missing author field.");
            }

            return this.Jira.RestClient.AddCommentToIssueAsync(this.Key.Value, comment, token);
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

            _jira.WithToken(token =>
            {
                this._jira.RemoteService.AddLabels(token, _originalIssue, labels);
            });
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
            return AddWorklog(new Worklog(timespent, DateTime.Now), worklogStrategy, newEstimate);
        }

        /// <summary>
        ///  Adds a worklog to this issue.
        /// </summary>
        /// <param name="worklog">The worklog instance to add</param>
        /// <param name="worklogStrategy">How to handle the remaining estimate, defaults to AutoAdjustRemainingEstimate</param>
        /// <param name="newEstimate">New estimate (only used if worklogStrategy set to NewRemainingEstimate)</param>
        /// <returns>Worklog as constructed by server</returns>
        public Worklog AddWorklog(Worklog worklog,
                                  WorklogStrategy worklogStrategy = WorklogStrategy.AutoAdjustRemainingEstimate,
                                  string newEstimate = null)
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to add worklog to issue, issue has not been saved to server.");
            }

            RemoteWorklog remoteWorklog = worklog.ToRemote();
            _jira.WithToken(token =>
            {
                switch (worklogStrategy)
                {
                    case WorklogStrategy.RetainRemainingEstimate:
                        remoteWorklog = _jira.RemoteService.AddWorklogAndRetainRemainingEstimate(token, _originalIssue.key, remoteWorklog);
                        break;
                    case WorklogStrategy.NewRemainingEstimate:
                        remoteWorklog = _jira.RemoteService.AddWorklogWithNewRemainingEstimate(token, _originalIssue.key, remoteWorklog, newEstimate);
                        break;
                    default:
                        remoteWorklog = _jira.RemoteService.AddWorklogAndAutoAdjustRemainingEstimate(token, _originalIssue.key, remoteWorklog);
                        break;
                }
            });

            return new Worklog(remoteWorklog);
        }

        /// <summary>
        /// Deletes the worklog with the given id and updates the remaining estimate field on the isssue
        /// </summary>
        public void DeleteWorklog(Worklog worklog, WorklogStrategy worklogStrategy = WorklogStrategy.AutoAdjustRemainingEstimate, string newEstimate = null)
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to delete worklog from issue, issue has not been saved to server.");
            }

            Jira.WithToken((token, client) =>
            {
                switch (worklogStrategy)
                {
                    case WorklogStrategy.AutoAdjustRemainingEstimate:
                        client.DeleteWorklogAndAutoAdjustRemainingEstimate(token, this._originalIssue.key, worklog.Id);
                        break;
                    case WorklogStrategy.RetainRemainingEstimate:
                        client.DeleteWorklogAndRetainRemainingEstimate(token, this._originalIssue.key, worklog.Id);
                        break;
                    case WorklogStrategy.NewRemainingEstimate:
                        client.DeleteWorklogWithNewRemainingEstimate(token, this._originalIssue.key, worklog.Id, newEstimate);
                        break;
                }
            });
        }

        /// <summary>
        /// Retrieve the resolution date for this issue.
        /// </summary>
        /// <returns>Resultion date for this issue, null if it hasn't been resolved.</returns>
        public DateTime? GetResolutionDate()
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                return null;
            }

            return _jira.WithToken(token =>
            {
                var date = _jira.RemoteService.GetResolutionDateByKey(token, _originalIssue.key);
                this._resolutionDate = date.Ticks > 0 ? date : (DateTime?)null;
                return this._resolutionDate;
            });
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
                return _jira.RemoteService.GetWorkLogs(token, _originalIssue.key).Select(w => new Worklog(w)).ToList().AsReadOnly();
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
                return _jira.RemoteService.GetIssuesFromJqlSearch(token, "key = " + _originalIssue.key, 1).First();
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
                remote.status = Status.Id ?? Status.LoadByName(_jira, Project).Id;
            }

            if (Resolution != null)
            {
                remote.resolution = Resolution.Id ?? Resolution.LoadByName(_jira, Project).Id;
            }

            if (Priority != null)
            {
                remote.priority = Priority.Id ?? Priority.LoadByName(_jira, Project).Id;
            }

            if (Type != null)
            {
                remote.type = Type.Id ?? Type.LoadByName(_jira, Project).Id;
            }

            if (this.AffectsVersions.Count > 0)
            {
                remote.affectsVersions = this.AffectsVersions.Select(v => v.RemoteVersion).ToArray();
            }

            if (this.FixVersions.Count > 0)
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
        /// Gets the workflow actions that the issue can be transitioned to.
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
                return _jira.RemoteService.GetAvailableActions(token, _originalIssue.key).Select(a => new JiraNamedEntity(a));
            });
        }

        /// <summary>
        /// Gets the workflow actions that the issue can be transitioned to.
        /// </summary>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task<IEnumerable<JiraNamedEntity>> GetAvailableActionsAsync(CancellationToken token)
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to retrieve actions, issue has not been saved to server.");
            }

            return this._jira.RestClient.GetActionsForIssueAsync(_originalIssue.key, token);
        }

        /// <summary>
        /// Gets time tracking information for this issue.
        /// </summary>
        public IssueTimeTrackingData GetTimeTrackingData()
        {
            if (String.IsNullOrEmpty(_originalIssue.key))
            {
                throw new InvalidOperationException("Unable to retrieve time tracking data, issue has not been saved to server.");
            }

            return _jira.RestClient.GetTimeTrackingData(_originalIssue.key);
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
                    return jiraNamedEntity.Id ?? jiraNamedEntity.LoadByName(_jira, this.Project).Id;
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
