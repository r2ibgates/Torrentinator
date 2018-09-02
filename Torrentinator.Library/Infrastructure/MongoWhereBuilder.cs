using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Torrentinator.Library.Infrastructure
{
    public class MongoWhereBuilder
    {
        public static string ToMongoSql<T>(Expression<Func<T, bool>> expression)
        {
            if (expression == null)
                return "{}";
            return Recurse(expression.Body, true);
        }

        private static string Recurse(Expression expression, bool isUnary = false, bool quote = true)
        {
            if (expression == null)
                return "{}";
            if (expression is UnaryExpression)
            {
                var unary = (UnaryExpression)expression;
                return Recurse(unary.Operand, true).Trim();
            }
            if (expression is BinaryExpression)
            {
                var body = (BinaryExpression)expression;
                var right = Recurse(body.Right);
                var left = Recurse(body.Left);

                switch (body.NodeType)
                {
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                        return "{$and:[" + left + ", " + right + "]}";

                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                        return "{$or:[" + left + ", " + right + "]}";

                    case ExpressionType.NotEqual:
                        return "{" + left + " : {$ne : " + right + "}}";

                    case ExpressionType.GreaterThan:
                        return "{" + left + " : {$gt : " + right + "}}";

                    case ExpressionType.GreaterThanOrEqual:
                        return "{" + left + " : {$gte : " + right + "}}";

                    case ExpressionType.LessThan:
                        return "{" + left + " : {$lt : " + right + "}}";

                    case ExpressionType.LessThanOrEqual:
                        return "{" + left + " : {$lte : " + right + "}}";

                    default:
                        return "{" + left + " " + NodeTypeToString(body.NodeType, right == "NULL") + " " + right + "}";
                }
            }
            if (expression is ConstantExpression)
            {
                var constant = (ConstantExpression)expression;
                return ValueToString(constant.Value, isUnary, quote);
            }
            if (expression is MemberExpression)
            {
                var member = (MemberExpression)expression;

                if (member.Member is PropertyInfo)
                {
                    var property = (PropertyInfo)member.Member;
                    return property.Name;
                }
                if (member.Member is FieldInfo)
                {
                    return ValueToString(GetValue(member), isUnary, quote);
                }
                throw new Exception($"Expression does not refer to a property or field: {expression}");
            }
            if (expression is MethodCallExpression)
            {
                var methodCall = (MethodCallExpression)expression;
                // LIKE queries:
                if (methodCall.Method == typeof(string).GetMethod("Contains", new[] { typeof(string) }))
                {
                    return "{" + Recurse(methodCall.Object) + " : /" + Recurse(methodCall.Arguments[0], quote: false) + "/gi}";
                }
                if (methodCall.Method == typeof(string).GetMethod("StartsWith", new[] { typeof(string) }))
                {
                    return "{" + Recurse(methodCall.Object) + " : /^" + Recurse(methodCall.Arguments[0], quote: false) + "/gi}";
                }
                if (methodCall.Method == typeof(string).GetMethod("EndsWith", new[] { typeof(string) }))
                {
                    return "{" + Recurse(methodCall.Object) + " : /" + Recurse(methodCall.Arguments[0], quote: false) + "$/gi}";
                }
                // IN queries:
                if (methodCall.Method.Name == "Contains")
                {
                    Expression collection;
                    Expression property;
                    if (methodCall.Method.IsDefined(typeof(ExtensionAttribute)) && methodCall.Arguments.Count == 2)
                    {
                        collection = methodCall.Arguments[0];
                        property = methodCall.Arguments[1];
                    }
                    else if (!methodCall.Method.IsDefined(typeof(ExtensionAttribute)) && methodCall.Arguments.Count == 1)
                    {
                        collection = methodCall.Object;
                        property = methodCall.Arguments[0];
                    }
                    else
                    {
                        throw new Exception("Unsupported method call: " + methodCall.Method.Name);
                    }
                    var values = (IEnumerable)GetValue(collection);
                    var concated = "";
                    foreach (var e in values)
                    {
                        concated += ValueToString(e, false, true) + ", ";
                    }
                    if (concated == "")
                    {
                        return ValueToString(false, true, false);
                    }
                    return "(" + Recurse(property) + " IN (" + concated.Substring(0, concated.Length - 2) + "))";
                }
                throw new Exception("Unsupported method call: " + methodCall.Method.Name);
            }
            throw new Exception("Unsupported expression: " + expression.GetType().Name);
        }

        private static string ValueToString(object value, bool isUnary, bool quote)
        {
            if (value is bool)
            {
                if (isUnary)
                {
                    return (bool)value ? "{}" : "(1=0)";
                }
                return (bool)value ? "true" : "false";
            }
            else if ((value != null) && (value.GetType().IsEnum))
            {
                return ValueToString(Convert.ChangeType(value, value.GetType().GetEnumUnderlyingType()), isUnary, quote);
            }
            return (value == null ? "null" : (quote && (value is string) ? $"'{value}'" : $"{value}").Replace("/", "\\/"));
        }

        private static bool IsEnumerableType(Type type)
        {
            return type
                .GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        private static object GetValue(Expression member)
        {
            // source: http://stackoverflow.com/a/2616980/291955
            var objectMember = Expression.Convert(member, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            return getter();
        }

        private static object NodeTypeToString(ExpressionType nodeType, bool rightIsNull)
        {
            switch (nodeType)
            {
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.And:
                    return "&";
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Equal:
                    return rightIsNull ? "IS" : ":";
                case ExpressionType.ExclusiveOr:
                    return "^";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.Modulo:
                    return "%";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Negate:
                    return "-";
                case ExpressionType.Not:
                    return "NOT";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.Or:
                    return "|";
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Subtract:
                    return "-";
                case ExpressionType.Convert:
                    return "";
            }
            throw new Exception($"Unsupported node type: {nodeType}");
        }
    }
}
