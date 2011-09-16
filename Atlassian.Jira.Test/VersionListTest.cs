using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Moq;
using Atlassian.Jira.Linq;

namespace Atlassian.Jira.Test
{
    public class VersionListTest
    {
        [Fact]
        public void Construct_IfNulls_ShouldThrow()
        {
            //arrange
            var mockSoapClient = new Mock<IJiraSoapServiceClient>();
            var jira = new Jira(null, mockSoapClient.Object, null, "user", "pass");

            //act
            Assert.Throws(typeof(ArgumentNullException), () => new VersionList(jira, null, new List<Version>()));
            Assert.Throws(typeof(ArgumentNullException), () => new VersionList(null, "fo", new List<Version>()));
        }

        [Fact]
        public void AddWithVersionName_ShouldRetrieveVersionsFromServer()
        {
            //arrange
            var mockSoapClient = new Mock<IJiraSoapServiceClient>();
            mockSoapClient.Setup(j => j.Url).Returns("http://foo:2990/jira/");

            var remoteVersions = new RemoteVersion[]
            {
                new RemoteVersion() 
                {
                    id = "1",
                    name = "1.0"
                }
            };

            mockSoapClient.Setup(m => m.GetVersions(It.IsAny<string>(), "fooproject")).Returns(remoteVersions);
            var jira = new Jira(null, mockSoapClient.Object, null, "user", "pass");

            //act
            var versions = new VersionList(jira, "fooproject", new List<Version>());
            VersionList.ClearCache();
            versions.Add("1.0");

            //assert
            Assert.Equal(1, versions.Count);
        }
    }
}
