using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Atlassian.Jira.Linq;
using System.IO;

namespace Atlassian.Jira
{
    /// <summary>
    /// Collection of attachments for an issue
    /// </summary>
    public class AttachmentCollection: IEnumerable<Attachment>
    {
        private readonly string _issueKey;
        private readonly Jira _jira;
        private readonly IFileSystem _fileSystem;

        private List<Attachment> _list = null;
        

        public AttachmentCollection(Jira jira, IFileSystem fileSystem, string issueKey)
        {
            _jira = jira;
            _issueKey = issueKey;
            _fileSystem = fileSystem;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="paths"></param>
        public void Upload(params string[] paths)
        {
            if (String.IsNullOrEmpty(_issueKey))
            {
                throw new InvalidOperationException("Unable upload attachment to server, issue has not been created.");
            }

            if (paths.Length > 0)
            {
                List<string> fileNames = new List<string>();
                List<string> fileContents = new List<string>();

                foreach (string path in paths)
                {
                    fileNames.Add(Path.GetFileName(path));
                    fileContents.Add(Convert.ToBase64String(_fileSystem.FileReadAllBytes(path)));
                }
                _jira.AddAttachmentsToIssue(_issueKey, fileNames.ToArray(), fileContents.ToArray());
            }
        }

        public string GetBase64EncodeContent(byte[] content)
        {
            return Convert.ToBase64String(content);
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
                throw new InvalidOperationException("Unable retrieve attachment from server, issue has not been created.");
            }

            _list = new List<Attachment>(_jira.GetAttachmentsForIssue(_issueKey));
        }
    }
}
