using System;
using Xunit;

namespace Atlassian.Jira.Test
{
    public class IssueFieldMetadataTest
    {
        [Fact]
        public void AllowedValuesCanBeAbsent()
        {
            IssueFieldEditMetadata m = new IssueFieldEditMetadata();
            m.AllowedValuesAs<Object>();
        }
    }
}