using System;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task CreateGetAndDeleteUsersWithEmailAsUsername(Jira jira)
        {
            var userInfo = BuildUserInfo();
            userInfo.Username = userInfo.Email;

            // verify create a user.
            var user = await jira.Users.CreateUserAsync(userInfo);
            Assert.Equal(user.Email, userInfo.Email);
            Assert.Equal(user.DisplayName, userInfo.DisplayName);
            Assert.Equal(user.Username, userInfo.Username);
            Assert.NotNull(user.Key);
            Assert.True(user.IsActive);
            Assert.False(String.IsNullOrEmpty(user.Locale));

            // verify retrieve a user.
            user = await jira.Users.GetUserAsync(userInfo.Username);
            Assert.Equal(user.DisplayName, userInfo.DisplayName);

            // verify search for a user
            var users = await jira.Users.SearchUsersAsync("test");
            Assert.Contains(users, u => u.Username == userInfo.Username);

            // verify equality override (see https://bitbucket.org/farmas/atlassian.net-sdk/issues/570)
            Assert.True(users.First().Equals(users.First()));

            // verify delete a user
            await jira.Users.DeleteUserAsync(userInfo.Username);
            users = await jira.Users.SearchUsersAsync(userInfo.Username);
            Assert.Empty(users);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task CanAccessAvatarUrls(Jira jira)
        {
            var user = await jira.Users.GetUserAsync("admin");
            Assert.NotNull(user.AvatarUrls);
            Assert.NotNull(user.AvatarUrls.XSmall);
            Assert.NotNull(user.AvatarUrls.Small);
            Assert.NotNull(user.AvatarUrls.Medium);
            Assert.NotNull(user.AvatarUrls.Large);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task SearchAssignableUsersForIssue(Jira jira)
        {
            var userInfo = BuildUserInfo();
            userInfo.Username = userInfo.Email;

            // verify create a user.
            var user = await jira.Users.CreateUserAsync(userInfo);
            Assert.NotNull(user);

            // any user can be assigned to SCRUM issues.
            var users = await jira.Users.SearchAssignableUsersForIssueAsync("test", "SCRUM-1");
            Assert.NotNull(users.FirstOrDefault(u => u.Username == user.Username));

            // only developers can be assigned to TST issues.
            users = await jira.Users.SearchAssignableUsersForIssueAsync("test", "TST-1");
            Assert.Null(users.FirstOrDefault(u => u.Username == user.Username));

            // verify delete a user
            await jira.Users.DeleteUserAsync(userInfo.Username);
            users = await jira.Users.SearchUsersAsync(userInfo.Username);
            Assert.Empty(users);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task SearchAssignableUsersForProject(Jira jira)
        {
            var userInfo = BuildUserInfo();
            userInfo.Username = userInfo.Email;

            // verify create a user.
            var user = await jira.Users.CreateUserAsync(userInfo);
            Assert.NotNull(user);

            // any user can be assigned to SCRUM issues.
            var users = await jira.Users.SearchAssignableUsersForProjectAsync("test", "SCRUM");
            Assert.NotNull(users.FirstOrDefault(u => u.Username == user.Username));

            // only developers can be assigned to TST issues.
            users = await jira.Users.SearchAssignableUsersForProjectAsync("test", "TST");
            Assert.Null(users.FirstOrDefault(u => u.Username == user.Username));

            // verify delete a user
            await jira.Users.DeleteUserAsync(userInfo.Username);
            users = await jira.Users.SearchUsersAsync(userInfo.Username);
            Assert.Empty(users);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task SearchAssignableUsersForProjects(Jira jira)
        {
            var userInfo = BuildUserInfo();
            userInfo.Username = userInfo.Email;

            // verify create a user.
            var user = await jira.Users.CreateUserAsync(userInfo);
            Assert.NotNull(user);

            // test user is assignable because any user can be assigned to SCRUM issues.
            var users = await jira.Users.SearchAssignableUsersForProjectsAsync("test", new[] { "SCRUM" });
            Assert.NotNull(users.FirstOrDefault(u => u.Username == user.Username));

            // test user is not assignable because only developers can be assigned to TST issues.
            users = await jira.Users.SearchAssignableUsersForProjectsAsync("test", new[] { "TST" });
            Assert.Null(users.FirstOrDefault(u => u.Username == user.Username));

            // test user is not assignable because only developers can be assigned to both SCRUM and TST issues.
            users = await jira.Users.SearchAssignableUsersForProjectsAsync("test", new[] { "SCRUM", "TST" });
            Assert.Null(users.FirstOrDefault(u => u.Username == user.Username));

            // verify delete a user
            await jira.Users.DeleteUserAsync(userInfo.Username);
            users = await jira.Users.SearchUsersAsync(userInfo.Username);
            Assert.Empty(users);
        }
    }
}
