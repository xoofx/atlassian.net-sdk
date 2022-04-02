using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Atlassian.Jira.Remote;
using Moq;
using Xunit;

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
                Assert.Empty(issue.AffectsVersions);
                Assert.Null(issue.Assignee);
                Assert.Empty(issue.Components);
                Assert.Null(issue.Created);
                Assert.Empty(issue.CustomFields);
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
                    priority = new RemotePriority() { id = "priority" },
                    project = "project",
                    reporter = "reporter",
                    resolution = new RemoteResolution() { id = "resolution" },
                    status = new RemoteStatus() { id = "status" },
                    summary = "summary",
                    type = new RemoteIssueType() { id = "type" },
                    updated = new DateTime(2011, 2, 2),
                    votesData = new RemoteVotes() { votes = 1, hasVoted = true }
                };

                var issue = remoteIssue.ToLocal(TestableJira.Create());

                Assert.Single(issue.AffectsVersions);
                Assert.Equal("assignee", issue.Assignee);
                Assert.Single(issue.Components);
                Assert.Equal(new DateTime(2011, 1, 1), issue.Created);
                Assert.Single(issue.CustomFields);
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
                Assert.True(issue.HasUserVoted);
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
                Assert.Null(remoteIssue.votesData);
            }

            [Fact]
            public void IfFieldsSet_ShouldPopulateFields()
            {
                var jira = TestableJira.Create();
                var issue = jira.CreateIssue("ProjectKey");
                var version = new RemoteVersion() { id = "1" }.ToLocal(issue.Jira);
                var component = new RemoteComponent() { id = "1" }.ToLocal();

                jira.IssueTypeService.Setup(s => s.GetIssueTypesAsync(CancellationToken.None))
                    .Returns(Task.FromResult(Enumerable.Repeat(new IssueType("4", "issuetype"), 1)));
                jira.IssuePriorityService.Setup(s => s.GetPrioritiesAsync(CancellationToken.None))
                    .Returns(Task.FromResult(Enumerable.Repeat(new IssuePriority("1", "priority"), 1)));

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

                var remoteIssue = issue.ToRemote();

                Assert.Single(remoteIssue.affectsVersions);
                Assert.Equal("assignee", remoteIssue.assignee);
                Assert.Single(remoteIssue.components);
                Assert.Null(remoteIssue.created);
                Assert.Equal("description", remoteIssue.description);
                Assert.Equal(new DateTime(2011, 1, 1), remoteIssue.duedate);
                Assert.Equal("environment", remoteIssue.environment);
                Assert.Null(remoteIssue.key);
                Assert.Equal("1", remoteIssue.priority.id);
                Assert.Equal("ProjectKey", remoteIssue.project);
                Assert.Equal("reporter", remoteIssue.reporter);
                Assert.Null(remoteIssue.resolution);
                Assert.Null(remoteIssue.status);
                Assert.Equal("summary", remoteIssue.summary);
                Assert.Equal("4", remoteIssue.type.id);
                Assert.Null(remoteIssue.updated);
            }

            [Fact]
            public void ToRemote_IfTypeSetByName_FetchId()
            {
                var jira = TestableJira.Create();
                var issue = jira.CreateIssue("ProjectKey");
                var issueType = new IssueType(new RemoteIssueType() { id = "1", name = "Bug" });
                jira.IssueTypeService.Setup(s => s.GetIssueTypesAsync(CancellationToken.None))
                    .Returns(Task.FromResult(Enumerable.Repeat<IssueType>(issueType, 1)));

                issue.Type = "Bug";

                var remoteIssue = issue.ToRemote();
                Assert.Equal("1", remoteIssue.type.id);
            }
        }

        public class GetUpdatedFields
        {
            [Fact]
            public async Task ReturnsCustomFieldsAdded()
            {
                var jira = TestableJira.Create();
                var customField = new CustomField(new RemoteField() { id = "CustomField1", name = "My Custom Field" });
                var remoteIssue = new RemoteIssue()
                {
                    key = "TST-1",
                    project = "TST",
                    type = new RemoteIssueType() { id = "1" }
                };

                jira.IssueService.SetupIssues(jira, remoteIssue);
                jira.IssueFieldService.Setup(c => c.GetCustomFieldsAsync(CancellationToken.None))
                    .Returns(Task.FromResult(Enumerable.Repeat<CustomField>(customField, 1)));

                var issue = jira.CreateIssue("TST");
                issue["My Custom Field"] = "test value";

                var result = await GetUpdatedFieldsForIssueAsync(issue);
                Assert.Single(result);
                Assert.Equal("CustomField1", result.First().id);
            }

            [Fact]
            public async Task ExcludesCustomFieldsNotModified()
            {
                var jira = TestableJira.Create();
                var customField = new CustomField(new RemoteField() { id = "CustomField1", name = "My Custom Field" });
                var remoteCustomFieldValue = new RemoteCustomFieldValue()
                {
                    customfieldId = "CustomField1",
                    values = new string[1] { "My Value" }
                };
                var remoteIssue = new RemoteIssue()
                {
                    key = "TST-1",
                    project = "TST",
                    type = new RemoteIssueType() { id = "1" },
                    customFieldValues = new RemoteCustomFieldValue[1] { remoteCustomFieldValue }
                };

                jira.IssueService.Setup(s => s.GetIssueAsync("TST-1", CancellationToken.None))
                    .Returns(Task.FromResult(new Issue(jira, remoteIssue)));
                jira.IssueFieldService.Setup(c => c.GetCustomFieldsAsync(CancellationToken.None))
                    .Returns(Task.FromResult(Enumerable.Repeat<CustomField>(customField, 1)));
                jira.IssueTypeService.Setup(s => s.GetIssueTypesAsync(CancellationToken.None))
                    .Returns(Task.FromResult(Enumerable.Repeat(new IssueType("1"), 1)));

                var issue = jira.Issues.GetIssueAsync("TST-1").Result;

                var result = await GetUpdatedFieldsForIssueAsync(issue);
                Assert.Empty(result);
            }

            [Fact]
            public async Task ReturnsCustomFieldThatWasModified()
            {
                var jira = TestableJira.Create();
                var customField = new CustomField(new RemoteField() { id = "CustomField1", name = "My Custom Field" });
                var remoteCustomFieldValue = new RemoteCustomFieldValue()
                {
                    customfieldId = "CustomField1",
                    values = new string[1] { "My Value" }
                };
                var remoteIssue = new RemoteIssue()
                {
                    key = "TST-1",
                    project = "TST",
                    type = new RemoteIssueType() { id = "1" },
                    customFieldValues = new RemoteCustomFieldValue[1] { remoteCustomFieldValue }
                };

                jira.IssueService.Setup(s => s.GetIssueAsync("TST-1", CancellationToken.None))
                    .Returns(Task.FromResult(new Issue(jira, remoteIssue)));
                jira.IssueFieldService.Setup(c => c.GetCustomFieldsAsync(CancellationToken.None))
                    .Returns(Task.FromResult(Enumerable.Repeat<CustomField>(customField, 1)));
                jira.IssueTypeService.Setup(s => s.GetIssueTypesAsync(CancellationToken.None))
                    .Returns(Task.FromResult(Enumerable.Repeat(new IssueType("1"), 1)));

                var issue = jira.Issues.GetIssueAsync("TST-1").Result;
                issue["My Custom Field"] = "My New Value";

                var result = await GetUpdatedFieldsForIssueAsync(issue);
                Assert.Single(result);
                Assert.Equal("CustomField1", result.First().id);
                Assert.Equal("My New Value", result.First().values[0]);
            }

            [Fact]
            public async Task IfIssueTypeWithId_ReturnField()
            {
                var jira = TestableJira.Create();
                var issue = jira.CreateIssue("TST");
                issue.Priority = "5";

                jira.IssuePriorityService.Setup(s => s.GetPrioritiesAsync(CancellationToken.None))
                    .Returns(Task.FromResult(Enumerable.Repeat(new IssuePriority("5"), 1)));

                var result = await GetUpdatedFieldsForIssueAsync(issue);
                Assert.Single(result);
                Assert.Equal("5", result[0].values[0]);
            }

            [Fact]
            public async Task IfIssueTypeWithName_ReturnsFieldWithIdInferred()
            {
                var jira = TestableJira.Create();
                var issueType = new IssueType(new RemoteIssueType() { id = "2", name = "Task" });
                jira.IssueTypeService.Setup(s => s.GetIssueTypesAsync(CancellationToken.None))
                    .Returns(Task.FromResult(Enumerable.Repeat<IssueType>(issueType, 1)));
                var issue = jira.CreateIssue("FOO");
                issue.Type = "Task";

                var result = await GetUpdatedFieldsForIssueAsync(issue);
                Assert.Single(result);
                Assert.Equal("2", result[0].values[0]);
            }

            [Fact]
            public async Task IfIssueTypeWithNameNotChanged_ReturnsNoFieldsChanged()
            {
                var jira = TestableJira.Create();
                var issueType = new IssueType(new RemoteIssueType() { id = "5", name = "Task" });
                jira.IssueTypeService.Setup(s => s.GetIssueTypesAsync(CancellationToken.None))
                    .Returns(Task.FromResult(Enumerable.Repeat(issueType, 1)));
                var remoteIssue = new RemoteIssue()
                {
                    type = new RemoteIssueType() { id = "5" },
                };

                var issue = remoteIssue.ToLocal(jira);
                issue.Type = "Task";

                var fields = await GetUpdatedFieldsForIssueAsync(issue);
                Assert.Empty(fields);
            }

            [Fact]
            public async Task ReturnEmptyIfNothingChanged()
            {
                var issue = CreateIssue();

                Assert.Empty((await GetUpdatedFieldsForIssueAsync(issue)));
            }

            [Fact]
            public async Task IfString_ReturnOneFieldThatChanged()
            {
                var issue = CreateIssue();
                issue.Summary = "foo";

                Assert.Single((await GetUpdatedFieldsForIssueAsync(issue)));
            }

            [Fact]
            public async Task IfString_ReturnAllFieldsThatChanged()
            {
                var jira = TestableJira.Create();
                var issue = jira.CreateIssue("TST");
                issue.Summary = "foo";
                issue.Description = "foo";
                issue.Assignee = "foo";
                issue.Environment = "foo";
                issue.Reporter = "foo";
                issue.Type = "2";
                issue.Resolution = "3";
                issue.Priority = "4";

                jira.IssuePriorityService.Setup(s => s.GetPrioritiesAsync(CancellationToken.None))
                    .Returns(Task.FromResult(Enumerable.Repeat(new IssuePriority("4"), 1)));
                jira.IssueResolutionService.Setup(s => s.GetResolutionsAsync(CancellationToken.None))
                    .Returns(Task.FromResult(Enumerable.Repeat(new IssueResolution("3"), 1)));
                jira.IssueTypeService.Setup(s => s.GetIssueTypesAsync(CancellationToken.None))
                    .Returns(Task.FromResult(Enumerable.Repeat(new IssueType("2"), 1)));

                Assert.Equal(8, (await GetUpdatedFieldsForIssueAsync(issue)).Length);
            }

            [Fact]
            public async Task IfStringEqual_ReturnNoFieldsThatChanged()
            {
                var remoteIssue = new RemoteIssue()
                {
                    summary = "Summary"
                };

                var issue = remoteIssue.ToLocal(TestableJira.Create());

                issue.Summary = "Summary";

                Assert.Empty(await GetUpdatedFieldsForIssueAsync(issue));
            }

            [Fact]
            public async Task IfComparableEqual_ReturnNoFieldsThatChanged()
            {
                var jira = TestableJira.Create();
                var remoteIssue = new RemoteIssue()
                {
                    priority = new RemotePriority() { id = "5" },
                };

                var issue = remoteIssue.ToLocal(jira);
                issue.Priority = "5";

                jira.IssuePriorityService.Setup(s => s.GetPrioritiesAsync(CancellationToken.None))
                    .Returns(Task.FromResult(Enumerable.Repeat(new IssuePriority("5"), 1)));
                Assert.Empty(await GetUpdatedFieldsForIssueAsync(issue));
            }

            [Fact]
            public async Task IfComparable_ReturnsFieldsThatChanged()
            {
                var jira = TestableJira.Create();
                var issue = jira.CreateIssue("TST");
                issue.Priority = "5";

                jira.IssuePriorityService.Setup(s => s.GetPrioritiesAsync(CancellationToken.None))
                    .Returns(Task.FromResult(Enumerable.Repeat(new IssuePriority("5"), 1)));

                Assert.Single(await GetUpdatedFieldsForIssueAsync(issue));
            }

            [Fact]
            public async Task IfDateTimeChanged_ReturnsFieldsThatChanged()
            {
                var issue = CreateIssue();
                issue.DueDate = new DateTime(2011, 10, 10);

                var fields = await GetUpdatedFieldsForIssueAsync(issue);
                Assert.Single(fields);
                Assert.Equal("10/Oct/11", fields[0].values[0]);
            }

            [Fact]
            public async Task IfDateTimeUnChangd_ShouldNotIncludeItInFieldsThatChanged()
            {
                var remoteIssue = new RemoteIssue()
                {
                    duedate = new DateTime(2011, 1, 1)
                };

                var issue = remoteIssue.ToLocal(TestableJira.Create());
                Assert.Empty(await GetUpdatedFieldsForIssueAsync(issue));
            }

            [Fact]
            public async Task IfComponentsAdded_ReturnsFields()
            {
                var issue = new RemoteIssue() { key = "foo" }.ToLocal(TestableJira.Create());
                var component = new RemoteComponent() { id = "1", name = "1.0" };
                issue.Components.Add(component.ToLocal());

                var fields = await GetUpdatedFieldsForIssueAsync(issue);
                Assert.Single(fields);
                Assert.Equal("components", fields[0].id);
                Assert.Equal("1", fields[0].values[0]);
            }

            [Fact]
            public async Task IfAddFixVersion_ReturnAllFieldsThatChanged()
            {
                var issue = new RemoteIssue() { key = "foo" }.ToLocal(TestableJira.Create());
                var version = new RemoteVersion() { id = "1", name = "1.0" };
                issue.FixVersions.Add(version.ToLocal(TestableJira.Create()));

                var fields = await GetUpdatedFieldsForIssueAsync(issue);
                Assert.Single(fields);
                Assert.Equal("fixVersions", fields[0].id);
                Assert.Equal("1", fields[0].values[0]);
            }

            [Fact]
            public async Task IfAddAffectsVersion_ReturnAllFieldsThatChanged()
            {
                var issue = new RemoteIssue() { key = "foo" }.ToLocal(TestableJira.Create());
                var version = new RemoteVersion() { id = "1", name = "1.0" };
                issue.AffectsVersions.Add(version.ToLocal(TestableJira.Create()));

                var fields = await GetUpdatedFieldsForIssueAsync(issue);
                Assert.Single(fields);
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

                Assert.Throws<InvalidOperationException>(() => issue.GetAttachmentsAsync().Result);
            }

            [Fact]
            public void IfIssueIsCreated_ShouldLoadAttachments()
            {
                //arrange
                var jira = TestableJira.Create();
                var remoteAttachment = new RemoteAttachment() { filename = "attach.txt" };
                jira.IssueService.Setup(j => j.GetAttachmentsAsync("issueKey", It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(Enumerable.Repeat<Attachment>(new Attachment(jira, remoteAttachment), 1)));

                var issue = (new RemoteIssue() { key = "issueKey" }).ToLocal(jira);

                //act
                var attachments = issue.GetAttachmentsAsync().Result;

                //assert
                Assert.Single(attachments);
                Assert.Equal("attach.txt", attachments.First().FileName);
            }
        }

        public class AddAttachment
        {
            [Fact]
            public void AddAttachment_IfIssueNotCreated_ShouldThrowAnException()
            {
                var issue = CreateIssue();

                Assert.Throws<InvalidOperationException>(() => issue.AddAttachment("foo", new byte[] { 1 }));
            }
        }

        public class WorkflowTransition
        {
            [Fact]
            public void IfTransitionNotFound_ShouldThrowAnException()
            {
                var jira = TestableJira.Create();
                var issue = (new RemoteIssue() { key = "key" }).ToLocal(jira);

                Assert.Throws<AggregateException>(() => issue.WorkflowTransitionAsync("foo").Wait());
            }
        }

        public class GetComments
        {
            [Fact]
            public void IfIssueNotCreated_ShouldThrowException()
            {
                var issue = CreateIssue();

                Assert.Throws<InvalidOperationException>(() => issue.GetCommentsAsync().Result);
            }

            [Fact]
            public void IfIssueIsCreated_ShouldLoadComments()
            {
                //arrange
                var jira = TestableJira.Create();
                jira.IssueService.Setup(j => j.GetCommentsAsync("issueKey", It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(Enumerable.Repeat<Comment>(new Comment() { Body = "the comment" }, 1)));
                var issue = (new RemoteIssue() { key = "issueKey" }).ToLocal(jira);

                //act
                var comments = issue.GetCommentsAsync().Result;

                //assert
                Assert.Single(comments);
                Assert.Equal("the comment", comments.First().Body);
            }
        }

        private static Issue CreateIssue(string project = "TST")
        {
            return TestableJira.Create().CreateIssue(project);
        }

        private static Task<RemoteFieldValue[]> GetUpdatedFieldsForIssueAsync(Issue issue)
        {
            return ((IRemoteIssueFieldProvider)issue).GetRemoteFieldValuesAsync(CancellationToken.None);
        }
    }
}
