using AgGridDynamicFilter.SampleData.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AgGridDynamicFilter.SampleData
{
    public static class DataLoader
    {
        private static HttpClient httpClient { get; set; }
        static DataLoader()
        {
            httpClient = new HttpClient();
        }

        public static async Task<IEnumerable<OlympicWinner>> GetOlympicWinners()
        {
            return await httpClient.GetFromJsonAsync<IEnumerable<OlympicWinner>>(OlympicWinner.JsonUrl, default);
        }
    }
}
