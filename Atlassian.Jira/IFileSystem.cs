namespace Atlassian.Jira
{
    public interface IFileSystem
    {
        byte[] FileReadAllBytes(string path);
    }
}
