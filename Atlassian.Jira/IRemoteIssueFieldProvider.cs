using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Remote;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents a type that can provide RemoteFieldValues. 
    /// </summary>
    public interface IRemoteIssueFieldProvider
    {
        RemoteFieldValue[] GetRemoteFields(string fieldName);
    }
}
