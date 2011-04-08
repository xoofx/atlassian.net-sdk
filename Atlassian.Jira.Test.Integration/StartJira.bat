atlas-run-standalone --product jira --version 4.3 --lib-plugins com.atlassian.jira.plugins:atlassian-jira-rpc-plugin:4.3 "-DdataPath=jira-plugin-test-resources-4.3-rpc-enabled.zip"


mvn.bat com.atlassian.maven.plugins:maven-amps-plugin:3.3:run-standalone  -Dproduct=jira "-Dproduct.version=4.3" "-Dlib.plugins=com.atlassian.jira.plugins:atlassian-jira-rpc-plugin:4.3" "-DproductDataPath=C:/Atlas/dev/Atlassian.Net SDK/Atlassian.Jira.Test.Integration/jira-plugin-test-resources-4.3-rpc-enabled.zip"