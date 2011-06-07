using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Atlassian.Jira.Linq;

namespace Atlassian.Jira
{
    /// <summary>
    /// Collection of attachments for an issue
    /// </summary>
    public class AttachmentCollection: IEnumerable<Attachment>
    {
        private readonly string _issueKey;
        private readonly Jira _jira;
        private List<Attachment> _list = null;

        public AttachmentCollection(Jira jira, string issueKey)
        {
            _jira = jira;
            _issueKey = issueKey;
        }

        public IEnumerator<Attachment> GetEnumerator()
        {
            if (_list == null)
            {
                if(String.IsNullOrEmpty(_issueKey))
                {
                    _list = new List<Attachment>();
                }
                else
                {
                    Refresh();
                }
            }
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Refreshes the attachment list for this issue from the server
        /// </summary>
        public void Refresh()
        {
            if (String.IsNullOrEmpty(_issueKey))
            {
                throw new InvalidOperationException("Unable retrieve attachment from server, issue key has not been specified");
            }

            _list = new List<Attachment>(_jira.GetAttachmentsForIssue(_issueKey));
        }
    }
}
