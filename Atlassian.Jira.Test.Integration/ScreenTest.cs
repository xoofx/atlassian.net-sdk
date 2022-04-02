using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test.Integration
{
    /// <remarks>
    /// Screen URL used in this test: http://localhost:8080/plugins/servlet/project-config/TST/screens/1
    /// </remarks>
    public class ScreenTest
    {
        private const string SCREEN_ID = "1";
        private const string SCREEN_TAB_ID = "10110";

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task GetScreenAvailableFields(Jira jira)
        {
            var screenAvailableFields = await jira.Screens.GetScreenAvailableFieldsAsync(SCREEN_ID);

            // Verify there's at least one available field.
            Assert.Contains(screenAvailableFields, x => x.Name.Equals("Development", StringComparison.InvariantCultureIgnoreCase));
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task GetScreenTabs(Jira jira)
        {
            var screenTabs = await jira.Screens.GetScreenTabsAsync(SCREEN_ID);

            // Verify there's two tabs.
            Assert.Equal(2, screenTabs.Count());

            // Verify there's the "Extra Tab" tab.
            var screenTab = screenTabs.FirstOrDefault(x => x.Name.Equals("Extra Tab", StringComparison.InvariantCultureIgnoreCase));
            Assert.NotNull(screenTab);
            Assert.NotNull(screenTab.Id);
        }

        [Theory]
        [ClassData(typeof(JiraProvider))]
        public async Task GetScreenTabFields(Jira jira)
        {
            var screenTabFields = await jira.Screens.GetScreenTabFieldsAsync(SCREEN_ID, SCREEN_TAB_ID);

            // Verify there's two fields in the "Extra Tab" tab.
            Assert.Equal(2, screenTabFields.Count());

            // Verify the fields have a name and a type.
            var field = screenTabFields.FirstOrDefault(x => x.Name.Equals("Epic Name", StringComparison.InvariantCultureIgnoreCase));
            Assert.NotNull(field);
            Assert.NotNull(field.Id);
            Assert.Equal("Name of Epic", field.Type);
        }
    }
}
