using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuickKartClientApp.Models;
using System.Net.Http;
using System.Text.Json;

namespace QuickKartClientApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductClientController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly JsonSerializerOptions jsonSerializerOptions;

        public ProductClientController(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;

            jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
        }

        [HttpGet]
        public JsonResult FetchAllProductDetails()
        {
            var httpClient = httpClientFactory.CreateClient("apiGatewayServices");
          
            List<Product> listOfProducts = new List<Product>();

            Task<HttpResponseMessage> httpResponseMessageTask = httpClient.GetAsync("/apiGateway/GatewayForProduct");       
            httpResponseMessageTask.Wait();

            HttpResponseMessage httpResponseMessage = httpResponseMessageTask.Result;

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                HttpContent httpContent = httpResponseMessage.Content;

                Task<string> httpContentTask = httpContent.ReadAsStringAsync();
                httpContentTask.Wait();
                string serializedData = httpContentTask.Result;

                listOfProducts = JsonSerializer
                    .Deserialize<List<Product>>(serializedData, jsonSerializerOptions);
            }
            else
            {
                listOfProducts = null;
            }

            return Json(listOfProducts);
        }
     
        [HttpDelete("{id}")]
        public JsonResult DeleteAllProduct(string id) {
            var httpClient = httpClientFactory.CreateClient("apiGatewayServices");
            bool result = false;
            Task<HttpResponseMessage> httpResponseMessageTask = httpClient.DeleteAsync($"/apiGateway/GatewayForProduct?id={id}");
            httpResponseMessageTask.Wait();
            HttpResponseMessage httpResponseMessage = httpResponseMessageTask.Result;
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                result = true;
            }
            else
            {
                result = false;
            }
            return Json(result);
        }
        [HttpGet("{id}")]
        public JsonResult GetProductDetails(string id)
        {
            var httpClient = httpClientFactory.CreateClient("apiGatewayServices");
            Product product = null;

            Task<HttpResponseMessage> httpResponseMessageTask =
                httpClient.GetAsync($"/apiGateway/GatewayForProduct?id={id}");

            httpResponseMessageTask.Wait();
            HttpResponseMessage httpResponseMessage = httpResponseMessageTask.Result;

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                HttpContent httpContent = httpResponseMessage.Content;

                Task<string> httpContentTask = httpContent.ReadAsStringAsync();
                httpContentTask.Wait();

                string serializedData = httpContentTask.Result;

                var products = JsonSerializer.Deserialize<List<Product>>(
                                   serializedData,
                                   jsonSerializerOptions);

                product = products?.FirstOrDefault();
            }

            return Json(product);
        }

    }
}

