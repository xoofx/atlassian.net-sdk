# How to configure library for Jira user privacy changes (GDPR)

Atlassian announced changes to [Jira Cloud REST APIs to improve user privacy](https://developer.atlassian.com/cloud/jira/platform/deprecation-notice-user-privacy-api-migration-guide/) that is incompatible with the way this library handles responses from Jira.

Jira Cloud users that are affected by this change need to configure the library to enable user privacy mode:

```
var settings = new JiraRestClientSettings();
settings.EnableUserPrivacyMode = true;

var jira = Jira.CreateRestClient("jira-url", "username", "api-token", settings);
```

## Changes Included

Custom serializers are registered that read and write the 'accountId' of users instead of the default 'username'.

These fields affected are:

- Issue.Assignee
- Issue.Reporter
- Comment.Author
- Comment.UpdateAuthor
- Worklog.Author
- Project.Lead
- Attachment.Author

Additionally, two custom field types now read and write the 'accountId' of users instead of the default 'username'. The custom field types affected are:

- 'User Picker' custom field type
- 'Multi User Picker'  custom field type

Finally, all requests that include the 'username' as part of querystring or body, now use 'accountId'. API's affected are:

- IIssueService.DeleteWatcherAsync()
- IIssueService.AddWatcherAsync()
- IJiraGroupService.AddUserAsync()
- IJiraGroupService.RemoveUserAsync()
- IJiraUserService.GetUserAsync()
- IJiraUserService.DeleteUserAsync()
- IJiraUserService.SearchUsersAsync()
- Issue.AddCommentAsync()
- Issue.AddWatcherAsync()
- Issue.DeleteWatcherAsync()
- Issue.AssignAsync()

## Examples

Read and write account id's to user picker custom fields:

```
issue["Test User Picker"] = myAccountId;
issue.CustomFields.AddArray("Test Multi User Picker", myAccountId, testAccountId);

issue = await issue.SaveChangesAsync();

var singleAccountId = issue["Test User Picker"]);
var multiAccounIds = issue.CustomFields["Test Multi User Picker"].Values;
```