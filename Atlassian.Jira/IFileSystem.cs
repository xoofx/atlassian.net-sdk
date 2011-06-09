using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    public interface IFileSystem
    {
        byte[] FileReadAllBytes(string path);
    }
}
