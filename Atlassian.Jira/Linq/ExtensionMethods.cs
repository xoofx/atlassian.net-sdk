using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Atlassian.Jira.Linq
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Create a new RemoteIssue based on the information in a given issue
        /// </summary>
        public static RemoteIssue ToRemote(this Issue issue)
        {
            var remote = new RemoteIssue()
            {
                assignee = issue.Assignee,
                description = issue.Description,
                environment = issue.Environment,
                project = issue.Project,
                reporter = issue.Reporter,
                status = issue.Status,
                summary = issue.Summary,
                type = issue.Type,
                votes = issue.Votes
            };

            remote.key = issue.Key != null ? issue.Key.Value : null;
            remote.priority = issue.Priority != null ? issue.Priority.Value : null;
            remote.resolution = issue.Resolution != null ? issue.Resolution.Value : null;

            return remote;
        }

        /// <summary>
        /// Create a new Issue from a RemoteIssue
        /// </summary>
        public static Issue ToLocal(this RemoteIssue remoteIssue, Jira jira = null)
        {
            return new Issue(jira, remoteIssue);
        }

        /// <summary>
        /// Create a new Attachment from a RemoteAttachment
        /// </summary>
        public static Attachment ToLocal(this RemoteAttachment remoteAttachment, Jira jira, IWebClient webClient)
        {
            return new Attachment(jira, webClient, remoteAttachment);
        }

        /// <summary>
        /// Gets the RemoteFieldValues representing the fields that where updated
        /// </summary>
        public static RemoteFieldValue[] GetUpdatedFields(this Issue issue)
        {
            var fields = new List<RemoteFieldValue>();

            var remoteFields = typeof(RemoteIssue).GetProperties();
            foreach (var localProperty in typeof(Issue).GetProperties())
            {
                var remoteProperty = remoteFields.FirstOrDefault(i => i.Name.Equals(localProperty.Name, StringComparison.OrdinalIgnoreCase));
                if (remoteProperty == null)
                {
                    continue;
                }

                if (!typeof(IEnumerable<Version>).IsAssignableFrom(localProperty.PropertyType))
                {
                    var localStringValue = GetStringValueForProperty(issue, localProperty);
                    var remoteStringValue = GetStringValueForProperty(issue.OriginalRemoteIssue, remoteProperty);

                    if (remoteStringValue != localStringValue)
                    {
                        fields.Add(new RemoteFieldValue()
                        {
                            id = remoteProperty.Name,
                            values = new string[1] { localStringValue }
                        });
                    }
                }
            }

            return fields.ToArray();
        }

        private static string GetStringValueForProperty(object container, PropertyInfo property)
        {
            var value = property.GetValue(container, null);

            if (property.PropertyType == typeof(DateTime?))
            {
                var dateValue = (DateTime?)value;
                return dateValue.HasValue ? dateValue.Value.ToString("d/MMM/yy") : null;
            }
            else
            {
                return value != null ? value.ToString() : null;
            }
        }
    }
}
