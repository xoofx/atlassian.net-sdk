using System;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    public class JiraGroupTest
    {
        private readonly Random _random = new Random();

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task CreateAndRemoveGroupWithUser(Jira jira)
        {
            // Create the group.
            var groupName = $"test-group-{_random.Next(int.MaxValue)}";
            await jira.Groups.CreateGroupAsync(groupName);

            // Add user to group
            await jira.Groups.AddUserAsync(groupName, "admin");

            // Get users from group.
            var users = await jira.Groups.GetUsersAsync(groupName);
            Assert.Contains(users, u => u.Username == "admin");

            // Delete user from group.
            await jira.Groups.RemoveUserAsync(groupName, "admin");
            users = await jira.Groups.GetUsersAsync(groupName);
            Assert.Empty(users);

            // Delete group
            await jira.Groups.DeleteGroupAsync(groupName);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task CreateAndRemoveGroupWithSpecialCharacterAndUser(Jira jira)
        {
            // Create the group.
            var groupName = $"test-group-@@@@-{_random.Next(int.MaxValue)}";
            await jira.Groups.CreateGroupAsync(groupName);

            // Add user to group
            await jira.Groups.AddUserAsync(groupName, "admin");

            // Get users from group.
            var users = await jira.Groups.GetUsersAsync(groupName);
            Assert.Contains(users, u => u.Username == "admin");

            // Delete user from group.
            await jira.Groups.RemoveUserAsync(groupName, "admin");
            users = await jira.Groups.GetUsersAsync(groupName);
            Assert.Empty(users);

            // Delete group
            await jira.Groups.DeleteGroupAsync(groupName);
        }
    }
}
