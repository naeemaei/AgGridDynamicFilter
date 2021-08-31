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

        public static async Task<IEnumerable<OlympicWinner>> GetAndFilterAndSort(AgGridPaginationFilterModel filter)
        {
            var allWinners = await DataLoader.GetOlympicWinners();

            var expression = Extensions.Extensions.GetExpression<OlympicWinner, AgGridPaginationFilterModel>(filter);

            return allWinners.Where(expression.Compile()).DynamicOrderBy(filter);
        }

        [Theory]
        [MemberData(nameof(AgeFilterAgeSort))]
        public async Task OlympicWinnersApi_GivenGetList_WhenFilterByAge_SortByAge_ThenResultBW18And26SortByAge(int startRow, int endRow, List<SortModel> sortModel, Dictionary<string, FilterModel> filterModel)
        {
            var filter = new AgGridPaginationFilterModel
            {
                StartRow = startRow,
                EndRow = endRow,
                FilterModel = filterModel,
                SortModel = sortModel,
            };

            var filteredWinners = await GetAndFilterAndSort(filter);

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
        public async Task OlympicWinnersApi_GivenGetList_WhenFilterByAgeAndCountry_SortByCountry_ThenResultFromIranAndIraqAndSortByName(int startRow, int endRow, List<SortModel> sortModel, Dictionary<string, FilterModel> filterModel)
        {
            var filter = new AgGridPaginationFilterModel
            {
                StartRow = startRow,
                EndRow = endRow,
                FilterModel = filterModel,
                SortModel = sortModel,
            };
 
            var filteredWinners = await GetAndFilterAndSort(filter);

            Assert.True(filteredWinners.Max(e => e.Age) <= 52 && filteredWinners.Min(e => e.Age) >= 23);
            Assert.DoesNotContain(filteredWinners, e => e.Country != "Iran" && e.Country != "Iraq");

        }
    }
}
