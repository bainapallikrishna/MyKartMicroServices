
using System.Text.Json.Serialization;

namespace ProductMicroservices.Models
{
    public class Product
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public byte CategoryId { get; set; }
        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }
        [JsonIgnore]
        public virtual Category Category { get; set; }
    }
}
