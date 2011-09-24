using System;
namespace Atlassian.Jira.Remote
{
    /// <summary>
    /// Abstracts the translation of an Expression tree into JQL
    /// </summary>
    public interface IJqlExpressionVisitor
    {
        JqlData Process(System.Linq.Expressions.Expression expression);
    }
}
