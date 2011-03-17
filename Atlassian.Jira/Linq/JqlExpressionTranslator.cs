using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace Atlassian.Jira.Linq
{
    public class JqlExpressionTranslator: ExpressionVisitor
    {
        private readonly StringBuilder _jql;

        public JqlExpressionTranslator()
        {
            _jql = new StringBuilder();
        }


        public string Jql 
        { 
            get
            {
                return _jql.ToString();
            } 
        }

        private Tuple<PropertyInfo, object> DecomposeConstantOperatorExpression(BinaryExpression expression)
        {
            
            var memberExpression = expression.Left as MemberExpression;
            if (memberExpression != null)
            {
                var propertyInfo = memberExpression.Member as PropertyInfo;
                if (propertyInfo != null)
                {
                    var constant = expression.Right as ConstantExpression;
                    if (constant != null)
                    {
                        return Tuple.Create<PropertyInfo, object>(propertyInfo, constant.Value);
                    }
                }
            }

            throw new NotSupportedException(String.Format(
                   "Operator '{0}' can only be applied on fields with constant values.",
                   expression.NodeType));
        }

        private void ProcessNumbericOperatorExpression(BinaryExpression expression, string operatorString)
        {
            var tuple = DecomposeConstantOperatorExpression(expression);

            _jql.Append(tuple.Item1.Name);
            _jql.Append(operatorString);
            _jql.Append(tuple.Item2);
        }

        private void ProcessEqualityOperator(BinaryExpression expression, string stringOperator, string normalOperator, string isOperator)
        {
            var tuple = DecomposeConstantOperatorExpression(expression);

            // field
            _jql.Append(tuple.Item1.Name);

            // special case for empty string
            if (tuple.Item2 == null)
            {
                _jql.Append(isOperator + "null");
                return;
            }
            else if(tuple.Item2.Equals(""))
            {
                _jql.Append(isOperator + "empty");
                return;
            }

            // operator
            if(tuple.Item1.PropertyType == typeof(String))
            {
                
                // special case for empty
                _jql.Append(stringOperator);
            }
            else
            {
                _jql.Append(normalOperator);
            }

            // value
            if (tuple.Item2.GetType() == typeof(String))
            {
                _jql.Append(String.Format("\"{0}\"", tuple.Item2));
            }
            else
            {
                _jql.Append(tuple.Item2);
            }

        }

        private void ProcessUnionOperator(BinaryExpression expression, string operatorString)
        {
            _jql.Append("(");
            Visit(expression.Left);
            _jql.Append(operatorString);
            Visit(expression.Right);
            _jql.Append(")");
        }


        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.GreaterThan:
                    ProcessNumbericOperatorExpression(node, " > ");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    ProcessNumbericOperatorExpression(node, " >= ");
                    break;

                case ExpressionType.LessThan:
                    ProcessNumbericOperatorExpression(node, " < ");
                    break;

                case ExpressionType.LessThanOrEqual:
                    ProcessNumbericOperatorExpression(node, " <= ");
                    break;

                case ExpressionType.Equal:
                    ProcessEqualityOperator(node, " ~ ", " = ", " is ");
                    break;

                case ExpressionType.NotEqual:
                    ProcessEqualityOperator(node, " !~ ", " != ", " is not ");
                    break;

                case ExpressionType.AndAlso:
                    ProcessUnionOperator(node, " and ");
                    break;

                case ExpressionType.OrElse:
                    ProcessUnionOperator(node, " or ");
                    break;

                default:
                    throw new NotSupportedException(String.Format("Expression type '{0}' is not supported.", node.NodeType));

            }

            return node;
        }

    }
}
