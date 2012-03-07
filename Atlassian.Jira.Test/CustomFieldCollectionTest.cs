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
        public void IndexByName_ShouldReturnRemoteValue()
        {
            //arrange
            var jira = TestableJira.Create();
            jira.SoapService
                .Setup(c => c.GetIssuesFromJqlSearch(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(new RemoteIssue[] 
                {
                    new RemoteIssue() { key = "123" }
                });
            jira.SoapService.Setup(c => c.GetFieldsForEdit(It.IsAny<string>(), "123")).Returns(new RemoteField[] { 
                new RemoteField(){ id="123", name= "CustomField" }});

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

            //assert
            Assert.Equal("abc", issue["CustomField"]);
            Assert.Equal("123", issue.CustomFields["CustomField"].Id);

            issue["customfield"] = "foobar";
            Assert.Equal("foobar", issue["customfield"]);
        }
    }
}
