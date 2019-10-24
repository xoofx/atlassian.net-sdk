# How to run the integration tests

It is possible to run the integration tests using the [official Atlassian Docker images](https://hub.docker.com/r/atlassian/jira-software),
which makes it easy to run the tests on both Jira v7 and v8. For now, integration tests have to be run manually.

Since Atlassian SDK needs specific data for each version, it supports one version per major version:
* v7.13
* v8.5

By default the tests are run against Jira 7.13.

## Prerequisites

The only dependency to run the integration tests are a tiny bit of knowledge of the command line, and Docker:
* Install Docker for Linux ([Ubuntu](https://docs.docker.com/install/linux/docker-ce/ubuntu/),
  [Fedora](https://docs.docker.com/install/linux/docker-ce/fedora/), and
  [others](https://docs.docker.com/install/#supported-platforms)...)
* [Install Docker for Windows](https://docs.docker.com/docker-for-windows/install/)
* [Install Docker for Mac](https://docs.docker.com/docker-for-mac/install/)

## Prepare the Jira instance

To prepare the Jira instance, there is a batch file that runs Docker and restore the test data:
```
$ start-jira-with-data.bat
```

Wait until the Chrome window is closed. If successful, the console should show `--- Finished setting up Jira ---`.

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
