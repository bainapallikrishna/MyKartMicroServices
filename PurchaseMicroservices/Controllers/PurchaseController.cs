using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyKart.Contracts.Product;
using PurchaseMicroservices.Models;
using PurchaseMicroservices.Repository;

namespace PurchaseMicroservices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseController : ControllerBase
    {
        public readonly PurchaseRepository _purchaseRepository;
        private readonly HttpClient _httpClient;
        private readonly string? _productServiceUrl;
        private readonly ProductGrpc.ProductGrpcClient _productGrpcClient;

        public PurchaseController(PurchaseRepository purchaseRepository,
             PurchaseDBContext context,
             HttpClient httpClient,
             IConfiguration configuration,
             ProductGrpc.ProductGrpcClient productGrpcClient)
        {
            _purchaseRepository = purchaseRepository;
            _httpClient = httpClient;
            _productServiceUrl = configuration.GetValue<string>("ProductServiceUrl");
            _productGrpcClient = productGrpcClient;
        }
        [HttpGet]
        public IActionResult GetAllProducts()
        {
            var listOfPurchase = _purchaseRepository.GetAllProducts();
            return Ok(listOfPurchase);
        }

        [HttpPost("product")]
        public IActionResult AddNewProduct(Purchase purchase)
        {
            bool status = _purchaseRepository.AddNewProduct(purchase);
            if (status)
            {
                return Ok("Product added successfully");
            }
            else
            {
                return BadRequest("Failed to add product");
            }
        }
        [HttpPut("product")]
        public IActionResult UpdateProductDetails(Purchase purchase)
        {
            int status = _purchaseRepository.UpdateProductDetails(purchase);
            if (status == 1)
            {
                return Ok("Product details updated successfully");
            }
            else if (status == -1)
            {
                return NotFound("Product not found");
            }
            else
            {
                return BadRequest("Failed to update product details");
            }
        }
        [HttpDelete("product")]
        public IActionResult DeleteProduct(string PurchaseId)
        {
            bool status = _purchaseRepository.DeleteProduct(PurchaseId);
            if (status)
            {
                return Ok("Product deleted successfully");
            }
            else
            {
                return NotFound("Product not found");
            }
        }
        [HttpPost("purchaseProduct")]
        public async Task<JsonResult> AddPurchase(Purchase purchase)
        {
            string result = "";
            bool? status = false;
            try
            {
                double priceOfProduct = 0;
                var priceReply = await _productGrpcClient.GetPriceAsync(new GetPriceRequest { ProductId = purchase.ProductId });
                priceOfProduct = priceReply.Price;
                if (priceOfProduct > 0)
                {
                    var updateReply = await _productGrpcClient.UpdateQuantityAsync(new UpdateQuantityRequest
                    {
                        ProductId = purchase.ProductId,
                        QuantityPurchased = purchase.QuantityPurchased
                    });

                    if (updateReply.Result == 1)
                    {
                        purchase.TotalPrice = (decimal)(purchase.QuantityPurchased * priceOfProduct);
                        status = await _purchaseRepository.AddPurchaseDetails(purchase);
                        if (status == true)
                        {
                            result = "Successfully added purchase details!";
                        }
                        else if (status == false)
                        {
                            result = "Purchase details could not be added!";
                        }
                        else
                        {
                            result = "Some error occurred while storing purchase details!";
                        }
                    }
                    else
                    {
                        result = "Some error occurred while updating the stock!";
                    }
                }
                else
                {
                    result = "Failed to fetch the price of the product";
                }
            }
            catch (Exception ex)
            {
                result = "Exception!";
            }
            return new JsonResult(result);
        }

    }
}
