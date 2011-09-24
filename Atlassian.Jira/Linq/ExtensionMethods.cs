using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Atlassian.Jira.Linq
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Create a new RemoteIssue based on the information in a given issue
        /// </summary>
        public static RemoteIssue ToRemote(this Issue issue)
        {
            var remote = new RemoteIssue()
            {
                assignee = issue.Assignee,
                description = issue.Description,
                environment = issue.Environment,
                project = issue.Project,
                reporter = issue.Reporter,
                status = issue.Status,
                summary = issue.Summary,
                type = issue.Type,
                votes = issue.Votes
            };

            remote.key = issue.Key != null ? issue.Key.Value : null;
            remote.priority = issue.Priority != null ? issue.Priority.Value : null;
            remote.resolution = issue.Resolution != null ? issue.Resolution.Value : null;

            if (issue.AffectsVersions.Count > 0)
            {
                remote.affectsVersions = issue.AffectsVersions.Select(v => v.RemoteVersion).ToArray();
            }

            if(issue.FixVersions.Count > 0)
            {
                remote.fixVersions = issue.FixVersions.Select(v => v.RemoteVersion).ToArray();

            }

            return remote;
        }

        /// <summary>
        /// Create a new Issue from a RemoteIssue
        /// </summary>
        public static Issue ToLocal(this RemoteIssue remoteIssue, Jira jira = null)
        {
            return new Issue(jira, remoteIssue);
        }

        /// <summary>
        /// Create a new Attachment from a RemoteAttachment
        /// </summary>
        public static Attachment ToLocal(this RemoteAttachment remoteAttachment, Jira jira, IWebClient webClient)
        {
            return new Attachment(jira, webClient, remoteAttachment);
        }

        /// <summary>
        /// Creates a new Version from RemoteVersion
        /// </summary>
        public static Version ToLocal(this RemoteVersion remoteVersion)
        {
            return new Version(remoteVersion);
        }

        /// <summary>
        /// Creates a new Component from RemoteComponent
        /// </summary>
        public static Component ToLocal(this RemoteComponent remoteComponent)
        {
            return new Component(remoteComponent);
        }
    }
}
