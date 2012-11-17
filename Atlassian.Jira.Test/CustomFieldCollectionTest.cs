using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Atlassian.Jira.Remote;
using Moq;

namespace Atlassian.Jira.Test
{
    public class CustomFieldCollectionTest
    {
        [Fact]
        public void IndexByName_ShouldThrowIfUnableToFindRemoteValue()
        {
            var jira = TestableJira.Create();
            jira.SoapService
               .Setup(c => c.GetIssuesFromJqlSearch(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
               .Returns(new RemoteIssue[] 
                {
                    new RemoteIssue() { key = "123" }
                });

            var issue = new RemoteIssue()
            {
                project = "bar",
                key = "foo",
                customFieldValues = new RemoteCustomFieldValue[]{
                                new RemoteCustomFieldValue(){
                                    customfieldId = "123",
                                    values = new string[] {"abc"}
                                }
                            }
            }.ToLocal(jira);

            Assert.Throws(typeof (InvalidOperationException), () => issue["CustomField"]);
        }

        [Fact]
        public void IndexByName_ShouldReturnRemoteValue()
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
            Assert.Equal("abc", issue["CustomField"]);
            Assert.Equal("123", issue.CustomFields["CustomField"].Id);

            issue["customfield"] = "foobar";
            Assert.Equal("foobar", issue["customfield"]);
        }

        [Fact]
        public void CanSwitchContextToRetrieveDifferentTypeOfCustomFields()
        {
            // Arrange
            var jira = TestableJira.Create();
            jira.SoapService.Setup(c => c.GetFieldsForEdit(It.IsAny<string>(), "issueKey")).Returns(new RemoteField[] { 
                new RemoteField(){ id="editField1", name= "EditCustomField" }});
            jira.SoapService.Setup(c => c.GetFieldsForAction(It.IsAny<string>(), "issueKey", "action1")).Returns(new RemoteField[] { 
                new RemoteField(){ id="actionField1", name= "ActionCustomField" }});

            var issue = new RemoteIssue()
            {
                project = "projectKey",
                key = "issueKey",
                customFieldValues = new RemoteCustomFieldValue[]{
                    new RemoteCustomFieldValue() {
                        customfieldId = "editField1",
                        values = new string[] {"editFieldValue"}
                    },
                    new RemoteCustomFieldValue() {
                        customfieldId = "actionField1",
                        values = new string[] {"actionFieldValue"}
                    },
                }
            }.ToLocal(jira);

            // Act/Assert
            var fields = issue.CustomFields;
            Assert.Equal("actionFieldValue", fields.ForAction("action1")["ActionCustomField"].Values[0]);
            Assert.Equal("editFieldValue", fields.ForEdit()["EditCustomField"].Values[0]);
        }

        [Fact]
        public void CanSwitchContextToSetDifferentTypeOfCustomFields()
        {
            // Arrange
            var jira = TestableJira.Create();
            jira.SoapService.Setup(c => c.GetFieldsForEdit(It.IsAny<string>(), "issueKey")).Returns(new RemoteField[] { 
                new RemoteField(){ id="editField1", name= "EditCustomField" }});
            jira.SoapService.Setup(c => c.GetFieldsForAction(It.IsAny<string>(), "issueKey", "action1")).Returns(new RemoteField[] { 
                new RemoteField(){ id="actionField1", name= "ActionCustomField" }});

            var issue = new RemoteIssue()
            {
                project = "projectKey",
                key = "issueKey",
                customFieldValues = null,
            }.ToLocal(jira);

            // Act
            issue.CustomFields
                .ForAction("action1").Add("ActionCustomField", "actionFieldValue")
                .ForEdit().Add("EditCustomField", "editFieldValue");

            // Assert
            Assert.Equal(2, issue.CustomFields.Count);
        }

        [Fact]
        public void WillThrowErrorIfCustomFieldNotFoundInContextSpecified()
        {
            // Arrange
            var jira = TestableJira.Create();
            jira.SoapService.Setup(c => c.GetFieldsForEdit(It.IsAny<string>(), "issueKey")).Returns(new RemoteField[] { 
                new RemoteField(){ id="editField1", name= "EditCustomField" }});
            jira.SoapService.Setup(c => c.GetFieldsForAction(It.IsAny<string>(), "issueKey", "action1")).Returns(new RemoteField[] { 
                new RemoteField(){ id="actionField1", name= "ActionCustomField" }});

            var issue = new RemoteIssue()
            {
                project = "projectKey",
                key = "issueKey",
                customFieldValues = null,
            }.ToLocal(jira);

            // Act / Assert
            var fields = issue.CustomFields;
            Assert.Throws<InvalidOperationException>(() => fields.ForAction("action1")["EditCustomField"].Values[0]);
            Assert.Throws<InvalidOperationException>(() => fields.ForEdit()["ActionCustomField"].Values[0]);
        }
    }
}
