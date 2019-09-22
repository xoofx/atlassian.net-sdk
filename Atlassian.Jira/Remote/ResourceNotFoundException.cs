using System;
using System.Runtime.Serialization;

namespace Atlassian.Jira.Remote
{
    /// <summary>
    /// Exception thrown when the server responds with HTTP code 404.
    /// </summary>
    public class ResourceNotFoundException : InvalidOperationException
    {
        public ResourceNotFoundException()
        {
        }

        public ResourceNotFoundException(string message) : base(message)
        {
        }

        public ResourceNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ResourceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
