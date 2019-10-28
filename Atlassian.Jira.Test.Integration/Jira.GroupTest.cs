using System;
using System.Linq;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class JiraGroupTest
    {
        private readonly Random _random = new Random();

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void CreateAndRemoveGroupWithUser(Jira jira)
        {
            // Create the group.
            var groupName = $"test-group-{_random.Next(int.MaxValue)}";
            jira.Groups.CreateGroupAsync(groupName).Wait();

            // Add user to group
            jira.Groups.AddUserAsync(groupName, "admin").Wait();

            // Get users from group.
            var users = jira.Groups.GetUsersAsync(groupName).Result;
            Assert.Contains(users, u => u.Username == "admin");

            // Delete user from group.
            jira.Groups.RemoveUserAsync(groupName, "admin").Wait();
            users = jira.Groups.GetUsersAsync(groupName).Result;
            Assert.Empty(users);

            // Delete group
            jira.Groups.DeleteGroupAsync(groupName).Wait();
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public void CreateAndRemoveGroupWithSpecialCharacterAndUser(Jira jira)
        {
            // Create the group.
            var groupName = $"test-group-@@@@-{_random.Next(int.MaxValue)}";
            jira.Groups.CreateGroupAsync(groupName).Wait();

            // Add user to group
            jira.Groups.AddUserAsync(groupName, "admin").Wait();

            // Get users from group.
            var users = jira.Groups.GetUsersAsync(groupName).Result;
            Assert.Contains(users, u => u.Username == "admin");

            // Delete user from group.
            jira.Groups.RemoveUserAsync(groupName, "admin").Wait();
            users = jira.Groups.GetUsersAsync(groupName).Result;
            Assert.Empty(users);

            // Delete group
            jira.Groups.DeleteGroupAsync(groupName).Wait();
        }
    }
}
