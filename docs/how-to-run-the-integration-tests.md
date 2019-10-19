# How to run the integration tests

Integration tests run checks against a live JIRA server that has been pre-populated with test data.

## Setup a test server

In summary, the process involves using the [Atlassian Plugin SDK](https://developer.atlassian.com/display/DOCS/Install+the+Atlassian+SDK+on+a+Windows+System)
to setup a JIRA server locally. Then use a program that uses WebDriver to automate the browser and upload the
[test data](/Atlassian.Jira.Test.Integration.Setup/TestData.zip) into the server.

Step by step instructions are available [here](/Atlassian.Jira.Test.Integration/ReadMe.txt).

## Run Tests

Once the JIRA server is setup, you can run the tests [here](/Atlassian.Jira.Test.Integration/). If you need to change
the url or account used to log in, you can modify the [BaseIntegrationTest.cs](/Atlassian.Jira.Test.Integration/BaseIntegrationTest.cs)
 file.