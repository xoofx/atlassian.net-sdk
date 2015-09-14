using Atlassian.Jira.Remote;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            jira.SoapService.Setup(c => c.GetFieldsForEdit(It.IsAny<string>(), "issueKey")).Returns(new RemoteField[] {
                new RemoteField(){ id="123", name= "CustomField" }});

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
