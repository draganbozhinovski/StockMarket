namespace StockMarket.Common
{
    public class Order
    {
        public Guid Id { get; set; }
        public string? User { get; set; }
        public string? Stock { get; set; }
        public double? Number { get; set; }
        public double? Bid { get; set; }
    }
}
