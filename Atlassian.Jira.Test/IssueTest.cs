using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Atlassian.Jira.Remote;
using Moq;

namespace Atlassian.Jira.Test
{
    public class IssueTest
    {
        [Fact]
        public void Constructor_ShouldSetDefaultValues()
        {
            var issue = CreateIssue("ProjectKey");
            Assert.Equal(0, issue.AffectsVersions.Count);
            Assert.Null(issue.Assignee);
            Assert.Equal(0, issue.Components.Count);
            Assert.Null(issue.Created);
            Assert.Equal(0, issue.CustomFields.Count);
            Assert.Null(issue.Description);
            Assert.Null(issue.DueDate);
            Assert.Null(issue.Environment);
            Assert.Null(issue.Key);
            Assert.Null(issue.Priority);
            Assert.Equal("ProjectKey", issue.Project);
            Assert.Null(issue.Reporter);
            Assert.Null(issue.Resolution);
            Assert.Null(issue.Status);
            Assert.Null(issue.Summary);
            Assert.Null(issue.Type);
            Assert.Null(issue.Updated);
            Assert.Null(issue.Votes);
        }

        [Fact]
        public void ConstructorFromRemote_ShouldPopulateFields()
        {
            var remoteIssue = new RemoteIssue()
            {
                affectsVersions = new RemoteVersion[] { new RemoteVersion() { id = "remoteVersion"}},
                assignee = "assignee",
                components = new RemoteComponent[] { new RemoteComponent() { id = "remoteComponent"}},
                created = new DateTime(2011, 1, 1),
                customFieldValues = new RemoteCustomFieldValue[] { new RemoteCustomFieldValue() { customfieldId = "customField"} },
                description = "description",
                duedate = new DateTime(2011, 3, 3),
                environment = "environment",
                fixVersions = new RemoteVersion[] { new RemoteVersion() { id = "remoteFixVersion"}},
                key = "key",
                priority = "priority",
                project = "project",
                reporter = "reporter",
                resolution = "resolution",
                status = "status",
                summary = "summary",
                type = "type",
                updated = new DateTime(2011, 2, 2),
                votes = 1
            };

            var issue = remoteIssue.ToLocal();

            Assert.Equal(1, issue.AffectsVersions.Count);
            Assert.Equal("assignee", issue.Assignee);
            Assert.Equal(1, issue.Components.Count);
            Assert.Equal(new DateTime(2011, 1, 1), issue.Created);
            Assert.Equal(1, issue.CustomFields.Count);
            Assert.Equal("description", issue.Description);
            Assert.Equal(new DateTime(2011, 3, 3), issue.DueDate);
            Assert.Equal("environment", issue.Environment);
            Assert.Equal("key", issue.Key.Value);
            Assert.Equal("priority", issue.Priority.Value);
            Assert.Equal("project", issue.Project);
            Assert.Equal("reporter", issue.Reporter);
            Assert.Equal("resolution", issue.Resolution.Value);
            Assert.Equal("status", issue.Status);
            Assert.Equal("summary", issue.Summary);
            Assert.Equal("type", issue.Type.Id);
            Assert.Equal(new DateTime(2011, 2, 2), issue.Updated);
            Assert.Equal(1, issue.Votes);
        }

      

        [Fact]
        public void ToRemote_IfFieldsNotSet_ShouldLeaveFieldsNull()
        {
            var issue = CreateIssue("ProjectKey");

            var remoteIssue = issue.ToRemote();

            Assert.Null(remoteIssue.affectsVersions);
            Assert.Null(remoteIssue.assignee);
            Assert.Null(remoteIssue.components);
            Assert.Null(remoteIssue.created);
            Assert.Null(remoteIssue.customFieldValues);
            Assert.Null(remoteIssue.description);
            Assert.Null(remoteIssue.duedate);
            Assert.Null(remoteIssue.environment);
            Assert.Null(remoteIssue.key);
            Assert.Null(remoteIssue.priority);
            Assert.Equal("ProjectKey", remoteIssue.project);
            Assert.Null(remoteIssue.reporter);
            Assert.Null(remoteIssue.resolution);
            Assert.Null(remoteIssue.status);
            Assert.Null(remoteIssue.summary);
            Assert.Null(remoteIssue.type);
            Assert.Null(remoteIssue.updated);
            Assert.Null(remoteIssue.votes);
        }

