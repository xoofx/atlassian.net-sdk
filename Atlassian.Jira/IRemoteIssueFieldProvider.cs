using Atlassian.Jira.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Atlassian.Jira
{
    /// <summary>
    /// Represents a type that can provide RemoteFieldValues.
    /// </summary>
    public interface IRemoteIssueFieldProvider
    {
        Task<RemoteFieldValue[]> GetRemoteFieldValuesAsync(CancellationToken token);
    }
}
