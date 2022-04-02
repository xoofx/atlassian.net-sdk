# How to use the SDK

## Query Issues

A LinqToJIRA provider allows to query a JIRA server using Linq syntax:

```csharp
// create a connection to JIRA using the Rest client
var jira = Jira.CreateRestClient("http://<your_jira_server>", "<user>", "<password>");

// use LINQ syntax to retrieve issues
var issues = from i in jira.Issues.Queryable
             where i.Assignee == "admin" && i.Priority == "Major"
             orderby i.Created
             select i;
```
Some Jira server URLs need HTTPS for authentication to work.

By default, string comparisons are translated using the JIRA contains operator ('~'). A literal match can be forced by
wrapping the string with the LiteralMatch class:

```csharp
var issues = from i in jira.Issues.Queryable
             where i.Summary == new LiteralMatch("My Title")
             select i;
```

See the [JQL Page](https://bitbucket.org/farmas/atlassian.net-sdk/wiki/JQL) for a list of supported fields and
operators.

## Create Issue

```csharp
var issue = jira.CreateIssue("My Project");
issue.Type = "Bug";
issue.Priority = "Major";
issue.Summary = "Issue Summary";

await issue.SaveChangesAsync();
```

## Update Issue

```csharp
var issue = await jira.Issues.GetIssueAsync("TST-5");
issue.Summary = "Updated Summary";

await issue.SaveChangesAsync();
```

## Auto fetch field values

```csharp
var issue = await jira.Issues.GetIssueAsync("TST-5");

Console.WriteLine(issue.Priority.Name);      // returns the string of the priority field, for example "Critical"
Console.WriteLine(issue.Type.Name);          // returns the string of the issue type field, for example "Bug"
Console.WriteLine(issue["My CustomField"]);  // returns the string of the custom field named "My CustomField"
```

## Custom Fields

```csharp
var issue = (from i in jira.Issues.Queryable
             where i["My CustomField"] == "Custom Field Value"
             select i).First();

issue["My CustomField"] = "Updated Field";  // No need to know the id of the custom field.
issue.CustomFields.AddArray("Custom Labels Field", "label1", "label2"); // Adds an array value to a custom field.
issue.CustomFields.AddCascadingSelectField("Custom Cascading Select Field", "Option3"); // Adds a value to a cascading select field.

await issue.SaveChangesAsync();

var cascadingSelect = issue.CustomFields.GetCascadingSelectField("Custom Cascading Select Field"); // Gets the value of a cascading field.
```

For more information and advanced scenarios see the [Custom Fields](https://bitbucket.org/farmas/atlassian.net-sdk/wiki/Custom%20Fields)
page.

## Attachments

```csharp
var issue = await jira.Issues.GetIssueAsync("TST-5");

// get attachments
var attachments = await issue.GetAttachmentsAsync();
Console.WriteLine(attachments.First().FileName);

// download an attachment
var tempFile = Path.GetTempFileName();
attachments[0].Download(tempFile);

// upload an attachment
await issue.AddAttachmentsAsync("fileToAdd.txt");
```

## Comments

```csharp
var issue = await jira.Issues.GetIssueAsync("TST-5");

// get comments
var comments = await issue.GetCommentsAsync();
Console.WriteLine(comments.First().Body);

// add comment
await issue.AddCommentAsync("new comment");
```

## Worklogs

```csharp
var issue = await jira.Issues.GetIssueAsync("TST-5");

// add a worklog
await issue.AddWorklogAsync("1h");

// add worklog with new remaining estimate
await issue.AddWorklogAsync("1m", WorklogStrategy.NewRemainingEstimate, "4h");

// retrieve worklogs
var worklogs = await issue.GetWorklogsAsync();
```

## Create Sub-Task

```csharp
var issue = jira.CreateIssue("My Project", "PARENTISSUE-1");
issue.Type = "5"; // the id of the sub-task issue type
issue.Summary = "A sub task";

await issue.SaveChangesAsync();
```

## Workflow transitions
Workflowtransitions can be used to update the status of an issue. 

You can either use one of the pre-set functions such as:
```csharp
var issue = await jira.Issues.GetIssueAsync("TST-5");
issue.Resolution = "Won't Fix";

await issue.WorkflowTransitionAsync(WorkflowActions.Resolve);
```

Or you can pretty much have a lookout for the button which executes the function which you are looking for:
```csharp
// Sets the ticket status to Pending
issue.WorkflowTransitionAsync("Pending");

// Removes status Pending
issue.WorkflowTransitionAsync("Back to open");

```

