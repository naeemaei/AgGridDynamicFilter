using AgGridDynamicFilter.Enums;
using AgGridDynamicFilter.Models;
using AgGridDynamicFilter.SampleData.Models;
using AgGridDynamicFilter.Test.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace AgGridDynamicFilter.Test
{
    public partial class OlympicAthleteTest
    {


        public static IEnumerable<object[]> AgeFilterAgeSort()
        {
            var result = new List<object[]>
            {
                new object[]
                {
                    0,
                    12,
                    AgGridFilter.CreateSortModel(
                        fieldName: nameof(OlympicWinner.Country),
                        sortType:  "desc"),
                    AgGridFilter.CreateFilterModel(

                        fieldName: nameof(OlympicWinner.Age),
                        filterModelType: FilterModelType.inRange,
                        filterModelFilterType: FilterModelFilterType.number,
                        filter: "18",
                        filterTo: "26"),

                }

            };
            return result;
        }

        public static IEnumerable<object[]> AgeAndCountryFilterNameSort()
        {
            var result = new List<object[]>
            {
                new object[]
                {
                   0,
                    12,
                    AgGridFilter.CreateSortModel(
                        fieldName: nameof(OlympicWinner.Athlete),
                        sortType:  "asc"),
                    AgGridFilter.CreateFilterModel(

                        fieldName: nameof(OlympicWinner.Country),

                        condition1: AgGridFilter.CreateFilterModel(
                            fieldName: nameof(OlympicWinner.Country),
                            filterModelType: FilterModelType.contains,
                            filterModelFilterType: FilterModelFilterType.text,
                            filter: "Iran")[nameof(OlympicWinner.Country)],

                        condition2: AgGridFilter.CreateFilterModel(
                            fieldName: nameof(OlympicWinner.Country),
                            filterModelType: FilterModelType.contains,
                            filterModelFilterType: FilterModelFilterType.text,
                            filter: "Iraq")[nameof(OlympicWinner.Country)],
                        filterModelOperator: FilterModelOperator.OR).Concat(

                        AgGridFilter.CreateFilterModel(

                        fieldName: nameof(OlympicWinner.Age),

                        condition1: AgGridFilter.CreateFilterModel(
                            fieldName: nameof(OlympicWinner.Age),
                            filterModelType: FilterModelType.greaterThanOrEqual,
                            filterModelFilterType: FilterModelFilterType.number,
                            filter: "23")[nameof(OlympicWinner.Age)],

                        condition2: AgGridFilter.CreateFilterModel(
                            fieldName: nameof(OlympicWinner.Age),
                            filterModelType: FilterModelType.lessThanOrEqual,
                            filterModelFilterType: FilterModelFilterType.number,
                            filter: "52")[nameof(OlympicWinner.Age)]
                        
                        )).ToDictionary(e => e.Key,e=> e.Value),


                }

            };
            return result;
        }




    }
}


