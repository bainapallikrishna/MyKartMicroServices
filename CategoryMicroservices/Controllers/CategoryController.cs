using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CategoryMicroservices.Models;
using CategoryMicroservices.Repository;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CategoryMicroservices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : Controller

    {
        CategoryRepository repository;
        public CategoryController(CategoryRepository categoryRepository)
        {
            repository = categoryRepository;
        }

        [HttpGet]
        public JsonResult GetAllCategoriesDetails()
        {
            List<Category> listOfCategories = repository.GetAllCategories();
            return Json(listOfCategories);
        }

        [HttpGet("{id}")]
        public JsonResult GetCategoryById(byte id)
        {
            List<Category> listOfCategories = repository.GetAllCategories();
            Category category = listOfCategories.Find(c => c.CategoryId == id);
            return Json(category);
        }

        [HttpPost]
        public JsonResult AddNewCategory(Category category)
        {
            return Json(repository.AddNewCategory(category));
        }

        [HttpPut]
        public JsonResult UpdateCategory(Category category)
        {
            int result = repository.UpdateCategoryDetails(category);
            return Json(result);
        }

        [HttpDelete]
        public JsonResult DeleteCategory(byte categoryId)
        {
            bool result = repository.DeleteCategory(categoryId);
            return Json(result);
        }

    }
}
