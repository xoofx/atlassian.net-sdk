1. Install the Atlassian Plugin SDK 4.x
	Follow instructions here: https://developer.atlassian.com/display/DOCS/Install+the+Atlassian+SDK+on+a+Windows+System

2. Start Jira Server
	- Run "Atlassian.Jira.Test.Integration.Setup\bin\Debug\JiraSetup.exe start" from an elevated command prompt.
	- A JIRA server will start on a separate window. 
	- Wait until the tomcat container starts.
	- Note: the process may be interrupted by a prompt that asks if you want to subscribe to atlassian's dev
		mailing list.

3. Load test data into Jira Server
	- Run "Atlassian.Jira.Test.Integration.Setup\bin\Debug\JiraSetup.exe setup".
	- Note: If the process fails with an exception, run the command again.

4. Run the unit tests in this project