using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CategoryMicroservices.Models;
using CategoryMicroservices.Repository;
using SharedLibrary.Common;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CategoryMicroservices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class CategoryController : Controller

    {
        CategoryRepository repository;
        public CategoryController(CategoryRepository categoryRepository)
        {
            repository = categoryRepository;
        }

        [HttpGet]
        [Cacheable(durationInSeconds: 300)]
        public JsonResult GetAllCategoriesDetails()
        {
            List<Category> listOfCategories = repository.GetAllCategories();
            return Json(listOfCategories);
        }

        [HttpGet("{id}")]
        [Cacheable(durationInSeconds: 300)]
        public JsonResult GetCategoryById(byte id)
        {
            List<Category> listOfCategories = repository.GetAllCategories();
            Category category = listOfCategories.Find(c => c.CategoryId == id);
            return Json(category);
        }

        [HttpPost]
        [InvalidateCache("category:*")]
        public JsonResult AddNewCategory(Category category)
        {
            return Json(repository.AddNewCategory(category));
        }

        [HttpPut]
        [InvalidateCache("category:*")]
        public JsonResult UpdateCategory(Category category)
        {
            int result = repository.UpdateCategoryDetails(category);
            return Json(result);
        }

        [HttpDelete]
        [InvalidateCache("category:*")]
        public JsonResult DeleteCategory(byte categoryId)
        {
            bool result = repository.DeleteCategory(categoryId);
            return Json(result);
        }

    }
}
