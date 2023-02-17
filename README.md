# AgGridDynamicFilter

## Dynamic filter from AgGrid to ASP.NET Core web api without any extra codes
Just need to pass DB model class and filter to generate expression function then pass expression to linq `Where` function

## Usage

```
public static IEnumerable<OlympicWinner> GetAndFilterAndSort(AgGridPaginationFilterModel filter) // Filter fill in frontend
{
    var expression = AgGridDynamicFilter.Extensions.GetExpression<OlympicWinner, AgGridPaginationFilterModel>(filter);

    return olympicWinners.Where(expression).DynamicOrderBy(filter);
}

```
