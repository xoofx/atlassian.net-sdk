# How to run the integration tests

Integration tests run checks against a live JIRA server that has been pre-populated with test data. JIRA can be run using the [official Atlassian Docker images](https://hub.docker.com/r/atlassian/jira-software) 
which makes it easy to run the tests on both Jira v7 and v8.

By default the tests are run against Jira 8.5.2

## Prerequisites

The only dependency to run the integration tests are a tiny bit of knowledge of the command line, and Docker:

- Install Docker for Linux ([Ubuntu](https://docs.docker.com/install/linux/docker-ce/ubuntu/),
  [Fedora](https://docs.docker.com/install/linux/docker-ce/fedora/), and
  [others](https://docs.docker.com/install/#supported-platforms)...)
- [Install Docker for Windows](https://docs.docker.com/docker-for-windows/install/)
- [Install Docker for Mac](https://docs.docker.com/docker-for-mac/install/)

## Prepare the Jira instance

### Automated Setup ###

- Automated setup uses Selenium WebDriver to automate the Chrome browser to run through the JIRA setup wizard. It requires the machine to have Chrome installed.
- To prepare the Jira instance, there is a batch file that runs Docker and restore the test data:
```
start-jira-with-data.bat
```

- Wait until the Chrome window is closed. If successful, the console should show `--- Finished setting up Jira ---`.

### Manual Setup ###

Useful if anything fails with the automated setup above.

- Launch the docker container.
```
SET JIRA_VERSION=8.5.2
docker-compose up -d
```
- Navigate to http://localhost:8080.
- Once the setup wizard loads click on “I’ll set it up myself” and “Next”.
- On the next screen Keep “Built in” selected and click on “Next” (wait until database is setup).
- On the next screen click on the “import your data” link at the top.
- Enter “TestData_8.5.2.zip” in the “File Name” field and click on “Import” (wait until the test data is imported).

### Setup a different version of Jira
You can setup a different version by running:
```
$ start-jira-with-data.bat <version>
```

Note: Each Jira version requires a different test data file, look in the `Atlassian.Jira.Test.Integration.Setup/import/` folder for the versions that are available. For example, if there is a file `Atlassian.Jira.Test.Integration.Setup/import/TestData_8.5.0.zip` then can run:
```
$ start-jira-with-data.bat 8.5.0
```

## Run the integration tests

Once the image is up and running you can run the integration tests within your IDE.

But you might prefer the command line:
```
$ dotnet test Atlassian.Jira.Test.Integration/
```

## Clean the container

To clean the Docker containers launched for the integration tests:
```
$ docker-compose down -v
```