        [Fact]
        public void ToRemote_IfFieldsSet_ShouldPopulateFields()
        {
            var issue = CreateIssue("ProjectKey");
            var version = new RemoteVersion().ToLocal();
            var component = new RemoteComponent().ToLocal();

            issue.AffectsVersions.Add(version);
            issue.Assignee = "assignee";
            

            var remoteIssue = new RemoteIssue()
            {
                affectsVersions = new RemoteVersion[] { new RemoteVersion() { id = "remoteVersion" } },
                assignee = "assignee",
                components = new RemoteComponent[] { new RemoteComponent() { id = "remoteComponent" } },
                created = new DateTime(2011, 1, 1),
                customFieldValues = new RemoteCustomFieldValue[] { new RemoteCustomFieldValue() { customfieldId = "customField" } },
                description = "description",
                duedate = new DateTime(2011, 3, 3),
                environment = "environment",
                fixVersions = new RemoteVersion[] { new RemoteVersion() { id = "remoteFixVersion" } },
                key = "key",
                priority = "priority",
                project = "project",
                reporter = "reporter",
                resolution = "resolution",
                status = "status",
                summary = "summary",
                type = "type",
                updated = new DateTime(2011, 2, 2),
                votes = 1
            };


            var remoteIssue = new RemoteIssue()
            {
                created = new DateTime(2011, 1, 1),
                updated = new DateTime(2011, 2, 2),
                duedate = new DateTime(2011, 3, 3),
                priority = "High",
                resolution = "Open",
                key = "key1"
            };

            var newRemoteIssue = remoteIssue.ToLocal().ToRemote();

            Assert.Null(newRemoteIssue.created);
            Assert.Null(newRemoteIssue.updated);
            Assert.Null(newRemoteIssue.duedate);
            Assert.Equal("High", newRemoteIssue.priority);
            Assert.Equal("Open", newRemoteIssue.resolution);
            Assert.Equal("key1", newRemoteIssue.key);
        }

        [Fact]
        public void ToRemote_IfVersionsAreSet_ShouldSetVersionsField()
        {
            var issue = CreateIssue();
            var affectsVersion = new RemoteVersion();
            var fixVersion = new RemoteVersion();

            issue.AffectsVersions.Add(affectsVersion.ToLocal());
            issue.FixVersions.Add(fixVersion.ToLocal());
            
            var remoteIssue = issue.ToRemote();
            Assert.Equal(1, remoteIssue.affectsVersions.Length);
            Assert.ReferenceEquals(affectsVersion, remoteIssue.affectsVersions[0]);

            Assert.Equal(1, remoteIssue.fixVersions.Length);
            Assert.ReferenceEquals(fixVersion, remoteIssue.fixVersions[0]);
        }

        [Fact]
        public void ToRemote_IfComponentsAreSet_ShouldSetComponentsField()
        {
            var issue = CreateIssue();
            var component = new RemoteComponent();

            issue.Components.Add(component.ToLocal());

            var remoteIssue = issue.ToRemote();

            Assert.Equal(1, remoteIssue.components.Length);
            Assert.Equal(component, remoteIssue.components[0]);
        }

        [Fact]
        public void ToRemote_IfNamedEntityNotSet_ShouldLeaveFieldNull()
        {
            var issue = CreateIssue();

            var remoteIssue = issue.ToRemote();

            Assert.Null(remoteIssue.type);
        }

        [Fact]
        public void GetUpdatedFields_ReturnEmptyIfNothingChanged()
        {
            var issue = CreateIssue();

            Assert.Equal(0, GetUpdatedFieldsForIssue(issue).Length);
        }

        [Fact]
        public void GetUpdatedFields_IfString_ReturnOneFieldThatChanged()
        {
            var issue = CreateIssue();
            issue.Summary = "foo";

            Assert.Equal(1, GetUpdatedFieldsForIssue(issue).Length);
        }

