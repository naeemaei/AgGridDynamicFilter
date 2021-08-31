using AgGridDynamicFilter.Enums;
using AgGridDynamicFilter.Models;
using System.Collections.Generic;

namespace AgGridDynamicFilter.Test.Helpers
{
    public static class AgGridFilter
    {
        public static List<SortModel> CreateSortModel(string fieldName, string sortType = "asc")
        {
            var sortModel = new List<SortModel>();
            sortModel.Add(new SortModel { ColId = fieldName, Sort = sortType });
            return sortModel;
        }

        public static Dictionary<string, FilterModel> CreateFilterModel(string fieldName, FilterModelType filterModelType = FilterModelType.contains, FilterModelFilterType filterModelFilterType = FilterModelFilterType.text, string filter = "",
            string filterTo = "", string[] setValues = null, FilterModel condition1 = null, FilterModel condition2 = null, FilterModelOperator filterModelOperator = FilterModelOperator.AND)
        {
            var filterDictionary = new Dictionary<string, FilterModel>();
            var filterModel = new FilterModel();
            filterModel.Type = filterModelType.ToString();
            filterModel.FilterType = filterModelFilterType.ToString();
            filterModel.Filter = filter;

            if (filterModel.Type == FilterModelType.inRange.ToString())
                filterModel.FilterTo = filterTo;

            if (condition1 is not null && condition2 is not null)
            {
                filterModel.Operator = filterModelOperator.ToString();
                filterModel.Condition1 = condition1;
                filterModel.Condition2 = condition2;
            }

            if (filterModel.FilterType == FilterModelFilterType.set.ToString())
                filterModel.Values = setValues;

            filterDictionary.Add(fieldName, filterModel);

            return filterDictionary;
        }
    }
}
