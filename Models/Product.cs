namespace MovieProductRecommenderWeb.Models
{
    public class Product
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Brand { get; set; }
        public int Year { get; set; }
        public double Rating { get; set; }
    }
}
