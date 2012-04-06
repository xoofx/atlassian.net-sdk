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
        public class Constructor
        {
            [Fact]
            public void ShouldSetDefaultValues()
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
            public void FromRemote_ShouldPopulateFields()
            {
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
                Assert.Equal("priority", issue.Priority.Id);
                Assert.Equal("project", issue.Project);
                Assert.Equal("reporter", issue.Reporter);
                Assert.Equal("resolution", issue.Resolution.Id);
                Assert.Equal("status", issue.Status.Id);
                Assert.Equal("summary", issue.Summary);
                Assert.Equal("type", issue.Type.Id);
                Assert.Equal(new DateTime(2011, 2, 2), issue.Updated);
                Assert.Equal(1, issue.Votes);
            }
        }

        public class ToRemote
        {
            [Fact]
            public void IfFieldsNotSet_ShouldLeaveFieldsNull()
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
            public void IfFieldsSet_ShouldPopulateFields()
            {
                var issue = CreateIssue("ProjectKey");
                var version = new RemoteVersion().ToLocal();
                var component = new RemoteComponent().ToLocal();

                issue.AffectsVersions.Add(version);
                issue.Assignee = "assignee";
                issue.Components.Add(component);
                // issue.CustomFields <-- requires extra setup, test below
                issue.Description = "description";
                issue.DueDate = new DateTime(2011, 1, 1);
                issue.Environment = "environment";
                issue.FixVersions.Add(version);
                // issue.Key <-- should be non-settable
                issue.Priority = "1";
                // issue.Project <-- should be non-settable
                issue.Reporter = "reporter";
                issue.Summary = "summary";
                issue.Type = "4";
                issue.Votes = 1;

                var remoteIssue = issue.ToRemote();

                Assert.Equal(1, remoteIssue.affectsVersions.Length);
                Assert.Equal("assignee", remoteIssue.assignee);
                Assert.Equal(1, remoteIssue.components.Length);
                Assert.Null(remoteIssue.created);
                //Assert.Equal(remoteIssue.customFieldValues);
                Assert.Equal("description", remoteIssue.description);
                Assert.Equal(new DateTime(2011, 1, 1), remoteIssue.duedate);
                Assert.Equal("environment", remoteIssue.environment);
                Assert.Null(remoteIssue.key);
                Assert.Equal("1", remoteIssue.priority);
                Assert.Equal("ProjectKey", remoteIssue.project);
                Assert.Equal("reporter", remoteIssue.reporter);
                Assert.Null(remoteIssue.resolution);
                Assert.Null(remoteIssue.status);
                Assert.Equal("summary", remoteIssue.summary);
                Assert.Equal("4", remoteIssue.type);
                Assert.Null(remoteIssue.updated);
                Assert.Equal(1, remoteIssue.votes);
            }

            [Fact]
            public void ToRemote_IfTypeSetByName_FetchId()
            {
                var jira = TestableJira.Create();
                var issue = jira.CreateIssue("ProjectKey");
                jira.SoapService.Setup(s => s.GetIssueTypes(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(new RemoteIssueType[]{
                    new RemoteIssueType() { id = "1", name = "Bug"}});

                issue.Type = "Bug";

                var remoteIssue = issue.ToRemote();
                Assert.Equal("1", remoteIssue.type);
            }
        }

        public class GetUpdatedFields
        {
            [Fact]
            public void IfJiraNamedPropertyWithId_ReturnField()
            {
                var issue = CreateIssue();
                issue.Type = "5";

                var result = GetUpdatedFieldsForIssue(issue);
                Assert.Equal(1, result.Length);
                Assert.Equal("5", result[0].values[0]);
            }

            [Fact]
            public void IfJiraNamedPropertyWithName_ReturnsFieldWithIdInferred()
            {
                var jira = TestableJira.Create();
                jira.SoapService.Setup(s => s.GetIssueTypes(It.IsAny<string>(), It.IsAny<string>()))
                                .Returns(new RemoteIssueType[]{
                                    new RemoteIssueType() { id ="2", name="Task" }
                                });
                var issue = jira.CreateIssue("FOO");
                issue.Type = "Task";

                var result = GetUpdatedFieldsForIssue(issue);
                Assert.Equal(1, result.Length);
                Assert.Equal("2", result[0].values[0]);
            }

            [Fact]
            public void IfJiraNamedPropertyWithNameNotChanged_ReturnsNoFieldsChanged()
            {
                var jira = TestableJira.Create();
                jira.SoapService.Setup(s => s.GetIssueTypes(It.IsAny<string>(), It.IsAny<string>()))
                                .Returns(new RemoteIssueType[]{
                                    new RemoteIssueType() { id ="5", name="Task" }
                                });
                var remoteIssue = new RemoteIssue()
                {
                    type = "5",
                };

                var issue = remoteIssue.ToLocal(jira);
                issue.Type = "Task";

                Assert.Equal(0, GetUpdatedFieldsForIssue(issue).Length);
            }

            [Fact]
            public void ReturnEmptyIfNothingChanged()
            {
                var issue = CreateIssue();

                Assert.Equal(0, GetUpdatedFieldsForIssue(issue).Length);
            }

            [Fact]
            public void IfString_ReturnOneFieldThatChanged()
            {
                var issue = CreateIssue();
                issue.Summary = "foo";

                Assert.Equal(1, GetUpdatedFieldsForIssue(issue).Length);
            }

            [Fact]
            public void IfString_ReturnAllFieldsThatChanged()
            {
                var issue = CreateIssue();
                issue.Summary = "foo";
                issue.Description = "foo";
                issue.Assignee = "foo";
                issue.Environment = "foo";
                issue.Reporter = "foo";
                issue.Type = "2";
                issue.Resolution = "3";
                issue.Priority = "4";

                Assert.Equal(8, GetUpdatedFieldsForIssue(issue).Length);
            }

            [Fact]
            public void IfStringEqual_ReturnNoFieldsThatChanged()
            {
                var remoteIssue = new RemoteIssue()
                {
                    summary = "Summary"
                };

                var issue = remoteIssue.ToLocal();

                issue.Summary = "Summary";

                Assert.Equal(0, GetUpdatedFieldsForIssue(issue).Length);
            }

            [Fact]
            public void IfComparableEqual_ReturnNoFieldsThatChanged()
            {
                var remoteIssue = new RemoteIssue()
                {
                    priority = "5",
                };

                var issue = remoteIssue.ToLocal();

                issue.Priority = "5";

                Assert.Equal(0, GetUpdatedFieldsForIssue(issue).Length);
            }

            [Fact]
            public void IfComparable_ReturnsFieldsThatChanged()
            {
                var issue = CreateIssue();
                issue.Priority = "5";

                Assert.Equal(1, GetUpdatedFieldsForIssue(issue).Length);

            }

            [Fact]
            public void IfDateTimeChanged_ReturnsFieldsThatChanged()
            {
                var issue = CreateIssue();
                issue.DueDate = new DateTime(2011, 10, 10);

                var fields = GetUpdatedFieldsForIssue(issue);
                Assert.Equal(1, fields.Length);
                Assert.Equal("10/Oct/11", fields[0].values[0]);
            }

            [Fact]
            public void IfDateTimeUnChangd_ShouldNotIncludeItInFieldsThatChanged()
            {
                var remoteIssue = new RemoteIssue()
                {
                    duedate = new DateTime(2011, 1, 1)
                };

                var issue = remoteIssue.ToLocal();
                Assert.Equal(0, GetUpdatedFieldsForIssue(issue).Length);
            }

            [Fact]
            public void IfComponentsAdded_ReturnsFields()
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
            public void IfAddFixVersion_ReturnAllFieldsThatChanged()
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
            public void IfAddAffectsVersion_ReturnAllFieldsThatChanged()
            {
                var issue = new RemoteIssue() { key = "foo" }.ToLocal();
                var version = new RemoteVersion() { id = "1", name = "1.0" };
                issue.AffectsVersions.Add(version.ToLocal());

                var fields = GetUpdatedFieldsForIssue(issue);
                Assert.Equal(1, fields.Length);
                Assert.Equal("versions", fields[0].id);
                Assert.Equal("1", fields[0].values[0]);
            }
        }

        public class GetAttachments
        {
            [Fact]
            public void IfIssueNotCreated_ShouldThrowException()
            {
                var issue = CreateIssue();

                Assert.Throws(typeof(InvalidOperationException), () => issue.GetAttachments());
            }

            [Fact]
            public void IfIssueIsCreated_ShouldLoadAttachments()
            {
                //arrange
                var jira = TestableJira.Create();
                jira.SoapService.Setup(j => j.GetAttachmentsFromIssue(TestableJira.Token, "key"))
                    .Returns(new RemoteAttachment[1] { new RemoteAttachment() { filename = "attach.txt" } });

                var issue = (new RemoteIssue() { key = "key" }).ToLocal(jira);

                //act
                var attachments = issue.GetAttachments();

                //assert
                Assert.Equal(1, attachments.Count);
                Assert.Equal("attach.txt", attachments[0].FileName);
            }
        }

        public class AddAttachment
        {
            [Fact]
            public void AddAttachment_IfIssueNotCreated_ShouldThrowAnException()
            {
                var issue = CreateIssue();

                Assert.Throws(typeof(InvalidOperationException), () => issue.AddAttachment("foo", new byte[] { 1 }));
            }

            [Fact]
            public void AddAttachment_IfIssueCreated_ShouldUpload()
            {
                //arrange
                var jira = TestableJira.Create();
                jira.FileSystem.Setup(f => f.FileReadAllBytes("foo.txt")).Returns(new byte[] { 1, 2, 3 });
                var issue = (new RemoteIssue() { key = "key" }).ToLocal(jira);

                //act
                issue.AddAttachment("foo.txt");

                //assert
                jira.SoapService.Verify(j => j.AddBase64EncodedAttachmentsToIssue(
                                                    "token",
                                                    "key",
                                                    new string[] { "foo.txt" },
                                                    new string[] { "AQID" }));
            }
        }

        public class AddLabels
        {
            [Fact]
            public void IfIssueNotCreated_ShouldThrowAnException()
            {
                var issue = CreateIssue();
                Assert.Throws(typeof(InvalidOperationException), () => issue.AddLabels());
            }
        }

        public class WorkflowTransition
        {
            [Fact]
            public void IfIssueNotCreated_ShouldThrowAnException()
            {
                var issue = CreateIssue();
                Assert.Throws(typeof(InvalidOperationException), () => issue.WorkflowTransition("foo"));
            }

            [Fact]
            public void IfTransitionNotFound_ShouldThrowAnException()
            {
                var jira = TestableJira.Create();
                var issue = (new RemoteIssue() { key = "key" }).ToLocal(jira);

                Assert.Throws(typeof(InvalidOperationException), () => issue.WorkflowTransition("foo"));
            }

            [Fact]
            public void CallsProgressWorkflowAction()
            {
                var jira = TestableJira.Create();
                var issue = (new RemoteIssue() { key = "key" }).ToLocal(jira);
                jira.SoapService.Setup(s => s.GetAvailableActions(It.IsAny<string>(), "key"))
                                .Returns(new RemoteNamedObject[1] { new RemoteNamedObject() { id = "123", name = "action" } });
                jira.SoapService.Setup(s => s.ProgressWorkflowAction(It.IsAny<string>(), "key", "123", It.IsAny<RemoteFieldValue[]>()))
                                .Returns(new RemoteIssue() { status = "456" });

                issue.WorkflowTransition("action");

                Assert.Equal("456", issue.Status.Id);
            }
        }

        public class GetComments
        {
            [Fact]
            public void IfIssueNotCreated_ShouldThrowException()
            {
                var issue = CreateIssue();

                Assert.Throws(typeof(InvalidOperationException), () => issue.GetComments());
            }

            [Fact]
            public void IfIssueIsCreated_ShouldLoadComments()
            {
                //arrange
                var jira = TestableJira.Create();
                jira.SoapService.Setup(j => j.GetCommentsFromIssue(TestableJira.Token, "key"))
                    .Returns(new RemoteComment[1] { new RemoteComment() { body = "the comment" } });
                var issue = (new RemoteIssue() { key = "key" }).ToLocal(jira);

                //act
                var comments = issue.GetComments();

                //assert
                Assert.Equal(1, comments.Count);
                Assert.Equal("the comment", comments[0].Body);
            }
        }

        public class AddComment
        {
            [Fact]
            public void IfIssueNotCreated_ShouldThrowAnException()
            {
                var issue = CreateIssue();

                Assert.Throws(typeof(InvalidOperationException), () => issue.AddComment("foo"));
            }

            [Fact]
            public void IfIssueCreated_ShouldUpload()
            {
                //arrange
                var jira = TestableJira.Create();
                var issue = (new RemoteIssue() { key = "key" }).ToLocal(jira);

                //act
                issue.AddComment("the comment");

                //assert
                jira.SoapService.Verify(j => j.AddComment(
                                                    "token",
                                                    "key",
                                                    It.Is<RemoteComment>(r => r.body == "the comment" && r.author == "user")));
            }
        }

        public class AddWorklog
        {
            [Fact]
            public void IfIssueNotCreated_ShouldThrowAnException()
            {
                var issue = CreateIssue();

                Assert.Throws(typeof(InvalidOperationException), () => issue.AddWorklog("foo"));
            }

            [Fact]
            public void WithWorklogObject_ShouldCallServiceToCreateLog()
            {
                var jira = TestableJira.Create();
                var work = new Worklog("1m", DateTime.Now) { Comment = "comment" };
                var issue = (new RemoteIssue() { key = "key" }).ToLocal(jira);

                var result = issue.AddWorklog(work);

                jira.SoapService.Verify(j => j.AddWorklogAndAutoAdjustRemainingEstimate(
                    It.IsAny<string>(),
                    "key",
                    It.Is<RemoteWorklog>(l => l.timeSpent == "1m" && l.comment == "comment")));
            }

            [Fact]
            public void ShouldCallServiceToAddWorkLog()
            {
                var jira = TestableJira.Create();
                var issue = (new RemoteIssue() { key = "key" }).ToLocal(jira);

                //act
                var result = issue.AddWorklog("1d");

                //assert
                jira.SoapService.Verify(j => j.AddWorklogAndAutoAdjustRemainingEstimate(
                    It.IsAny<string>(),
                    "key",
                    It.Is<RemoteWorklog>(l => l.timeSpent == "1d")));
            }

            [Fact]
            public void IfRetainRemainingEstimate_ShouldAddWorkLog()
            {
                var jira = TestableJira.Create();
                var remoteWorkLog = new RemoteWorklog() { id = "12345" };
                jira.SoapService.Setup(s => s.AddWorklogAndRetainRemainingEstimate(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RemoteWorklog>())).Returns(remoteWorkLog);
                var issue = (new RemoteIssue() { key = "key" }).ToLocal(jira);

                //act
                var result = issue.AddWorklog("1d", WorklogStrategy.RetainRemainingEstimate);

                //assert
                Assert.Equal("12345", result.Id);
                jira.SoapService.Verify(j => j.AddWorklogAndRetainRemainingEstimate(
                    "token",
                    "key",
                    It.Is<RemoteWorklog>(l => l.timeSpent == "1d")));
            }

            [Fact]
            public void IfNewRemainingEstimate_ShouldAddWorkLog()
            {
                var jira = TestableJira.Create();
                var remoteWorkLog = new RemoteWorklog() { id = "12345" };
                jira.SoapService.Setup(s => s.AddWorklogWithNewRemainingEstimate(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RemoteWorklog>(), It.IsAny<string>())).Returns(remoteWorkLog);
                var issue = (new RemoteIssue() { key = "key" }).ToLocal(jira);

                //act
                var result = issue.AddWorklog("1d", WorklogStrategy.NewRemainingEstimate, "5d");

                //assert
                Assert.Equal("12345", result.Id);
                jira.SoapService.Verify(j => j.AddWorklogWithNewRemainingEstimate(
                    "token",
                    "key",
                    It.Is<RemoteWorklog>(l => l.timeSpent == "1d"),
                    "5d"));
            }
        }

        public class GetWorklogs
        {
            [Fact]
            public void ShouldGetWorklogsFromServer()
            {
                var jira = TestableJira.Create();
                var logs = new RemoteWorklog[] { new RemoteWorklog() { id = "12345" } };
                jira.SoapService.Setup(s => s.GetWorkLogs(It.IsAny<string>(), "111")).Returns(logs);
                var issue = (new RemoteIssue() { key = "111" }).ToLocal(jira);

                var result = issue.GetWorklogs();

                Assert.Equal("12345", result.First().Id);
            }

            [Fact]
            public void IfIssueNotCreated_ShouldThrowException()
            {
                var issue = new Issue(TestableJira.Create(), "project");

                Assert.Throws(typeof(InvalidOperationException), () => issue.GetWorklogs());
            }
        }

        public class Refresh
        {
            [Fact]
            public void Refresh_IfIssueNotCreated_ShouldThrowAnException()
            {
                var issue = CreateIssue();

                Assert.Throws(typeof(InvalidOperationException), () => issue.Refresh());
            }
        }

        private static Issue CreateIssue(string project = "TST")
        {
            return TestableJira.Create().CreateIssue(project);
        }

        private static RemoteFieldValue[] GetUpdatedFieldsForIssue(Issue issue)
        {
            return ((IRemoteIssueFieldProvider)issue).GetRemoteFields();
        }
    }
}
