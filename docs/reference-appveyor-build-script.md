# AppVeyor Build script for v10 #

```
dotnet build -c Release -p:Version=$env:APPVEYOR_REPO_TAG_NAME

.\pfx2snk.ps1 -pfxFilePath .\Atlassian.Jira\Atlassian.Jira.pfx -pfxPassword $env:pfxPwd

dotnet build -c Release -p:Version=$env:APPVEYOR_REPO_TAG_NAME .\Atlassian.Jira\Atlassian.Jira.Signed.csproj
```