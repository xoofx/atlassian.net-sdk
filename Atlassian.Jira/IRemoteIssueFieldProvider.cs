using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atlassian.Jira.Linq;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents a type that can provide RemoteFieldValues. 
    /// </summary>
    /// <remarks>
    /// This was created to avoid polluting the Issue type with methods used in testing. Can be removed once test assembly
    /// has access to internals
    /// </remarks>
    public interface IRemoteIssueFieldProvider
    {
        RemoteFieldValue[] GetRemoteFields(string fieldName);
    }
}
