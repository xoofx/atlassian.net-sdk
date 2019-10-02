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
                Username = "test" + rand,
                DisplayName = "Test User " + rand,
                Email = String.Format("test{0}@user.com", rand),
                Password = "MyPass" + rand
            };
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void CreateGetAndDeleteUsers(Jira jira)
        {
            var userInfo = BuildUserInfo();

            // verify create a user.
            var user = jira.Users.CreateUserAsync(userInfo).Result;
            Assert.Equal(user.Email, userInfo.Email);
            Assert.Equal(user.DisplayName, userInfo.DisplayName);
            Assert.Equal(user.Username, userInfo.Username);
            Assert.Equal(user.Key, userInfo.Username);
            Assert.True(user.IsActive);
            Assert.False(String.IsNullOrEmpty(user.Locale));

            // verify retrieve a user.
            user = jira.Users.GetUserAsync(userInfo.Username).Result;
            Assert.Equal(user.DisplayName, userInfo.DisplayName);

            // verify search for a user
            var users = jira.Users.SearchUsersAsync("test").Result;
            Assert.Contains(users, u => u.Username == userInfo.Username);

            // verify delete a user
            jira.Users.DeleteUserAsync(userInfo.Username).Wait();
            users = jira.Users.SearchUsersAsync(userInfo.Username).Result;
            Assert.Empty(users);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void AddAndRemoveUserFromGroup(Jira jira)
        {
            // Create the group.
            var groupName = "test-group-" + _random.Next(int.MaxValue);
            jira.Groups.CreateGroupAsync(groupName).Wait();

            // add user to group
            jira.Groups.AddUserAsync(groupName, "admin").Wait();

            // get users from group.
            var users = jira.Groups.GetUsersAsync(groupName).Result;
            Assert.Equal("admin", users.First().Username);

            // delete user from group.
            jira.Groups.RemoveUserAsync(groupName, "admin").Wait();
            users = jira.Groups.GetUsersAsync(groupName).Result;
            Assert.Empty(users);

            // delete group
            jira.Groups.DeleteGroupAsync(groupName).Wait();
        }
    }
}
