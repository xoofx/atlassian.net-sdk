1. Install the Atlassian Plugin SDK 6.2
	Follow instructions here: https://developer.atlassian.com/display/DOCS/Install+the+Atlassian+SDK+on+a+Windows+System

2. Start Jira Server
	- Run "Atlassian.Jira.Test.Integration.Setup\bin\Debug\JiraSetup.exe start" from an elevated command prompt.
	- A JIRA server will start on a separate window.
	- Wait until the tomcat container starts.
	- Note: the process may be interrupted by a prompt that asks if you want to subscribe to atlassian's dev
		mailing list.

3. Login to Jira and skip all the tutorials if needed.

4. Load test data into Jira Server
	- Run "Atlassian.Jira.Test.Integration.Setup\bin\Debug\JiraSetup.exe restore <admin_user> <admin_pass>".
	- Note: If the process fails with an exception, run the command again.

5. Run the tests in this project.
