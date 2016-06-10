using Atlassian.Jira.Remote;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Atlassian.Jira.Test
{
    public class CustomFieldTest
    {
        [Fact]
        public void Name_ShouldRetriveValueFromRemote()
        {
            //arrange
            var jira = TestableJira.Create();
            var customField = new CustomField(new RemoteField() { id = "123", name = "CustomField" });
            jira.IssueFieldService.Setup(c => c.GetCustomFieldsAsync(CancellationToken.None))
                .Returns(Task.FromResult(Enumerable.Repeat<CustomField>(customField, 1)));

            var issue = new RemoteIssue()
            {
                project = "projectKey",
                key = "issueKey",
                customFieldValues = new RemoteCustomFieldValue[]{
                                new RemoteCustomFieldValue(){
                                    customfieldId = "123",
                                    values = new string[] {"abc"}
                                }
                            }
            }.ToLocal(jira);

            //assert
            Assert.Equal("CustomField", issue.CustomFields[0].Name);
        }
    }
}
