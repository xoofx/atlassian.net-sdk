using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira.Test
{
    public class TestableJira : Jira
    {
        public Mock<IJiraSoapServiceClient> SoapService;
        public Mock<IFileSystem> FileSystem;

        private TestableJira(Mock<IJiraSoapServiceClient> soapService, Mock<IFileSystem> fileSystem, string token, Func<JiraCredentials> credentialsProvider)
            : base(null, soapService.Object, fileSystem.Object, token, credentialsProvider  )
        {
            SoapService = soapService;
            FileSystem = fileSystem;
        }

        private TestableJira(Mock<IJiraSoapServiceClient> soapService, Mock<IFileSystem> fileSystem)
            : base(null, soapService.Object, fileSystem.Object)
        {
            SoapService = soapService;
            FileSystem = fileSystem;
        }

        public static TestableJira Create(string token = "token", JiraCredentials credentials = null)
        {
            return new TestableJira(new Mock<IJiraSoapServiceClient>(), new Mock<IFileSystem>(), token, () => credentials);
        }

        public static TestableJira CreateAnonymous()
        {
            return new TestableJira(new Mock<IJiraSoapServiceClient>(), new Mock<IFileSystem>());
        }
    }
}
