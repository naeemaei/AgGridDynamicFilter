using System.Text.Json.Serialization;

namespace AgGridDynamicFilter.SampleData.Models
{
    public class OlympicWinner
    {
        public const string JsonUrl = "https://www.ag-grid.com/example-assets/olympic-winners.json";

        [JsonPropertyName("athlete")]
        public string Athlete { get; set; }

        [JsonPropertyName("age")]

        public int? Age { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("sport")]
        public string Sport { get; set; }

        [JsonPropertyName("gold")]
        public int Gold { get; set; }

        [JsonPropertyName("silver")]
        public int Silver { get; set; }

        [JsonPropertyName("bronze")]
        public int Bronze { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}
