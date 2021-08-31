using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AgGridDynamicFilter.Extensions
{
    public static partial class Extensions
    {
        private static readonly MethodInfo QueryableOrderByMethod = typeof(Queryable).GetMethods().Single(method => method.Name == "OrderBy" && method.GetParameters().Length == 2);

        private static readonly MethodInfo QueryableOrderByDescendingMethod = typeof(Queryable).GetMethods().Single(method => method.Name == "OrderByDescending" && method.GetParameters().Length == 2);

        private static readonly MethodInfo EnumerableOrderByMethod = typeof(Enumerable).GetMethods().Single(method => method.Name == "OrderBy" && method.GetParameters().Length == 2);

        private static readonly MethodInfo EnumerableOrderByDescendingMethod = typeof(Enumerable).GetMethods().Single(method => method.Name == "OrderByDescending" && method.GetParameters().Length == 2);

        private static readonly MethodInfo AnyMethod = typeof(Queryable).GetMethods().Single(method => method.Name == "Any" && method.GetParameters().Length == 2);

        private static readonly MethodInfo ContainsMethod = typeof(string).GetMethods().Single(method => method.Name == "Contains" && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(string));

        private static readonly MethodInfo StartsWithMethod = typeof(string).GetMethods().Single(method => method.Name == "StartsWith" && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(string));

        private static readonly MethodInfo EndsWithMethod = typeof(string).GetMethods().Single(method => method.Name == "EndsWith" && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(string));


        public static Expression<Func<TEntity, bool>> AndAlso<TEntity>(this Expression<Func<TEntity, bool>> expr1, Expression<Func<TEntity, bool>> expr2)
        {
            // need to detect whether they use the same
            // parameter instance; if not, they need fixing
            ParameterExpression param = expr1.Parameters[0];
            if (ReferenceEquals(param, expr2.Parameters[0]))
            {
                // simple version
                return Expression.Lambda<Func<TEntity, bool>>(
                    Expression.AndAlso(expr1.Body, expr2.Body), param);
            }
            // otherwise, keep expr1 "as is" and invoke expr2
            return Expression.Lambda<Func<TEntity, bool>>(
                Expression.AndAlso(
                    expr1.Body,
                    Expression.Invoke(expr2, param)), param);
        }

        public static Expression<Func<TEntity, bool>> OrElse<TEntity>(this Expression<Func<TEntity, bool>> expr1, Expression<Func<TEntity, bool>> expr2)
        {
            // need to detect whether they use the same
            // parameter instance; if not, they need fixing
            ParameterExpression param = expr1.Parameters[0];
            if (ReferenceEquals(param, expr2.Parameters[0]))
            {
                // simple version
                return Expression.Lambda<Func<TEntity, bool>>(
                    Expression.OrElse(expr1.Body, expr2.Body), param);
            }
            // otherwise, keep expr1 "as is" and invoke expr2
            return Expression.Lambda<Func<TEntity, bool>>(
                Expression.OrElse(
                    expr1.Body,
                    Expression.Invoke(expr2, param)), param);
        }

        public static Expression<Func<TEntity, bool>> AddPredicate<TEntity>(string method, Expression left, Expression right, ParameterExpression tpe = null)
        {
            if (tpe is null)
                tpe = Expression.Parameter(typeof(TEntity), "t");

            Expression innerLambda = method.ToLower() switch
            {
                "contains" => Expression.Call(left, ContainsMethod, right),
                "notContains" => Expression.Not(Expression.Call(left, ContainsMethod, right)),
                "startswith" => Expression.Call(left, StartsWithMethod, right),
                "endswith" => Expression.Call(left, EndsWithMethod, right),
                "equals" => Expression.Equal(left, right),
                "notequal" => Expression.NotEqual(left, right),
                "lessthan" => Expression.LessThan(left, right),
                "lessthanorequal" => Expression.LessThanOrEqual(left, right),
                "greaterthan" => Expression.GreaterThan(left, right),
                "greaterthanorequal" => Expression.GreaterThanOrEqual(left, right),
                _ => throw new ArgumentException()
            };

            Expression<Func<TEntity, bool>> innerFunction = Expression.Lambda<Func<TEntity, bool>>(innerLambda, tpe);
            return innerFunction;
        }

        public static IQueryable<TEntity> OrderByProperty<TEntity>(this IQueryable<TEntity> source, string orderType = "asc", params PropertyInfo[] properties)
        {
            if (typeof(TEntity).GetProperty(properties[0].Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) == null)
            {
                return null;
            }

            ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity), "t");
            Expression orderByProperty = parameterExpression;

            foreach (var item in properties)
            {
                orderByProperty = Expression.Property(orderByProperty, item);
            }

            LambdaExpression lambda = Expression.Lambda(orderByProperty, parameterExpression);
            MethodInfo genericMethod = orderType == "asc" ? QueryableOrderByMethod.MakeGenericMethod(typeof(TEntity), orderByProperty.Type) : QueryableOrderByDescendingMethod.MakeGenericMethod(typeof(TEntity), orderByProperty.Type);
            object ret = genericMethod.Invoke(null, new object[] { source, lambda });
            return (IQueryable<TEntity>)ret;
        }

        public static IEnumerable<TEntity> OrderByProperty<TEntity>(this IEnumerable<TEntity> source, string orderType = "asc", params PropertyInfo[] properties)
        {
            var (genericMethod, lambda) = GetOrderByMethod<TEntity>(orderType, properties);
            object ret = genericMethod.Invoke(null, new object[] { source, lambda.Compile() });
            return (IEnumerable<TEntity>)ret;
        }


        public static Expression<Func<TEntity, bool>> AddPredicate<TEntity>(string method, object value, params PropertyInfo[] properties)
        {
            if (typeof(TEntity).GetProperty(properties[0].Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) == null)
            {
                return null;
            }

            ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity), "t");
            Expression member = parameterExpression;
            foreach (var item in properties)
            {
                member = Expression.Property(member, item);
            }

            var propertyType = properties[properties.Length - 1].PropertyType;
            var converter = TypeDescriptor.GetConverter(propertyType);
            var propertyValue = converter.ConvertFromInvariantString(value.ToString());
            var constant = Expression.Constant(propertyValue);
            var valueExpression = Expression.Convert(constant, propertyType);
            var body = AddPredicate<TEntity>(method, member, valueExpression, parameterExpression);
            return body;
        }

        private static (MethodInfo, LambdaExpression) GetOrderByMethod<TEntity>(string orderType = "asc", params PropertyInfo[] properties)
        {
            if (typeof(TEntity).GetProperty(properties[0].Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) == null)
            {
                return default;
            }

            ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity), "t");
            Expression orderByProperty = parameterExpression;

            foreach (var item in properties)
            {
                orderByProperty = Expression.Property(orderByProperty, item);
            }

            LambdaExpression lambda = Expression.Lambda(orderByProperty, parameterExpression);
            MethodInfo genericMethod = orderType == "asc" ? EnumerableOrderByMethod.MakeGenericMethod(typeof(TEntity), orderByProperty.Type) : EnumerableOrderByDescendingMethod.MakeGenericMethod(typeof(TEntity), orderByProperty.Type);
            return (genericMethod, lambda);
        }

    }
}
