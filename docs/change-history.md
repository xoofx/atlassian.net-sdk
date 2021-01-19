# Change History

## Version 12.4.0 (01/18/2021)

- [Issue 480](https://bitbucket.org/farmas/atlassian.net-sdk/issues/480) Adds ability to get an issue filter by id.

## Version 12.3.0 (12/05/2020)

- [Issue 562](https://bitbucket.org/farmas/atlassian.net-sdk/issues/562) Adds a new sprint serializer that can handle the new sprint format.
- [Issue 570](https://bitbucket.org/farmas/atlassian.net-sdk/issues/570) Fixes exception in JiraUser when using comparisons.

To register the new sprint serialiser:
```
var settings = new JiraRestClientSettings();
settings.CustomFieldSerializers["com.pyxis.greenhopper.jira:gh-sprint"] = new new GreenhopperSprintJsonCustomFieldValueSerialiser();
var jira = new Jira("url", "user", "password", settings);
```

## Version 12.2.0 (10/25/2020)

- [Issue 253](https://bitbucket.org/farmas/atlassian.net-sdk/issues/253) Exposes the avatar url's for projects and users.

## Version 12.1.0 (03/08/2020)

- [Issue #509](https://bitbucket.org/farmas/atlassian.net-sdk/issues/503): Allows users to handle changes due to user privacy updates (GDPR).
- Exposes JiraUser object for Issue.Reporter, Issue.Assignee, Comment.Author, Attachment.Author, Worklog.Author and Project.Lead.
- Allows users to deserialize custom fields to .NET types. Makes it possible to read JiraUser objects from 'user-picker' custom fields.
 
## Version 12.0.0 (02/15/2020)

- PR #80: Allows user to specify which issue links to get.
- [Issue #503](https://bitbucket.org/farmas/atlassian.net-sdk/issues/503): Skip and Take should reset with each query execution
- [Issue #499](https://bitbucket.org/farmas/atlassian.net-sdk/issues/499): Updated RestSharp to 106.10.1 (from 106.2.2)

__Breaking Changes__: RestSharp library has breaking changes between version 106.2.2 and 106.10.1

## Version 11.2.0 (01/06/2020) ##
- [PR #77](https://bitbucket.org/farmas/atlassian.net-sdk/pull-requests/77): Adds option to expand transition fields.
- [PR #78](https://bitbucket.org/farmas/atlassian.net-sdk/pull-requests/78): Adds Screen service.
- [PR #79](https://bitbucket.org/farmas/atlassian.net-sdk/pull-requests/79): Adds ServerInfo service.
- [Issue #477](https://bitbucket.org/farmas/atlassian.net-sdk/issues/477): Allow integration tests to run against Jira 8.x

## Version 11.1.0 (12/21/2019) ##
* [PR #75](https://bitbucket.org/farmas/atlassian.net-sdk/pull-requests/75): Adds ability to retrieve an issue status by its id.
* [PR #76](https://bitbucket.org/farmas/atlassian.net-sdk/pull-requests/76): Adds ability to retrieve data for issue status category.
* [Issue #494](https://bitbucket.org/farmas/atlassian.net-sdk/issues/494): Allows workflow transitions to be executed by using the action id.
* [Issue #481](https://bitbucket.org/farmas/atlassian.net-sdk/issues/481): Removes exception when library fails to discover the custom field type.

## Version 11.0.0 (11/08/2019) ##
* Jira.CreateOAuthRestClient() has been added to be able to interact with Jira using OAuth tokens instead of user/password.
* JiraOAuthTokenHelper class has been added to help consumers generate request/access tokens to use with OAuth.
* Special thanks to [Romain Failliot
](https://www.linkedin.com/in/romainfailliot) and Ubisoft for contributing the OAuth feature.

__Breaking Changes__:
* JiraCredentials type has been removed.
* IWebClient interface has been removed in favor of standardizing the use of RestSharp.
* Attachment.DownloadAsync() and Attachment.DownloadDataAsync() have been removed.
* Project.GetComponetsAsync() was renamed to Project.GetComponentsAsync().
* Atlassian.SDK.Signed package is no longer maintained and will no longer be updated.

## Version 10.8.0 (10/23/2019) ##
* Adds ability to set, delete and retrieve entity properties on jira issues.


## Version 10.7.0 (09/30/2019) ##
* Adds ability to search for custom fields by issue type.

## Version 10.6.0 (05/11/2019) ##
* Changes AdditionalFields.Comments to return a PagedQueryResult.
* Changes AdditionalFields.Worklogs to return a PagedQueryResult.
* Fixes bug where cancellation token was not being passed to GetPagedCommentsAsync rest call.

## Version 10.5.0 (03/28/2019) ##
* Adds ability to retrieve the RenderedBody of a comment.
* Searches for IssueTypes by the project in which the issue is created.
* Adds visibility field to comment.
* Adds ability to search for custom fields by project key.
* Adds an overload to GetCommentsAsync to allow user to retrieve comments with options.

## Version 10.4.0 (10/08/2018) ##
* Adds ability to update the comment of an issue.
* Adds ability to 'assign' an issue to a user without updating property fields.
* Adds additional metadata to the issue transition object returned by Issue.GetAvailableActionsAsync() method.
* JQL will now include HH:mm for DateTime expressions when TimeOfDay > TimeSpan.Zero.

## Version 10.3.0 (07/03/2018) ##
* Adds ability to define the issue fields to include in the response when querying issues.

## Version 10.2.0 (03/08/2018) ##
* Exposes the Issue.TimeTrackingData for issue upon retrieval.
* Allows user to create an issue with original estimate and other time tracking data.

## Version 10.1.0 (03/06/2018) ##
* Adds ability to download attachment as a byte array.
* Adds support for configuring a Proxy when creating the rest client.

## Version 10.0.0 (11/30/2017) ##
* Adds support for .NET Core 2.0. Special thanks to [Mikael Rudberg](https://bitbucket.org/mikael_rudberg/) for getting this to work.

## Version 9.8.0 (11/12/2017) ##
* Return new comment from Issue.AddCommentAsync.
* Adds ability to read/write custom fields with string arrays values without the need to register a custom serializer.

## Version 9.7.0 (09/13/2017) ##
* Adds the validateQuery setting to use when querying issues.
* Changes GetIssuesAsync to disable query validation.
* Marks Jira.MaxIssuesPerRequest as obsolete.

## Version 9.6.0 (08/16/2017) ##
* Adds support for suppressing email notification on issue update.
* Adds locale to User object to retrieve language information.

## Version 9.5.0 (07/25/2017) ##
* Adds support for retrieving the properties of each comment.

## Version 9.4.0 (07/12/2017) ##
* Adds support for Issue.HasUserVoted property.

## Version 9.3.0 (07/01/2017) ##
* Adds support for adding and retrieving remote issue links.
* Adds support for deleting comments from issues.

## Version 9.2.0 (12/15/2016) ##
* Adds Project.GetIssueTypesAsync to retrieve only the issue types for a specific project.

## Version 9.1.0 (12/01/2016) ##
* Adds support for the Skip expression in JqlQueryProvider.

## Version 9.0.0 (11/12/2016) ##
* __[Breaking Change]__ Removes all members previously marked as obsolete.
* __[Breaking Change]__ Fixes typo on IIssueService.GetIssuesFromJqlAsync.
* __[Breaking Change]__ Fixes typo on IJiraUserService.CreateUserAsync.
* Adds ability to save the "labels" field with Issue.SaveChanges().

## Version 8.8.0 (11/10/2016) ##
* Adds support for ProjectVersion.StartDate.

## Version 8.7.0 (09/27/2016) ##
* Adds support for loading a single project from JIRA.

## Version 8.6.0 (09/16/2016) ##
* Adds IconUrl to Priorities, Status and Type of issue.
* Adds Url to the Project class.

## Version 8.5.0 (09/03/2016) ##
* Add ability to remove attachments from an issue.

## Version 8.4.0 (08/26/2016) ##
* Add ability to trace the response for each request.

## Version 8.3.0 (07/22/2016) ##
* Add ability to retrieve the issue's security level.
* Add support for the Sprint custom field.

## Version 8.0.0 (06/29/2016) ##
* __[Breaking Change]__ Updates target framework to 4.5.2.
* __[Breaking Change]__ SOAP client has been removed.
* __[Breaking Change]__ Jira.WithToken and Jira.GetAccessToken have been removed.
* __[Breaking Change]__ Issue Queryable is now available at Jira.Issues.Queryable (instead of Jira.Issues).
* Jira object now exposes top level interfaces to interact with server resources.
* All methods now support asynchronous executing and return Tasks.
* All non-async methods are marked as obsolete.
* Add support for CRUD operations for Users and Groups.

## Version 7.1.0 (06/05/2016) ##
* Add support for creating new components in a project.

## Version 7.0.0 (05/30/2016) ##
* __[Breaking Change]__ Fixed typo of Issue.GetSubTasks method.

## Version 6.4.0 (05/17/2016) ##
* Upgrade to use RestSharp v105.2.3
* Allow user to configure the internal RestSharp.RestClient used by the JiraRestClient.

## Version 6.3.0 (04/05/2016) ##
* Add ability to retrieve user details from JIRA.

## Version 6.2.0 (03/14/2016) ##
* [Pull Request] Add access to edit-metadata information of an issue.

## Version 6.1.0 (03/07/2016) ##
* Add ability to add and remove versions to a project. Fixes https://bitbucket.org/farmas/atlassian.net-sdk/issues/102.
* Add ability to update a project version.

## Version 6.0.0 (03/03/2016) ##
* __[Breaking Change]__ Deprecated all constructors in favor of Jira.CreateRestClient.
* __[Breaking Change]__ GetIssuesFromJql now returns IPagedQueryResult<Issue> instead of IEnumerable<Issue> that contains paging metadata.
* __[Breaking Change]__ Rest client now throws different exception types depending on errors returned from Jira. Improves error handling when response is not JSON or when JIRA returns an error object. Fixes: https://bitbucket.org/farmas/atlassian.net-sdk/issues/161 and https://bitbucket.org/farmas/atlassian.net-sdk/issues/153.
* Marked SOAP constructor as obsolete, will be removed on the next major version release.
* Add ability to remove components and versions from issues. Fixes https://bitbucket.org/farmas/atlassian.net-sdk/issues/101 and https://bitbucket.org/farmas/atlassian.net-sdk/issues/163.
* Add ability to retrieve the change logs of an issue. Fixes https://bitbucket.org/farmas/atlassian.net-sdk/issues/172.


## Version 5.13.0 (02/22/2016) ##
* Add ability to get, add and remove watchers from an issue. Fixes https://bitbucket.org/farmas/atlassian.net-sdk/issues/168.

## Version 5.12.0 (02/21/2016) ##
* Add ability to retrieve the labels of an issue. Fixes https://bitbucket.org/farmas/atlassian.net-sdk/issues/158.

## Version 5.11.0 (01/06/2016) ##
* GetIssueTypes: Only return issue types for the given project when a projectKey is provided. Fixes: https://bitbucket.org/farmas/atlassian.net-sdk/issues/155.
* Lock the version of RestSharp to 105.1.0, since breaking changes where introduced on v105.2.0.

## Version 5.10.0 (01/05/2016) ##
* Add ability to create and retrieve issue links. Fixes https://bitbucket.org/farmas/atlassian.net-sdk/issues/77.

## Version 5.9.0 (12/14/2015) ##
* Add ability to download attachments asynchronously.

## Version 5.8.0 (12/10/2015) ##
* Add ability to retrieve projects asynchronously.

## Version 5.7.0 (12/09/2015) ##
* Add ability to create issues asynchronously.

## Version 5.6.0 (12/06/2015) ##
* Add support for Issue.GetAvailableActionsAsync() and Issue.WorkflowTransitionAsync().

## Version 5.5.0 (11/30/2015) ##
* Add ability to get sub tasks of an issue. Fixes https://bitbucket.org/farmas/atlassian.net-sdk/issues/146.
* Add ability to add and retrieve comments asynchronously.

## Version 5.4.0 (11/26/2015) ##
* Allow user to add and read CascadingSelect custom fields. Fixes https://bitbucket.org/farmas/atlassian.net-sdk/issues/144.

## Version 5.3.0 (11/25/2015) ##
* Add GetIssueAsync and UpdateIssueAsync to the IJiraRestClient.

## Version 5.2.0 (11/15/2015) ##
* Experimental Feature: Return an IPagedQueryResult<Issue> from GetIssueFromJqlAsync.

## Version 5.1.0 (11/13/2015) ##
* Expose "Async" methods on the Jira class.
* Expose the ParentIssueKey for issues that are subtasks. Fixes https://bitbucket.org/farmas/atlassian.net-sdk/issues/142.
* Expose the ResolutionDate for issues that are subtasks. Fixes https://bitbucket.org/farmas/atlassian.net-sdk/issues/140.

## Version 5.0.0 (11/05/2015) ##
* __[Breaking Change]__ All "Async" methods have been removed from the Jira class, they are now available in the  IJiraRestClient interface which is accessible by the Jira.Rest property.
* Consumers now have access to the internal cache mantained by the library and can now create a Jira client with a prepopulated cache.
* All constructors of Jira class (which create SOAP clients) have been marked as deprecated in favor of Jira.CreateSoapClient and Jira.CreateRestClient factory methods. __Note__: These constructors will be removed on the next major version.

## Version 4.6.0 (10/29/2015) ##
* Allow creating an anonymous REST client.
* Delay request for customfield descriptors until first used instead of at the moment of creation of the JIRA client.
* Add async versions of some operations: GetFavouriteFiltersAsync, GetIssuesFromJqlAsync, GetIssuePrioritiesAsync, GetIssueResolutionAsync, GetIssueStatusesAsync, GetIssueTypesAsync.
* Always specifying "asc" and "desc" when generating order by JQL statements. Fixes https://bitbucket.org/farmas/atlassian.net-sdk/issues/131.
* __[Breaking Change]__ The default sort order for all fields in Linq-to-JQL is now 'asc'. User can still opt-in for descending order.

## Version 4.5.2 (09/14/2015) ##
* Allow multiple uses of where clauses in separate queries.
 
## Version 4.5.1 (09/08/2015) ##
* Fixes https://bitbucket.org/farmas/atlassian.net-sdk/issues/128.

## Version 4.5.0 (09/08/2015) ##
* Add Async methods to the rest client. Fixes https://bitbucket.org/farmas/atlassian.net-sdk/issues/127.
* Add rest overload that accepts the request body as a string. Fixes: https://bitbucket.org/farmas/atlassian.net-sdk/issues/123.

## Version 4.4.0 (08/02/2015) ##
* Add ability to retrieve time tracking data for an issue.
* Fixes https://bitbucket.org/farmas/atlassian.net-sdk/issues/111.
* Fixes https://bitbucket.org/farmas/atlassian.net-sdk/issues/115.

## Version 4.3.0 (07/31/2015) ##
* Add support for users to execute hand crafted REST requests to their JIRA servers.
* Fixes https://bitbucket.org/farmas/atlassian.net-sdk/issues/112.

## Version 4.2.0 (07/30/2015) ##
* Add support for 'startAt' when calling GetIssuesFromJql.
* Fixes https://bitbucket.org/farmas/atlassian.net-sdk/issues/98.

## Version 4.1.1 (07/24/2015) ##
* Fixes https://bitbucket.org/farmas/atlassian.net-sdk/issues/109.
* Fixes https://bitbucket.org/farmas/atlassian.net-sdk/issues/110.

## Version 4.1.0 (07/21/2015) ##
* Add support for reading and writting complex custom field types.
* Fixes https://bitbucket.org/farmas/atlassian.net-sdk/issues/106.
* Fixes https://bitbucket.org/farmas/atlassian.net-sdk/issues/107.
* Fixes https://bitbucket.org/farmas/atlassian.net-sdk/issues/108.

## Version 4.0.0 (07/17/2015) ##
* Add support for using REST API to interact with JIRA server. For more information see (REST API|REST API).
* __[Breaking change]__ Package now has a dependency with RestSharp and JSON.NET.

## Version 3.0.0 ##
* This version was abandoned and its package has been removed from nuget.org.

## Verson 2.5.0 (12/07/2014) ##
* Exposed the Jira internal issue identifier. https://bitbucket.org/farmas/atlassian.net-sdk/issue/71.
* Allow user to provide rolelevel and grouplevel when adding a comment to an issue. https://bitbucket.org/farmas/atlassian.net-sdk/issue/58.
* Add IsSubTask property to the IssueType class that returns whether the issue type is a sub-task. https://bitbucket.org/farmas/atlassian.net-sdk/issue/74.
* Add Issue.GetResolutionDate. https://bitbucket.org/farmas/atlassian.net-sdk/issue/84.
* Expose Jira.GetSubTaskIssueTypes.

## Verson 2.4.0 (09/27/2014) ##
* Fixed issue when comparing dates from different locales (https://bitbucket.org/farmas/atlassian.net-sdk/issue/31).
* Allow clients to construct a Jira instance with the user's access token.
* Add support for deleting an issue (https://bitbucket.org/farmas/atlassian.net-sdk/issue/42).
* Uri encode user and password when downloading attachments (https://bitbucket.org/farmas/atlassian.net-sdk/issue/44).
* Made Issue.Status read only as JIRA only supports changing it via a workflow transition.

## Version 2.3.0 (12/08/1012) ##
* Add ability to add and remove custom fields for Edit and for Action (https://bitbucket.org/farmas/atlassian.net-sdk/issue/23).

## Version 2.2.0 (06/25/1012) ##
* Add ability to query for custom field values using literal match (https://bitbucket.org/farmas/atlassian.net-sdk/issue/3).
* Add integration test for updating Assignee field.
* Improved error handling when custom field is not found in server.

## Version 2.1.0 (05/19/2012) ##
* Increased SendTimeout and ReceiveTimeout default values for WCF bindings to 10 mins. (fixes https://bitbucket.org/farmas/atlassian.net-sdk-hg/issue/28)

## Version 2.0 (04/22/2012) ##
* Add ability to set properties when adding a worklog to issue (fixes https://bitbucket.org/farmas/atlassian.net-sdk-hg/issue/18)
* Add overload to WithToken that invokes caller's function with token and with IJiraSoapClient. This makes it easier to call SOAP methods that are not included as first class methods in Jira or Issue types. (fixes https://bitbucket.org/farmas/atlassian.net-sdk-hg/issue/22)
* Add ability to delete worklog (fixes https://bitbucket.org/farmas/atlassian.net-sdk-hg/issue/19)
* __[Breaking Change]__ Add ability to query customfields that are of datetime values (fixes https://bitbucket.org/farmas/atlassian.net-sdk-hg/issue/21). 

## Version 1.6 ##
* Add support for transition an issue through a workflow (fix for https://bitbucket.org/farmas/atlassian.net-sdk-hg/issue/17)
* Add support for retrieving issues from a filter (fix for https://bitbucket.org/farmas/atlassian.net-sdk-hg/issue/15)
* Fix for https://bitbucket.org/farmas/atlassian.net-sdk-hg/issue/13

## Version 1.5 ##
* Add WithToken() method to Jira object to retry execution of method if auth token has expired
* All methods now automatically retry execution if token has expired
* Fix CustomField.Name to use GetFieldsForEdit() to retrieve custom field name (workaround for JRA-6857)

## Version 1.4 ##
* Fix for https://bitbucket.org/farmas/atlassian.net-sdk-hg/issue/9
* Fix for https://bitbucket.org/farmas/atlassian.net-sdk-hg/issue/10
* Add Refresh() method to Issue to repopulate fields from server
* Issue now exposes the Jira object that was used to create the issue

## Version 1.3 ##
* Add support to add multiple attachments on the same request
* Add support to create issue as sub-task of another (only supported in JIRA 4.4 and greater)

## Version 1.2 ##
* Fix for https://bitbucket.org/farmas/atlassian.net-sdk-hg/issue/6
* Fix Jira.GetIssueTypes() to return issues for a particular project
* Return strong typed enumerables from Jira.GetIssuePriorities(), Jira.GetIssueStatuses() and Jira.GetIssueResolutions()
* Add support for retrieving projects from Jira server

## Version 1.1 ##
* Fix for https://bitbucket.org/farmas/atlassian.net-sdk-hg/issue/4
* Add support for adding and retrieving worklogs for an issue.

## Version 1.0 ##
* Release of v1.0 (no more API breaking changes until v2).

## Version 0.13 ##
* Add work around for https://jira.atlassian.com/browse/JRA-6857. Was using GetCustomFields() when fetching details for fields but turns out this method is restricted to admins. Changed to use GetFieldsForEdit instead.
* Issue Type, Resolution, Status and Priority are now strong typed.
* Added ability to auto-fetch the issue Type, Resolution, Status and Priority when setting the field to the string value. Client does not need to know the internal id's to set these values (look at main page for examples)

## Version 0.12 ##
* Add support for querying, creating and updating issues with custom fields
* __[Breaking change]__ Issue.SaveChanges() now used to create/update issues
* [Bug Fix] Fixed an issue where "Components" was not included when creating issues.

## Version 0.11 ##
* Add support to add "Labels" to issues.
* __[Breaking change]__ Renamed "Version" and "Component" types to "ProjectVersion" and "ProjectComponent" to avoid common namespace collisions. 

## Version 0.10 ##
* Add support for retrieval of all "Versions" and "Components" from project.
* Add support for retrieval of "Affects Versions", "Fix Versions" and "Components" from an issue.
* Add support for creating and updating an issue with "Affects Versions", "Fix Versions" and "Components".
* Add support for querying for issues using Linq by "Affects Versions", "Fix Versions" and "Components".
* Add helper method for getting a single issue by key.
* Add support for getting the raw JiraSoapServiceClient WFC proxy.
* __[Breaking change]__ Renamed namespace "Atlassian.Jira.Linq" to "Atlassian.Jira.Remote" to better represents its contents

## Version 0.9 ##
* Add support for retrieving all issue types, priorities, statuses and resolutions from the JIRA server.
* __[Breaking change]__ Renamed AddAttachments() to AddAttachment().
* Add AddAttachment() overload that takes a byte array of data.
* Fix issue #3 https://bitbucket.org/farmas/atlassian.net-sdk-hg/issue/3

## Version 0.8 ##
* Add ability to specify max number of issues per request (both on the Jira object as well as using the Take() LINQ method).
## Fix https://bitbucket.org/farmas/atlassian.net-sdk-hg/issue/2
* Fix bug where LINQ methods that do not return IEnumerable were not working correctly.

## Version 0.7 ##
* Add support for attachments and comments.

## Version 0.6 ##
* Add ability to update DueDate of an Issue.