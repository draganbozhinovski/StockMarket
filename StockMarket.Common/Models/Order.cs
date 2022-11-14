namespace StockMarket.Common.Models
{
    [Serializable]
    public class Order
    {
        public Guid Id { get; set; }
        public User User { get; set; }
        public Currency Currency { get; set; }
        public double NumberOf { get; set; } = 1;
        public double Bid { get; set; }
    }

    [Serializable]
    public class OrderInProcess : Order
    {
        public double CurrentAmmount { get; set; }
    }
}
