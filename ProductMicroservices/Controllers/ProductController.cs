using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ProductMicroservices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : Controller
    {
        public readonly Repository.ProductRepository _productRepository;
        public ProductController(Repository.ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        [HttpGet]
        public async Task<JsonResult> GetAllProducts()
        {
            List<Models.Product> listOfProducts = _productRepository.GetAllProducts();
            await Task.Delay(18000);

            return Json(listOfProducts);
        }
        [HttpGet("{id}")]
        public JsonResult GetProductById(string id)
        {
            Models.Product product = _productRepository.GetProductById(id);
            if (product != null)
            {
                return Json(product);
            }
            else
            {
                return Json("Product not found");
            }
        }
        [HttpPost]
        public JsonResult AddNewProduct(Models.Product product)
        {
            bool status = _productRepository.AddNewProduct(product);
            if (status)
            {
                return Json("Product Added Successfully");
            }
            else
            {
                return Json("Failed to add the product");
            }
        }
        [HttpPut]
        public JsonResult UpdateProductDetails(Models.Product product)
        {
            int status = _productRepository.UpdateProductDetails(product);
            if (status == 1)
            {
                return Json("Product Updated Successfully");
            }
            else if (status == -1)
            {
                return Json("Product not found");
            }
            else
            {
                return Json("Failed to update the product");
            }
        }
        [HttpDelete]
        public JsonResult DeleteProduct(string id)
        {
            bool status = _productRepository.DeleteProduct(id);
            if (status)
            {
                return Json("Product Deleted Successfully");
            }
            else
            {
                return Json("Product not found");
            }
        }
        [HttpGet("Price")]
        public async Task<JsonResult> GetPrice(string productId)
        {
            decimal result = await _productRepository.GetPrice(productId);
            return Json(result);
        }
        [HttpPut("Quantity")]
        public async Task<JsonResult> UpdateQuantity(string productId, int quantityPurchased)
        {
            int result = await _productRepository.UpdateQuantity(productId, quantityPurchased);
            return Json(result);
        }


    }

}
