using System;

namespace Atlassian.Jira
{
    internal static class ExtensionMethods
    {
        public static string ToJiraDateTimeString(this DateTime value)
        {
            /* Using "en-us" culture to conform to formats of JIRA.
             * See https://bitbucket.org/farmas/atlassian.net-sdk/issue/31
             */
            return value.ToString(
                value.TimeOfDay == TimeSpan.Zero ? Jira.DEFAULT_DATE_FORMAT : Jira.DEFAULT_DATE_TIME_FORMAT,
                Jira.DefaultCultureInfo);
        }
    }
}
