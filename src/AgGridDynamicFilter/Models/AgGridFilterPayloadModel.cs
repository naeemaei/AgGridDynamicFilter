using System.Collections.Generic;

namespace AgGridDynamicFilter.Models
{
    public class AgGridFilterPayloadModel
    {
        public AgGridFilterPayloadModel()
        {

        }

        public int StartRow { get; set; }
        public int EndRow { get; set; }
        public List<SortModel> SortModel { get; set; } = new List<SortModel>();
        public Dictionary<string, FilterModel> FilterModel { get; set; } = new Dictionary<string, FilterModel>();
    }
}
