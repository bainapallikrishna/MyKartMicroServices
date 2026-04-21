namespace PurchaseMicroservices.Models
{
    public class Purchase
    {
        public int  PurchaseId { get; set; }
        public string EmailId { get; set; }
        public string ProductId { get; set; }
        public int QuantityPurchased { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
