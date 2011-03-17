using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace Atlassian.Jira.Linq
{
    public class JqlExpressionTranslator: ExpressionVisitor, IJqlExpressionTranslator
    {
        private StringBuilder _jqlWhere;
        private StringBuilder _jqlOrderBy;

        public string Jql 
        { 
            get
            {
                return _jqlWhere.ToString() + _jqlOrderBy.ToString();
            } 
        }

        public string Translate(Expression expression)
        {
            _jqlWhere = new StringBuilder();
            _jqlOrderBy = new StringBuilder();

            this.Visit(expression);
            return Jql;
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

            // field
            _jqlWhere.Append(tuple.Item1.Name);

            // operator
            _jqlWhere.Append(String.Format(" {0} ", operatorString));

            // value
            ProcessConstant(tuple.Item2);
        }

        private void ProcessEqualityOperator(BinaryExpression expression, bool equal)
        {
            var tuple = DecomposeConstantOperatorExpression(expression);

            // field
            _jqlWhere.Append(tuple.Item1.Name);

            // special cases for empty/null string
            if (tuple.Item2 == null || tuple.Item2.Equals(""))
            {
                _jqlWhere.Append(" ");
                _jqlWhere.Append(equal? Operators.IS : Operators.ISNOT);
                _jqlWhere.Append(" ");
                _jqlWhere.Append(tuple.Item2 == null? "null" : "empty");
                return;
            }

            // operator
            var operatorString = String.Empty;
            if(tuple.Item1.GetCustomAttributes(typeof(ContainsEqualityAttribute), true).Count() > 0)
            {
                operatorString = equal? Operators.CONTAINS: Operators.NOTCONTAINS;
            }
            else
            {
                operatorString = equal? Operators.EQUALS: Operators.NOTEQUALS;
            }
            _jqlWhere.Append(String.Format(" {0} ", operatorString));

            // value
            ProcessConstant(tuple.Item2);
        }

        private void ProcessConstant(object value)
        {
            var valueType = value.GetType();
            if (valueType == typeof(String)
                || valueType == typeof(ComparableTextField))
            {
                _jqlWhere.Append(String.Format("\"{0}\"", value));
            }
            else
            {
                _jqlWhere.Append(value);
            }
        }

        private void ProcessUnionOperator(BinaryExpression expression, string operatorString)
        {
            _jqlWhere.Append("(");
            Visit(expression.Left);
            _jqlWhere.Append(operatorString);
            Visit(expression.Right);
            _jqlWhere.Append(")");
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "OrderBy" 
                || node.Method.Name == "OrderByDescending"
                || node.Method.Name == "ThenBy"
                || node.Method.Name == "ThenByDescending")
            {
                ProcessOrderBy(node);
            }

            return base.VisitMethodCall(node) ;
        }

        private void ProcessOrderBy(MethodCallExpression node)
        {
            var firstOrderBy = _jqlOrderBy.Length == 0;
            if (firstOrderBy)
            {
                _jqlOrderBy.Append(" order by ");
            }
           
            var member = ((LambdaExpression)((UnaryExpression)node.Arguments[1]).Operand).Body as MemberExpression;
            if (member != null)
            {
                if (firstOrderBy)
                {
                    _jqlOrderBy.Append(member.Member.Name);
                }
                else
                {
                    _jqlOrderBy.Insert(10, member.Member.Name + ", ");
                }
            }

            if (node.Method.Name == "OrderByDescending"
                || node.Method.Name == "ThenByDescending")
            {
                _jqlOrderBy.Append(" desc");
            }
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.GreaterThan:
                    ProcessNumbericOperatorExpression(node, Operators.GREATERTHAN);
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    ProcessNumbericOperatorExpression(node, Operators.GREATERTHANOREQUALS);
                    break;

                case ExpressionType.LessThan:
                    ProcessNumbericOperatorExpression(node, Operators.LESSTHAN);
                    break;

                case ExpressionType.LessThanOrEqual:
                    ProcessNumbericOperatorExpression(node, Operators.LESSTHANOREQUALS);
                    break;

                case ExpressionType.Equal:
                    ProcessEqualityOperator(node, true);
                    break;

                case ExpressionType.NotEqual:
                    ProcessEqualityOperator(node, false);
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
