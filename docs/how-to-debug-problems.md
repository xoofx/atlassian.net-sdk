# How to debug problems

## Turn on Request Tracing

When this setting is enabled, the SDK will trace all requests sent to your server and their responses. It uses the
standard [.NET Tracing](https://msdn.microsoft.com/en-us/library/zs6s4h68(v=vs.110).aspx) mechanism.

```csharp
var settings = new JiraRestClientSettings()
{
   EnableRequestTrace = true
};

var jira = Jira.CreateRestClient("<url>", "<username>", "<pwd>", settings);
```
