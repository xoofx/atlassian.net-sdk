using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Atlassian.Jira.Test
{
    public class CustomFieldOptionTest
    {
        [Fact]
        public void AllowedValuesCanBeAbsent()
        {
            IssueFieldEditMetadata m  = new IssueFieldEditMetadata();
            m.AllowedValuesAs<Object>();
        }
    }
}