        [Fact]
        public void GetUpdatedFields_IfString_ReturnAllFieldsThatChanged()
        {
            var issue = CreateIssue();
            issue.Summary = "foo";
            issue.Description = "foo";
            issue.Assignee = "foo";
            issue.Environment = "foo";
            issue.Project = "foo";
            issue.Reporter = "foo";
            issue.Status = "foo";
            issue.Type = "foo";

            Assert.Equal(8, GetUpdatedFieldsForIssue(issue).Length);
        }

        [Fact]
        public void GetUpdateFields_IfStringEqual_ReturnNoFieldsThatChanged()
        {
            var remoteIssue = new RemoteIssue()
            {
                summary = "Summary"
            };

            var issue = remoteIssue.ToLocal();

            issue.Summary = "Summary";
            issue.Status = null;

            Assert.Equal(0, GetUpdatedFieldsForIssue(issue).Length);
        }

        [Fact]
        public void GetUpdateFields_IfComparableEqual_ReturnNoFieldsThatChanged()
        {
            var remoteIssue = new RemoteIssue()
            {
                priority = "High",
            };

            var issue = remoteIssue.ToLocal();

            issue.Priority = "High";
            issue.Resolution = null;

            Assert.Equal(0, GetUpdatedFieldsForIssue(issue).Length);
        }

        [Fact]
        public void GetUpdateFields_IfComparable_ReturnsFieldsThatChanged()
        {
            var issue = CreateIssue();
            issue.Priority = "High";

            Assert.Equal(1, GetUpdatedFieldsForIssue(issue).Length);
            
        }

        [Fact]
        public void GetUpdateFields_IfDateTimeChanged_ReturnsFieldsThatChanged()
        {
            var issue = CreateIssue();
            issue.DueDate = new DateTime(2011, 10, 10);

            var fields = GetUpdatedFieldsForIssue(issue);
            Assert.Equal(1, fields.Length);
            Assert.Equal("10/Oct/11", fields[0].values[0]);
        }

        [Fact]
        public void GetUpdateFields_IfDateTimeUnChangd_ShouldNotIncludeItInFieldsThatChanged()
        {
            var remoteIssue = new RemoteIssue()
            {
                duedate = new DateTime(2011,1,1)
            };

            var issue = remoteIssue.ToLocal();
            Assert.Equal(0, GetUpdatedFieldsForIssue(issue).Length);
        }

        [Fact]
        public void GetAttachments_IfIssueNotCreated_ShouldThrowException()
        {
            var issue = CreateIssue();

            Assert.Throws(typeof(InvalidOperationException), () => issue.GetAttachments());
        }

        [Fact]
        public void GetAttachments_IfIssueIsCreated_ShouldLoadAttachments()
        {
            //arrange
            var mockJiraService = new Mock<IJiraSoapServiceClient>();
            mockJiraService.Setup(j => j.Login("user", "pass")).Returns("thetoken");
            mockJiraService.Setup(j => j.GetAttachmentsFromIssue("thetoken", "key"))
                .Returns(new RemoteAttachment[1] { new RemoteAttachment() { filename = "attach.txt" } });
            
            var jira = new Jira(null, mockJiraService.Object, null, "user", "pass");
            var issue = (new RemoteIssue() { key = "key" }).ToLocal(jira);

            //act
            var attachments = issue.GetAttachments();

            //assert
            Assert.Equal(1, attachments.Count);
            Assert.Equal("attach.txt", attachments[0].FileName);
        }

        [Fact]
        public void AddLabels_IfIssueNotCreated_ShouldThrowAnException()
        {
            var issue = CreateIssue();
            Assert.Throws(typeof(InvalidOperationException), () => issue.AddLabels());
        }

        [Fact]
        public void AddAttachment_IfIssueNotCreated_ShouldThrowAnException()
        {
            var issue = CreateIssue();

            Assert.Throws(typeof(InvalidOperationException), () => issue.AddAttachment("foo", new byte[] { 1 } ));
        }

