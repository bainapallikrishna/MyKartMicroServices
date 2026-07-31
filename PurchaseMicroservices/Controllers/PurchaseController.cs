using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using PurchaseMicroservices.Models;
using PurchaseMicroservices.Repository;
using SharedLibrary.Common;

namespace PurchaseMicroservices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class PurchaseController : ControllerBase
    {
        public readonly PurchaseRepository _purchaseRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string? _productServiceUrl;

        public PurchaseController(PurchaseRepository purchaseRepository,
             PurchaseDBContext context,
             IHttpClientFactory httpClientFactory,
             IConfiguration configuration)
        {
            _purchaseRepository = purchaseRepository;
            _httpClientFactory = httpClientFactory;
            _productServiceUrl = configuration.GetValue<string>("ProductServiceUrl");
        }
        [HttpGet]
        [Cacheable(durationInSeconds: 300)]
        public IActionResult GetAllProducts()
        {
            var listOfPurchase = _purchaseRepository.GetAllProducts();
            return Ok(listOfPurchase);
        }

        [HttpPost("product")]
        [InvalidateCache("purchase:*")]
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
        [InvalidateCache("purchase:*")]
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
        [InvalidateCache("purchase:*")]
        public IActionResult DeleteProduct(int PurchaseId)
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
        [InvalidateCache("purchase:*", "product:*")]
        public async Task<JsonResult> AddPurchase(Purchase purchase)
        {
            string result = "";
            bool? status = false;
            try
            {
                double priceOfProduct = 0;
                var client = _httpClientFactory.CreateClient("PropagatingClient");

                // Get price from Product service REST endpoint
                var priceResponse = await client.GetAsync($"{_productServiceUrl}/api/Product/Price?productId={purchase.ProductId}");
                if (!priceResponse.IsSuccessStatusCode)
                {
                    result = "Failed to fetch the price of the product";
                    return new JsonResult(result);
                }

                priceOfProduct = await priceResponse.Content.ReadFromJsonAsync<double>();
                if (priceOfProduct > 0)
                {
                    // Update quantity via Product service REST endpoint
                    var updateResponse = await client.PutAsync($"{_productServiceUrl}/api/Product/Quantity?productId={purchase.ProductId}&quantityPurchased={purchase.QuantityPurchased}", null);
                    if (updateResponse.IsSuccessStatusCode)
                    {
                        var updateResult = await updateResponse.Content.ReadFromJsonAsync<int>();
                        if (updateResult == 1)
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
