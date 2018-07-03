using System;
using System.Threading.Tasks;
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
        private readonly string _jiraUrl;
        private readonly IWebClient _webClient;

        /// <summary>
        /// Creates a new instance of an Attachment from a remote entity.
        /// </summary>
        /// <param name="jira">Object used to interact with JIRA.</param>
        /// <param name="webClient">WebClient to use to download attachment.</param>
        /// <param name="remoteAttachment">Remote attachment entity.</param>
        public Attachment(Jira jira, IWebClient webClient, RemoteAttachment remoteAttachment) :
            this(jira.Url, webClient, remoteAttachment)
        {
        }

        /// <summary>
        /// Creates a new instance of an Attachment from a remote entity.
        /// </summary>
        /// <param name="jiraUrl">Address to the JIRA server.</param>
        /// <param name="webClient">WebClient to use to download attachment.</param>
        /// <param name="remoteAttachment">Remote attachment entity.</param>
        public Attachment(string jiraUrl, IWebClient webClient, RemoteAttachment remoteAttachment)
        {
            _jiraUrl = jiraUrl;
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
        /// Downloads attachment to specified file.
        /// </summary>
        /// <param name="fullFileName">Full file name where attachment will be downloaded.</param>
        public Task DownloadAsync(string fullFileName)
        {
            var url = GetRequestUrl();

            return _webClient.DownloadWithAuthenticationAsync(url, fullFileName);
        }

        /// <summary>
        /// Downloads attachment as a byte array.
        /// </summary>
        public Task<byte[]> DownloadDataAsync()
        {
            var url = GetRequestUrl();

            return _webClient.DownloadDataWithAuthenticationAsync(url);
        }

        /// <summary>
        /// Downloads attachment to specified file
        /// </summary>
        /// <param name="fullFileName">Full file name where attachment will be downloaded</param>
        public void Download(string fullFileName)
        {
            this.DownloadAsync(fullFileName).Wait();
        }

        private string GetRequestUrl()
        {
            if (String.IsNullOrEmpty(_jiraUrl))
            {
                throw new InvalidOperationException("Unable to download attachment, JIRA url has not been set.");
            }

            return String.Format("{0}secure/attachment/{1}/{2}",
                _jiraUrl.EndsWith("/") ? _jiraUrl : _jiraUrl + "/",
                this.Id,
                this.FileName);
        }
    }
}
