IF NOT "%~1"=="" (
	SET JIRA_VERSION=%1
)

docker-compose up -d

Atlassian.Jira.Test.Integration.Setup\bin\net452\JiraSetup.exe %1