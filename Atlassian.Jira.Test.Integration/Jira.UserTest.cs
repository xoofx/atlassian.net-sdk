using System;
using System.Linq;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class JiraUserTest
    {
        private readonly Random _random = new Random();

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
            Assert.NotNull(user.Key);
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
        public void CreateGetAndDeleteUsersWithEmailAsUsername(Jira jira)
        {
            var userInfo = BuildUserInfo();
            userInfo.Username = userInfo.Email;

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
    }
}
