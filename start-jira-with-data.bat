IF NOT "%~1"=="" (
	SET JIRA_VERSION=%1
) ELSE (
	SET JIRA_VERSION=8.5.2
)

docker-compose up -d

Atlassian.Jira.Test.Integration.Setup\bin\net452\JiraSetup.exe %JIRA_VERSION%