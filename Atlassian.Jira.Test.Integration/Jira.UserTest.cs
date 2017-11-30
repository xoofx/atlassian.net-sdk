using System;
using System.Linq;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class JiraUserTest : BaseIntegrationTest
    {
        private JiraUserCreationInfo BuildUserInfo()
        {
            var rand = _random.Next(int.MaxValue);
            return new JiraUserCreationInfo()
            {
                Username = "TestUser" + rand,
                DisplayName = "Test User " + rand,
                Email = String.Format("test{0}@user.com", rand),
                Password = "MyPass" + rand
            };
        }

        [Fact]
        public void CreateGetAndDeleteUsers()
        {
            var userInfo = BuildUserInfo();

            // verify create a user.
            var user = _jira.Users.CreateUserAsync(userInfo).Result;
            Assert.Equal(user.Email, userInfo.Email);
            Assert.Equal(user.DisplayName, userInfo.DisplayName);
            Assert.Equal(user.Username, userInfo.Username);
            Assert.True(user.IsActive);
            Assert.False(String.IsNullOrEmpty(user.Locale));
            // verify retrieve a user.
            user = _jira.Users.GetUserAsync(userInfo.Username).Result;
            Assert.Equal(user.DisplayName, userInfo.DisplayName);

            // verify search for a user
            var users = _jira.Users.SearchUsersAsync("TestUser").Result;
            Assert.Contains(users, u => u.Username == userInfo.Username);

            // verify delete a user
            _jira.Users.DeleteUserAsync(userInfo.Username).Wait();
            users = _jira.Users.SearchUsersAsync(userInfo.Username).Result;
            Assert.Empty(users);
        }

        [Fact]
        public void AddAndRemoveUserFromGroup()
        {
            // Create the group.
            var groupName = "test-group-" + _random.Next(int.MaxValue);
            _jira.Groups.CreateGroupAsync(groupName).Wait();

            // add user to group
            _jira.Groups.AddUserAsync(groupName, "admin").Wait();

            // get users from group.
            var users = _jira.Groups.GetUsersAsync(groupName).Result;
            Assert.Equal("admin", users.First().Username);

            // delete user from group.
            _jira.Groups.RemoveUserAsync(groupName, "admin").Wait();
            users = _jira.Groups.GetUsersAsync(groupName).Result;
            Assert.Empty(users);

            // delete group
            _jira.Groups.DeleteGroupAsync(groupName).Wait();
        }
    }
}
