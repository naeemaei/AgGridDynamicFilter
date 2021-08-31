namespace AgGridDynamicFilter.Models
{
    public class AgGridPaginationFilterModel : AgGridFilterPayloadModel
    {
        public int PageNumber
        {
            get
            {
                return (StartRow + PageSize) / PageSize;
            }
        }
        public int PageSize
        {
            get { return EndRow - StartRow; }
        }
    }
}