        [Fact]
        public void AddAttachment_IfIssueCreated_ShouldUpload()
        {
            //arrange
            var mockJiraService = new Mock<IJiraSoapServiceClient>();
            mockJiraService.Setup(j => j.Login("user", "pass")).Returns("token");
            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(f => f.FileReadAllBytes("foo.txt")).Returns(new byte[] { 1, 2, 3 });

            var jira = new Jira(null, mockJiraService.Object, mockFileSystem.Object, "user", "pass");
            var issue = (new RemoteIssue() { key = "key" }).ToLocal(jira);

            //act
            issue.AddAttachment("foo.txt");

            //assert
            // TODO: Need to modify this obscure assertion.
            mockJiraService.Verify(j => j.AddBase64EncodedAttachmentsToIssue(
                                                "token",
                                                "key",
                                                new string[] { "foo.txt" },
                                                new string[] { "AQID" }));
        }

        [Fact]
        public void GetComments_IfIssueNotCreated_ShouldThrowException()
        {
            var issue = CreateIssue();

            Assert.Throws(typeof(InvalidOperationException), () => issue.GetComments());
        }

        [Fact]
        public void GetComments_IfIssueIsCreated_ShouldLoadComments()
        {
            //arrange
            var mockJiraService = new Mock<IJiraSoapServiceClient>();
            mockJiraService.Setup(j => j.Login("user", "pass")).Returns("thetoken");
            mockJiraService.Setup(j => j.GetCommentsFromIssue("thetoken", "key"))
                .Returns(new RemoteComment[1] { new RemoteComment() { body = "the comment" } });

            var jira = new Jira(null, mockJiraService.Object, null, "user", "pass");
            var issue = (new RemoteIssue() { key = "key" }).ToLocal(jira);

            //act
            var comments = issue.GetComments();

            //assert
            Assert.Equal(1, comments.Count);
            Assert.Equal("the comment", comments[0].Body);
        }

        [Fact]
        public void AddComment_IfIssueNotCreated_ShouldThrowAnException()
        {
            var issue = CreateIssue();

            Assert.Throws(typeof(InvalidOperationException), () => issue.AddComment("foo"));
        }

        [Fact]
        public void AddComment_IfIssueCreated_ShouldUpload()
        {
            //arrange
            var mockJiraService = new Mock<IJiraSoapServiceClient>();
            mockJiraService.Setup(j => j.Login("user", "pass")).Returns("token");

            var jira = new Jira(null, mockJiraService.Object, null, "user", "pass");
            var issue = (new RemoteIssue() { key = "key" }).ToLocal(jira);

            //act
            issue.AddComment("the comment");

            //assert
            // TODO: Need to modify this obscure assertion.
            mockJiraService.Verify(j => j.AddComment(
                                                "token",
                                                "key",
                                                It.Is<RemoteComment>(r => r.body == "the comment" && r.author == "user")));
        }

        [Fact]
        public void Components_IfIssueNotCreated_ShouldReturnEmptyList()
        {
            var issue = CreateIssue();

            Assert.Equal(0, issue.Components.Count);
        }

        [Fact]
        public void Versions_IfIssueNotCreated_ShouldReturnEmptyList()
        {
            var issue = CreateIssue();

            Assert.Equal(0, issue.AffectsVersions.Count());
            Assert.Equal(0, issue.FixVersions.Count());
        }

        [Fact]
        public void Components_IfIssueCreated_ShouldReturnComponents()
        {
            var remoteIssue = new RemoteIssue();
            remoteIssue.components = new RemoteComponent[]{
                new RemoteComponent(){
                    id = "1",
                    name = "Server"
                }
            };

            var issue = remoteIssue.ToLocal();

            Assert.Equal(1, issue.Components.Count);
            Assert.Equal("1", issue.Components[0].Id);
            Assert.Equal("Server", issue.Components[0].Name);
        }

