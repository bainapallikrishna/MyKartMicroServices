using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly string _productServiceUrl;

        public PurchaseController(PurchaseRepository purchaseRepository,
             PurchaseDBContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _purchaseRepository = purchaseRepository;
            _httpClient = httpClient;
            _productServiceUrl = configuration.GetValue<string>("ProductServiceUrl");
        }
        [HttpGet]
        public IActionResult GetAllProducts()
        {
            var listOfPurchase = _purchaseRepository.GetAllProducts();
            return Ok(listOfPurchase);
        }

        [HttpPost]
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
        [HttpPost]
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
        [HttpDelete]
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
        [HttpPost]
        public async Task<JsonResult> AddPurchase(Purchase purchase)
        {
            string result = "";
            bool? status = false;
            try
            {
                double priceOfProduct = 0;
                HttpResponseMessage priceResponse = await _httpClient
                    .GetAsync($"{_productServiceUrl}/api/Product/GetPrice?productId={purchase.ProductId}");
                if (priceResponse.IsSuccessStatusCode)
                {
                    priceOfProduct = Convert.ToDouble(await priceResponse.Content.ReadAsStringAsync());
                }
                if (priceOfProduct > 0)
                {
                    HttpResponseMessage updateQuantitytResponse = await
                        _httpClient.PutAsJsonAsync($"{_productServiceUrl}/api/Product/UpdateQuantity" +
                        $"?productId={purchase.ProductId}&quantityPurchased={purchase.QuantityPurchased}",
                        new { });
                    if (updateQuantitytResponse.IsSuccessStatusCode)
                    {
                        if (Convert.ToInt32(await updateQuantitytResponse.Content.ReadAsStringAsync()) == 1)
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
                        result = "Internal Server error while updating the stock!";
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
