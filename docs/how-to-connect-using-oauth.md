# How to Connect to Jira using OAuth

In order to connect to Jira using OAuth first you must configure an Application Link in Jira that uses OAuth for incoming requests, then retrieve a request and access tokens and provide them when creating a new Jira rest client. 

The detailed process is as follows:

## 1. Install the jira-oauth-cli Tool

```
dotnet tool install -g jira-oauth-cli
```

## 2. Gather an admin username/password and decide on a ConsumerKey

In order to create an Application Link you must have admin privileges on a Jira server. The ConsumerKey is a identifier that you will use when setting up the OAuth link.

## 3. Run the CLI tool for manual setup

```
jira-oauth-cli setup --url <jira_url> -u <jira_user> -p <jira_password> -k <consumer_key>
```

## 4. Follow the instructions to setup the AppLink

The jira-oauth-cli will point you to instructions in https://developer.atlassian.com/server/jira/platform/oauth and will print out the public key to use when configuring the AppLink.

Follow the instructions carefully on the page above and when asked, use the ConsumerKey that you decided on step #2, and the copy/paste then public key that was generated. (Note: include the header and footer of the public key).

## 5. Continue running CLI tool and retrieve tokens.

Once you finished configuring the AppLink, go back to the jira-oauth-cli command and press any key to let it continue. The tool will issue the appropiate requests to get a request and access tokens and will print them.

## 6. Use the tokens and secret to create a new Jira rest client.

```csharp
var jira = Jira.CreateOAuthRestClient(
    YOUR_JIRA_URL,
    YOUR_CONSUMER_KEY,    // as was decided on step #2    
    YOUR_CONSUMER_SECRET, // as was printed by the cli tool.
    YOUR_ACCESS_TOKEN,    // as was printed by the cli tool.
    YOUT_TOKEN_SECRET);   // as was printed by the cli tool.
```

* Note that the consumer secret is the private key generated in XML format and is a very long string. Keep it safe.

At this point you can interact with the Jira object normally.