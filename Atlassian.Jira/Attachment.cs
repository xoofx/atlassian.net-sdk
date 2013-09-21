﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// An attachment associated with an issue
    /// </summary>
    public class Attachment
    {
        private readonly string _author;
        private readonly DateTime? _created;
        private readonly string _fileName;
        private readonly string _mimeType;
        private readonly long? _fileSize;
        private readonly string _id;
        private readonly Jira _jira;
        private readonly IWebClient _webClient;

        internal Attachment(Jira jira, IWebClient webClient, RemoteAttachment remoteAttachment)
        {
            _jira = jira;
            _author = remoteAttachment.author;
            _created = remoteAttachment.created;
            _fileName = remoteAttachment.filename;
            _mimeType = remoteAttachment.mimetype;
            _fileSize = remoteAttachment.filesize;
            _id = remoteAttachment.id;
            _webClient = webClient;
        }

        /// <summary>
        /// Id of attachment
        /// </summary>
        public string Id
        {
            get { return _id; }
        } 

        /// <summary>
        /// Author of attachment (user that uploaded the file)
        /// </summary>
        public string Author
        {
            get { return _author; }
        }

        /// <summary>
        /// Date of creation
        /// </summary>
        public DateTime? CreatedDate
        {
            get { return _created; }
        }

        /// <summary>
        /// File name of the attachment
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
        }

        /// <summary>
        /// Mime type
        /// </summary>
        public string MimeType
        {
            get { return _mimeType; }
        }

        /// <summary>
        /// File size
        /// </summary>
        public long? FileSize
        {
            get { return _fileSize; }
        }

        /// <summary>
        /// Downloads attachment to specified file
        /// </summary>
        /// <param name="fullFileName">Full file name where attachment will be downloaded</param>
        public void Download(string fullFileName)
        {
            var credentials = _jira.GetCredentials();

            if (String.IsNullOrEmpty(credentials.UserName) || String.IsNullOrEmpty(credentials.Password))
            {
                throw new InvalidOperationException("Unable to download attachment, user and/or password are missing. You can specify a provider for credentials when constructing the Jira instance.");
            }

            _webClient.AddQueryString("os_username", Uri.EscapeDataString(credentials.UserName));
            _webClient.AddQueryString("os_password", Uri.EscapeDataString(credentials.Password));

            var url = String.Format("{0}secure/attachment/{1}/{2}",
                _jira.Url.EndsWith("/") ? _jira.Url : _jira.Url + "/",
                this.Id,
                this.FileName);

            _webClient.Download(url, fullFileName);
        }
    }
}
