using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                key = issue.Key.Value,
                status = issue.Status,
                summary = issue.Summary,
                type = issue.Type,
                votes = issue.Votes
            };

            //remote.key = issue.Key != null ?  issue.Key.Value: null;

            return remote;
        }

        /// <summary>
        /// Create a new Issue from a RemoteIssue
        /// </summary>
        public static Issue ToLocal(this RemoteIssue remoteIssue)
        {
            return new Issue()
            {
                Assignee = remoteIssue.assignee,
                Description = remoteIssue.description,
                Environment = remoteIssue.environment,
                Key = new ComparableTextField(remoteIssue.key),
                Priority = new ComparableTextField(remoteIssue.priority),
                Project = remoteIssue.project,
                Reporter = remoteIssue.reporter,
                Resolution = new ComparableTextField(remoteIssue.resolution),
                Status = remoteIssue.status,
                Summary = remoteIssue.summary,
                Type = remoteIssue.type,
                Votes = remoteIssue.votes.Value
            };
        }
    }
}
