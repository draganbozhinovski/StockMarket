using Newtonsoft.Json;

namespace StockMarket.Common
{
    public class Data
    {
        [JsonProperty("base")]
        public string Base { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }
    }

    public class PriceUpdate
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }
}
