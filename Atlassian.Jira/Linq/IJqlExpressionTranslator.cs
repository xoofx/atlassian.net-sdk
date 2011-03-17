using System;
namespace Atlassian.Jira.Linq
{
    public interface IJqlExpressionTranslator
    {
        string Translate(System.Linq.Expressions.Expression expression);
    }
}
