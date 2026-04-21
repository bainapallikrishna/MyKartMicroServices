using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace QuickKartClientApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryClientController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        JsonSerializerOptions jsonSerializerOptions;
        public CategoryClientController(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
            jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
        }
        [HttpGet]
        public IActionResult GetCategory()
        {
                var httpClient = httpClientFactory.CreateClient("apiGatewayServices");
               
            List<Models.Category> listOfCategories = new List<Models.Category>();
            Task<HttpResponseMessage> httpResponseMessageTask = httpClient.GetAsync("/apiGateway/GatewayForCategory");
            httpResponseMessageTask.Wait();
            HttpResponseMessage httpResponseMessage = httpResponseMessageTask.Result;
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                HttpContent httpContent = httpResponseMessage.Content;
                Task<string> httpContentTask = httpContent.ReadAsStringAsync();
                httpContentTask.Wait();
                string serializedData = httpContentTask.Result;
                listOfCategories = JsonSerializer.Deserialize<List<Models.Category>>(serializedData, jsonSerializerOptions);
            }
            else
            {
                listOfCategories = null;
            }
            return new JsonResult(listOfCategories);
        }
    }
}
