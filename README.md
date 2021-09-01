# AgGridDynamicFilter

##Usage

```
public static IEnumerable<OlympicWinner> GetAndFilterAndSort(AgGridPaginationFilterModel filter) // Filter fill in frontend
{
    var expression = AgGridDynamicFilter.Extensions.GetExpression<OlympicWinner, AgGridPaginationFilterModel>(filter);

    return olympicWinners.Where(expression).DynamicOrderBy(filter);
}

```
