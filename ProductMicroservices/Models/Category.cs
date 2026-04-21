namespace ProductMicroservices.Models
{
    public class Category
    {

        public byte CategoryId { get; set; }
        public string CategoryName { get; set; }

        // collection navigation; virtual for lazy-loading
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
