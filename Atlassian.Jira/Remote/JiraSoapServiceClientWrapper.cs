using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Xml;

namespace Atlassian.Jira.Remote
{
    /// <summary>
    /// Wraps the auto-generated JiraSoapServiceClient proxy
    /// </summary>
    internal class JiraSoapServiceClientWrapper: IJiraSoapServiceClient
    {
        private readonly JiraSoapServiceClient _client;
        private readonly string _url;

        public JiraSoapServiceClientWrapper(string jiraBaseUrl)
        {
            _client = JiraSoapServiceClientFactory.Create(jiraBaseUrl);
            _url = jiraBaseUrl.EndsWith("/") ? jiraBaseUrl : jiraBaseUrl += "/";
        }

        public string Url
        {
            get
            {
                return _url;
            }
        }

        public string Login(string username, string password)
        {
            return _client.login(username, password);
        }

        public RemoteIssue[] GetIssuesFromJqlSearch(string token, string jqlSearch, int maxNumResults)
        {
            return _client.getIssuesFromJqlSearch(token, jqlSearch, maxNumResults);
        }

        public RemoteIssue CreateIssue(string token, RemoteIssue newIssue)
        {
            
            return _client.createIssue(token, newIssue);
        }

        public RemoteIssue UpdateIssue(string token, string key, RemoteFieldValue[] fields)
        {
            return _client.updateIssue(token, key, fields);
        }

        public RemoteAttachment[] GetAttachmentsFromIssue(string token, string key)
        {
            return _client.getAttachmentsFromIssue(token, key);
        }

        public bool AddBase64EncodedAttachmentsToIssue(string token, string key, string[] fileNames, string[] base64EncodedAttachmentData)
        {
            return _client.addBase64EncodedAttachmentsToIssue(token, key, fileNames, base64EncodedAttachmentData);
        }


        public RemoteComment[] GetCommentsFromIssue(string token, string key)
        {
            return _client.getComments(token, key);
        }


        public void AddComment(string token, string key, RemoteComment comment)
        {
            _client.addComment(token, key, comment);
        }


        public RemoteIssueType[] GetIssueTypes(string token, string projectKey)
        {
            if (String.IsNullOrEmpty(null))
            {
                return _client.getIssueTypes(token);
            }
            else
            {
                return _client.getIssueTypesForProject(token, projectKey);
            }
        }

        public RemotePriority[] GetPriorities(string token)
        {
            return _client.getPriorities(token);
        }

        public RemoteResolution[] GetResolutions(string token)
        {
            return _client.getResolutions(token);
        }

        public RemoteStatus[] GetStatuses(string token)
        {
            return _client.getStatuses(token);
        }

        public RemoteVersion[] GetVersions(string token, string projectKey)
        {
            return _client.getVersions(token, projectKey);
        }

        public RemoteComponent[] GetComponents(string token, string projectKey)
        {
            return _client.getComponents(token, projectKey);
        }

        public RemoteField[] GetCustomFields(string token)
        {
            return _client.getCustomFields(token);
        }

        public RemoteField[] GetFieldsForEdit(string token, string key)
        {
            return _client.getFieldsForEdit(token, key);
        }

        public RemoteWorklog AddWorklogAndAutoAdjustRemainingEstimate(string token, string key, RemoteWorklog worklog)
        {
            return _client.addWorklogAndAutoAdjustRemainingEstimate(token, key, worklog);
        }

        public RemoteWorklog AddWorklogAndRetainRemainingEstimate(string token, string key, RemoteWorklog worklog)
        {
            return _client.addWorklogAndRetainRemainingEstimate(token, key, worklog);
        }

        public RemoteWorklog AddWorklogWithNewRemainingEstimate(string token, string key, RemoteWorklog worklog, string newRemainingEstimate)
        {
            return _client.addWorklogWithNewRemainingEstimate(token, key, worklog, newRemainingEstimate);
        }

        public RemoteWorklog[] GetWorkLogs(string token, string key)
        {
            return _client.getWorklogs(token, key);
        }
    }
}