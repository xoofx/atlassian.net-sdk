xcopy jira-plugin-test-resources-4.3-rpc-enabled.zip amps-standalone\target\jira\test-resources.* /R /Y

atlas-run-standalone --product jira --version 4.3 --lib-plugins com.atlassian.jira.plugins:atlassian-jira-rpc-plugin:4.3