using Atlassian.Jira.Remote;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira.Test
{
    public class TestableJira : Jira
    {
        public Mock<IJiraServiceClient> SoapService;
        public Mock<IFileSystem> FileSystem;

        private TestableJira(Mock<IJiraServiceClient> soapService, Mock<IFileSystem> fileSystem, JiraCredentials credentials, string token)
            : base(null, soapService.Object, fileSystem.Object, credentials, token)
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
            return new TestableJira(new Mock<IJiraServiceClient>(), new Mock<IFileSystem>(), credentials, token);
        }

        public static TestableJira CreateAnonymous()
        {
            return new TestableJira(new Mock<IJiraServiceClient>(), new Mock<IFileSystem>());
        }
    }
}
