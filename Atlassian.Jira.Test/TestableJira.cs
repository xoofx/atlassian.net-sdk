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
        public Mock<IJiraRemoteService> SoapService;
        public Mock<IFileSystem> FileSystem;

        public const string User = "user";
        public const string Password = "pass";
        public const string Token = "token";

        private TestableJira(Mock<IJiraRemoteService> soapService, Mock<IFileSystem> fileSystem, string user, string pass)
            : base(null, soapService.Object, fileSystem.Object, user, pass)
        {
            SoapService = soapService;
            FileSystem = fileSystem;
            SoapService.Setup(j => j.Login(User, Password)).Returns(Token);
        }

        public static TestableJira Create(string user = User, string pass = Password)
        {
            return new TestableJira(new Mock<IJiraRemoteService>(), new Mock<IFileSystem>(), user, pass);
        }
    }
}
