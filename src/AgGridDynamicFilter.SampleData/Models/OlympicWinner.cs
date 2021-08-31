namespace AgGridDynamicFilter.SampleData.Models
{
    public class OlympicWinner
    {
        public const string JsonUrl = "https://www.ag-grid.com/example-assets/olympic-winners.json";
        public string Athlete { get; set; }
        public int? Age { get; set; }
        public string Country { get; set; }
        public int Year { get; set; }
        public string Date { get; set; }
        public string Sport { get; set; }
        public int Gold { get; set; }
        public int Silver { get; set; }
        public int Bronze { get; set; }
        public int Total { get; set; }
    }
}