        [Fact]
        public void Versions_IfIssueCreated_ShouldReturnVersions()
        {
            var remoteIssue = new RemoteIssue();
            remoteIssue.affectsVersions = new RemoteVersion[] { new RemoteVersion() 
                                                                    { id = "1", 
                                                                      name = "1.0",
                                                                      archived = true,
                                                                      released = true,
                                                                      releaseDate = new DateTime(2011,1,1)
                                                                    } };
            remoteIssue.fixVersions = new RemoteVersion[] { new RemoteVersion() 
                                                                    { id = "2", 
                                                                      name = "2.0",
                                                                      archived = true,
                                                                      released = true,
                                                                      releaseDate = new DateTime(2011,1,1)
                                                                    } };

            var issue = remoteIssue.ToLocal();

            Assert.Equal(1, issue.AffectsVersions.Count());
            Assert.Equal("1", issue.AffectsVersions.ElementAt(0).Id);
            Assert.Equal("1.0", issue.AffectsVersions.ElementAt(0).Name);
            Assert.True(issue.AffectsVersions.ElementAt(0).IsArchived);
            Assert.True(issue.AffectsVersions.ElementAt(0).IsReleased);
            Assert.Equal(new DateTime(2011, 1, 1), issue.AffectsVersions.ElementAt(0).ReleasedDate);

            Assert.Equal(1, issue.FixVersions.Count());
            Assert.Equal("2", issue.FixVersions.ElementAt(0).Id);
            Assert.Equal("2.0", issue.FixVersions.ElementAt(0).Name);
            Assert.True(issue.FixVersions.ElementAt(0).IsArchived);
            Assert.True(issue.FixVersions.ElementAt(0).IsReleased);
            Assert.Equal(new DateTime(2011, 1, 1), issue.FixVersions.ElementAt(0).ReleasedDate);
        }

        [Fact]
        public void GetUpdatedFields_IfComponentsAdded_ReturnsFields()
        {
            var issue = new RemoteIssue() { key = "foo" }.ToLocal();
            var component = new RemoteComponent() { id = "1", name = "1.0" };
            issue.Components.Add(component.ToLocal());

            var fields = GetUpdatedFieldsForIssue(issue);
            Assert.Equal(1, fields.Length);
            Assert.Equal("components", fields[0].id);
            Assert.Equal("1", fields[0].values[0]);
        }

        [Fact]
        public void GetUpdatedFields_IfAddFixVersion_ReturnAllFieldsThatChanged()
        {
            var issue = new RemoteIssue() { key = "foo" }.ToLocal();
            var version = new RemoteVersion() { id = "1", name = "1.0" };
            issue.FixVersions.Add(version.ToLocal());

            var fields = GetUpdatedFieldsForIssue(issue);
            Assert.Equal(1, fields.Length);
            Assert.Equal("fixVersions", fields[0].id);
            Assert.Equal("1", fields[0].values[0]);
        }

        [Fact]
        public void GetUpdatedFields_IfAddAffectsVersion_ReturnAllFieldsThatChanged()
        {
            var issue = new RemoteIssue() { key = "foo" }.ToLocal();
            var version = new RemoteVersion() { id = "1", name = "1.0" };
            issue.AffectsVersions.Add(version.ToLocal());

            var fields = GetUpdatedFieldsForIssue(issue);
            Assert.Equal(1, fields.Length);
            Assert.Equal("versions", fields[0].id);
            Assert.Equal("1", fields[0].values[0]);
        }

        [Fact]
        public void CustomField_ShouldReturnRemoteValue()
        {
            //arrange
            var soapClient = new Mock<IJiraSoapServiceClient>();
            soapClient.Setup(c => c.Login("user", "pass")).Returns("token");
            soapClient.Setup(c => c.GetIssuesFromJqlSearch(It.IsAny<string>(), "project = \"bar\"", 1)).Returns(new RemoteIssue[] {
                                        new RemoteIssue() { key = "123" }});
            soapClient.Setup(c => c.GetFieldsForEdit(It.IsAny<string>(), "123")).Returns(new RemoteField[] { 
                new RemoteField(){ id="123", name= "CustomField" }});

            var jira = new Jira(null, soapClient.Object, null, "user", "pass");

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

        private Issue CreateIssue(string project = "TST")
        {
            var translator = new Mock<IJqlExpressionVisitor>();
            var soapClient = new Mock<IJiraSoapServiceClient>();
            var jira = new Jira(translator.Object, soapClient.Object, null, "username", "password");

            return new Issue(jira, project);
        }

        private RemoteFieldValue[] GetUpdatedFieldsForIssue(Issue issue)
        {
            return ((IRemoteIssueFieldProvider)issue).GetRemoteFields();
        }
    }
}
