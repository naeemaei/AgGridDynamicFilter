using System;
using System.Collections.Generic;

namespace AgGridDynamicFilter.Models
{
    public class FilterModel
    {
        public FilterModel()
        {

        }

        public FilterModel Condition1 { get; set; }
        public FilterModel Condition2 { get; set; }
        public string Operator { get; set; } // AND, OR
        public string Type { get; set; } // contains notContains equals notEqual startsWith lessThan lessThanOrEqual greaterThan greaterThanOrEqual inRange endsWith
        public string Filter { get; set; }
        public string FilterTo { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string FilterType { get; set; } // text number set
        public IEnumerable<string> Values { get; set; } = new List<string>(); // set list

    }
}
