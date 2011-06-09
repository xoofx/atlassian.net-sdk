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
            return issue.ToRemote();
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
        /// Gets the RemoteFieldValues representing the fields that where updated
        /// </summary>
        public static RemoteFieldValue[] GetUpdatedFields(this Issue issue)
        {
            return issue.GetUpdatedFields();
        }
    }
}
