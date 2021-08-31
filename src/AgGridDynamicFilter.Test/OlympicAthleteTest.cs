using AgGridDynamicFilter.Enums;
using AgGridDynamicFilter.Extensions;
using AgGridDynamicFilter.Models;
using AgGridDynamicFilter.SampleData;
using AgGridDynamicFilter.SampleData.Models;
using AgGridDynamicFilter.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AgGridDynamicFilter.Test
{
    public partial class OlympicAthleteTest
    {
        private static IEnumerable<OlympicWinner> olympicWinners;

        
        public OlympicAthleteTest()
        {
            //Task.Run(()=> DataLoader.GetOlympicWinnersRemote()).Wait();
            olympicWinners = DataLoader.GetOlympicWinnersFromFile();
        }

        public static IEnumerable<OlympicWinner> GetAndFilterAndSort(AgGridPaginationFilterModel filter)
        {
            var expression = Extensions.Extensions.GetExpression<OlympicWinner, AgGridPaginationFilterModel>(filter);

            return olympicWinners.Where(expression.Compile()).DynamicOrderBy(filter);
        }

        [Theory]
        [MemberData(nameof(AgeFilterAgeSort))]
        public void OlympicWinnersApi_GivenGetList_WhenFilterByAge_SortByAge_ThenResultBW18And26SortByAge(int startRow, int endRow, List<SortModel> sortModel, Dictionary<string, FilterModel> filterModel)
        {
            var filter = new AgGridPaginationFilterModel
            {
                StartRow = startRow,
                EndRow = endRow,
                FilterModel = filterModel,
                SortModel = sortModel,
            };

            var filteredWinners = GetAndFilterAndSort(filter);

            Assert.True(filteredWinners.Max(e => e.Age) <= 26 && filteredWinners.Min(e => e.Age) >= 18);
            Assert.DoesNotContain(filteredWinners
                           .Select((item, index) => new
                           {
                               item.Athlete,
                               item.Country,
                               PrevCountry = index > 0 ? filteredWinners.ElementAt(index - 1).Country : item.Country
                           }), e => string.Compare(e.Country, e.PrevCountry, StringComparison.InvariantCultureIgnoreCase) > 0);

        }


        [Theory]
        [MemberData(nameof(AgeAndCountryFilterNameSort))]
        public void OlympicWinnersApi_GivenGetList_WhenFilterByAgeAndCountry_SortByCountry_ThenResultFromIranAndIraqAndSortByName(int startRow, int endRow, List<SortModel> sortModel, Dictionary<string, FilterModel> filterModel)
        {
            var filter = new AgGridPaginationFilterModel
            {
                StartRow = startRow,
                EndRow = endRow,
                FilterModel = filterModel,
                SortModel = sortModel,
            };

            var filteredWinners = GetAndFilterAndSort(filter);

            Assert.True(filteredWinners.Max(e => e.Age) <= 52 && filteredWinners.Min(e => e.Age) >= 23);
            Assert.DoesNotContain(filteredWinners, e => e.Country != "Iran" && e.Country != "Iraq");

        }
    }
}
