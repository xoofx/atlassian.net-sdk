# How to configure what fields to download when querying issues

By default, when an issue is downloaded from the server the library will fetch all fields to populate all its public properties. For situations when a lot of issues are being downloaded a client may configure what fields to get as part of the query.

## Get only one basic field

In this case, the client is interested in only retrieving a sub-set of the fields that populate the public properties of the Issue class. In the sample below only `issue.Summary` will be available, all other properties will be null or empty.

```csharp
var options = new IssueSearchOptions("key = TST-1")
{
    FetchBasicFields = false,
    AdditionalFields = new List<string>() { "summary" }
};

var issues = await jira.Issues.GetIssuesFromJqlAsync(options);
```

## Get all basic fields plus one additional property

In this case, the client wants all the public properties of the Issue class to be populated, but is also interested in pre-fetching all the attachments of each issue. Note that the attachments of each fields are available in the `Issue.AdditionalFields` property.

```csharp
var options = new IssueSearchOptions("key = TST-1")
{
    FetchBasicFields = true,
    AdditionalFields = new List<string>() { "attachment" }
};

var issues = await jira.Issues.GetIssuesFromJqlAsync(options);
var attachemnts = issues.First().AdditionalFields.Attachments;
```

## Get only two non-basic fields

In this case, the client is not interested in any of the public properties of the Issue class, but wants only the comments and the worklogs of each issue.

```csharp
var options = new IssueSearchOptions($"key = {issue.Key.Value}")
{
    FetchBasicFields = false,
    AdditionalFields = new List<string>() { "comment", "worklog" }
};

var issues = await jira.Issues.GetIssuesFromJqlAsync(options);
var worklogs = issues.First().AdditionalFields.Worklogs;
var comments = issues.First().AdditionalFields.Comments;
```

## Get a non-typed field

In cases where a class doesn't exist to represent the field value, a client can get a JToken directly.

```csharp
var options = new IssueSearchOptions($"key = {issue.Key.Value}")
{
    FetchBasicFields = false,
    AdditionalFields = new List<string>() { "some-field" }
};

var issues = await jira.Issues.GetIssuesFromJqlAsync(options);
issues.First().AdditionalFields.TryGetValue("some-field", out JToken value);
```

