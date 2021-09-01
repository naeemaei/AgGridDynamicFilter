using AgGridDynamicFilter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AgGridDynamicFilter
{
    public static partial class Extensions
    {

        public static Expression<Func<TEntity, bool>> GetExpression<TEntity, TQuery>(TQuery request)
        {
            return Predicate<TEntity, TQuery>(request);
        }

        public static Expression<Func<TEntity, bool>> CreateCondition<TEntity>(FilterModel values, params PropertyInfo[] properties)
        {
            Expression<Func<TEntity, bool>> expression = null;
            if (string.Equals(values.Type, "inrange", StringComparison.OrdinalIgnoreCase))
            {
                Expression<Func<TEntity, bool>> from = AddPredicate<TEntity>("greaterthanorequal", ChangeType(values?.Filter, properties[properties.Length - 1].PropertyType), properties.ToArray());
                Expression<Func<TEntity, bool>> to = AddPredicate<TEntity>("lessthanorequal", ChangeType(values?.FilterTo, properties[properties.Length - 1].PropertyType), properties.ToArray());

                expression = from.AndAlso(to);
            }

            else if (string.Equals(values.FilterType, "set", StringComparison.OrdinalIgnoreCase))
            {
                if (values?.Values?.Count() > 0)
                {
                    foreach (var item in values?.Values)
                    {
                        Expression<Func<TEntity, bool>> condition = AddPredicate<TEntity>("equals", ChangeType(item, properties[properties.Length - 1].PropertyType), properties.ToArray());
                        expression = expression is null ? condition : expression.OrElse(condition);
                    }
                }
                else
                    expression = e => 1 == 1;
            }

            else
                expression = AddPredicate<TEntity>(values.Type.ToLower(), ChangeType(values.Filter, properties[properties.Length - 1].PropertyType), properties.ToArray());

            return expression;
        }

        public static IQueryable<TEntity> DynamicOrderBy<TEntity, TQuery>(this IQueryable<TEntity> query, TQuery request)
        {
            var filteringModel = request as AgGridFilterPayloadModel;
            var entityProperties = typeof(TEntity).GetPublicPropertiesFromCache();
            var subPropNames = filteringModel?.SortModel?.Where(e => e.ColId.Contains(".")).Select(e => new KeyValuePair<string, string[]>(e.ColId, e.ColId.Split('.')));

            if (filteringModel is not null && filteringModel?.SortModel is not null)
            {
                foreach (var item in GetOrderByItems<TEntity, TQuery>(request))
                {
                    query = query.OrderByProperty(item.Item1, item.Item2);
                }

                //foreach (var item in filteringModel.SortModel)
                //{
                //    var domainProperty = entityProperties?.Where(e => string.Equals(e.Name, item.ColId, StringComparison.OrdinalIgnoreCase))?.SingleOrDefault();
                //    var properties = FindDomainProperties(entityProperties, subPropNames, item.ColId);


                //    if (properties is null || properties.Count == 0)
                //        continue;

                //    query = query.OrderByProperty(item.Sort, properties.ToArray());

                //}
            }

            return query;
        }

        public static IEnumerable<TEntity> DynamicOrderBy<TEntity, TQuery>(this IEnumerable<TEntity> items, TQuery request)
        {
            var filteringModel = request as AgGridFilterPayloadModel;
            var entityProperties = typeof(TEntity).GetPublicPropertiesFromCache();
            var subPropNames = filteringModel?.SortModel?.Where(e => e.ColId.Contains(".")).Select(e => new KeyValuePair<string, string[]>(e.ColId, e.ColId.Split('.')));

            if (filteringModel is not null && filteringModel?.SortModel is not null)
            {
                foreach (var item in GetOrderByItems<TEntity, TQuery>(request))
                {
                    items = items.OrderByProperty(item.Item1, item.Item2);
                }
                 
            }

            return items;
        }

        public static Expression<Func<TEntity, bool>> Predicate<TEntity, TQuery>(TQuery request)
        {
            Expression<Func<TEntity, bool>> agFilterExpression = null;
            Expression<Func<TEntity, bool>> mainExpression = null;
            Expression<Func<TEntity, bool>> finalExpression = null;
            var filteringModel = request as AgGridFilterPayloadModel;
            var subPropNames = filteringModel?.FilterModel?.Keys?.Where(e => e.Contains("__")).Select(e => new KeyValuePair<string, string[]>(e, e.Split(new string[] { "__" }, StringSplitOptions.None)));
            var entityProperties = typeof(TEntity).GetPublicPropertiesFromCache();
            var requestProperties = request.GetType().GetPublicPropertiesFromCache();

            foreach (var item in requestProperties?.Where(e => e.Name != nameof(AgGridFilterPayloadModel.FilterModel)))
            {
                Expression<Func<TEntity, bool>> directExpression = null;

                var domainProperty = entityProperties?.Where(e => string.Equals(e.Name, item.Name, StringComparison.OrdinalIgnoreCase))?.SingleOrDefault();

                if (domainProperty is null)
                    continue;

                var value = request.GetType().GetPublicPropertiesFromCache()?.Where(e => string.Equals(e.Name, item.Name, StringComparison.OrdinalIgnoreCase))?.SingleOrDefault()?.GetValue(request, null);

                if (value is not null && domainProperty.PropertyType == typeof(string))
                {
                    directExpression = AddPredicate<TEntity>("contains", ChangeType(value, domainProperty.PropertyType), domainProperty);

                    mainExpression = mainExpression is null ? directExpression : mainExpression.AndAlso(directExpression);
                }
                else if (value is not null && !string.IsNullOrEmpty(value?.ToString()))
                {
                    directExpression = AddPredicate<TEntity>("equals", ChangeType(value, domainProperty.PropertyType), domainProperty);

                    mainExpression = mainExpression is null ? directExpression : mainExpression.AndAlso(directExpression);

                }
                else
                {
                    //foreach (var property in entityProperties.Where(pi => pi.PropertyType.IsGenericType && typeof(ICollection<>).IsAssignableFrom(pi.PropertyType.GetGenericTypeDefinition()) || (pi.GetType().IsClass)))
                    {
                        // TODO: for sub class filtering
                    }
                }

            }

            if (filteringModel is not null && filteringModel?.FilterModel is not null)
            {
                foreach (var item in filteringModel.FilterModel)
                {
                    var values = item.Value;

                    if (values is null)
                        continue;

                    Expression<Func<TEntity, bool>> expression;
                    var properties = FindDomainProperties(entityProperties, subPropNames, item.Key);

                    if (properties is null || properties.Count == 0)
                        continue;

                    if (values?.Condition1 is null && values?.Condition2 is null)
                    {
                        expression = CreateCondition<TEntity>(values, properties.ToArray());
                    }

                    else if (values?.Condition1 is not null || values?.Condition2 is not null)
                    {
                        Expression<Func<TEntity, bool>> condition1Expression;
                        Expression<Func<TEntity, bool>> condition2Expression;
                        condition1Expression = CreateCondition<TEntity>(values.Condition1, properties.ToArray());
                        condition2Expression = CreateCondition<TEntity>(values.Condition2, properties.ToArray());

                        if (values?.Operator?.ToLower() == "or")
                            expression = condition1Expression.OrElse(condition2Expression);
                        else
                            expression = condition1Expression.AndAlso(condition2Expression);
                    }
                    else
                        expression = e => 1 == 1;

                    agFilterExpression = agFilterExpression is null ? expression : agFilterExpression.AndAlso(expression);
                }
            }

            if (mainExpression is not null)
                finalExpression = mainExpression;

            if (agFilterExpression is not null)
                finalExpression = finalExpression is null ? agFilterExpression : finalExpression.AndAlso(agFilterExpression);

            return finalExpression;
        }

        private static List<PropertyInfo> FindDomainProperties(IEnumerable<PropertyInfo> entityProperties, IEnumerable<KeyValuePair<string, string[]>> subPropNames, string propertyName)
        {
            var finalProperty = new List<PropertyInfo>();

            var domainProperty = entityProperties?.Where(e => string.Equals(e.Name, propertyName, StringComparison.OrdinalIgnoreCase))?.SingleOrDefault();


            if (domainProperty is null)
            {
                if (subPropNames.Any(e => e.Key == propertyName))
                {
                    foreach (var item in subPropNames?.Single(e => e.Key == propertyName).Value)
                    {
                        domainProperty = entityProperties?.Where(e => string.Equals(e.Name, item, StringComparison.OrdinalIgnoreCase))?.SingleOrDefault();

                        if (domainProperty is not null && (!domainProperty.PropertyType.IsClass || domainProperty.PropertyType == typeof(string)))
                        {
                            finalProperty.Add(domainProperty);
                            return finalProperty;

                        }

                        else if (domainProperty?.PropertyType?.IsClass == true && domainProperty?.PropertyType != typeof(string))
                        {
                            var subEntityProperties = domainProperty.PropertyType.GetPublicPropertiesFromCache();

                            var prevProperties = new List<PropertyInfo>();
                            prevProperties.Add(domainProperty);
                            prevProperties.AddRange(FindDomainProperties(subEntityProperties, subPropNames, propertyName));
                            return prevProperties;
                        }
                    }
                }
                else
                    return null;
            }
            finalProperty.Add(domainProperty);
            return finalProperty;
        }

        private static IEnumerable<(string, PropertyInfo[])> GetOrderByItems<TEntity, TQuery>(TQuery request)
        {
            var filteringModel = request as AgGridFilterPayloadModel;
            var entityProperties = typeof(TEntity).GetPublicPropertiesFromCache();
            var subPropNames = filteringModel?.SortModel?.Where(e => e.ColId.Contains(".")).Select(e => new KeyValuePair<string, string[]>(e.ColId, e.ColId.Split('.')));

            if (filteringModel is not null && filteringModel?.SortModel is not null)
            {
                foreach (var item in filteringModel.SortModel)
                {
                    var domainProperty = entityProperties?.Where(e => string.Equals(e.Name, item.ColId, StringComparison.OrdinalIgnoreCase))?.SingleOrDefault();
                    var properties = FindDomainProperties(entityProperties, subPropNames, item.ColId);


                    if (properties is null || properties.Count == 0)
                        continue;

                    yield return (item.Sort, properties.ToArray());

                }
            }
        }

    }
}
