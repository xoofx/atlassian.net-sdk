using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class UserTest : BaseIntegrationTest
    {
        [Fact]
        public void TestGetUser()
        {
            var user = _jira.GetUserAsync("admin").Result;
            Assert.Equal(user.Email, "admin@example.com");
            Assert.Equal(user.DisplayName, "admin");
            Assert.Equal(user.Username, "admin");
            Assert.Equal(user.IsActive, true);
        }
    }
}
