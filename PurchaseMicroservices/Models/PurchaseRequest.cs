using System.ComponentModel.DataAnnotations;

namespace PurchaseMicroservices.Models
{
    public class PurchaseRequest
    {
        [Required]
        public string EmailId { get; set; }
        [Required]
        public string ProductId { get; set; }
        [Required]
        [Range(minimum: 1, maximum: int.MaxValue)]
        public int QuantityPurchased { get; set; }
    }
}
