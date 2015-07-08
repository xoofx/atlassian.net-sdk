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
        public Mock<IJiraServiceClient> SoapService;
        public Mock<IFileSystem> FileSystem;

        private TestableJira(Mock<IJiraServiceClient> soapService, Mock<IFileSystem> fileSystem, string token, Func<JiraCredentials> credentialsProvider)
            : base(null, soapService.Object, fileSystem.Object, token, credentialsProvider  )
        {
            SoapService = soapService;
            FileSystem = fileSystem;
        }

        private TestableJira(Mock<IJiraServiceClient> soapService, Mock<IFileSystem> fileSystem)
            : base(null, soapService.Object, fileSystem.Object)
        {
            SoapService = soapService;
            FileSystem = fileSystem;
        }

        public static TestableJira Create(string token = "token", JiraCredentials credentials = null)
        {
            return new TestableJira(new Mock<IJiraServiceClient>(), new Mock<IFileSystem>(), token, () => credentials);
        }

        public static TestableJira CreateAnonymous()
        {
            return new TestableJira(new Mock<IJiraServiceClient>(), new Mock<IFileSystem>());
        }
    }
}
