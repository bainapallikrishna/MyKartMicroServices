using CategoryMicroservices.Models;
namespace CategoryMicroservices.Repository
{
    public class CategoryRepository
    {
        CategoryDBContext dbContext;
        public CategoryRepository(CategoryDBContext context)
        {
            dbContext = context;
        }

        public List<Category> GetAllCategories()
        {
            List<Category> listOfCategories = dbContext.Categories.ToList();
            return listOfCategories;
        }
        public bool AddNewCategory(Category category)
        {
            bool status = false;
            try
            {
                dbContext.Categories.Add(category);
                dbContext.SaveChanges();
                status = true;
            }
            catch (Exception)
            {
                status = false;
            }
            return status;
        }
        public int UpdateCategoryDetails(Category category)
        {
            int status = -1;
            Category categoryObj = dbContext.Categories.Find(category.CategoryId);
            try
            {
                if (categoryObj != null)
                {
                    categoryObj.CategoryId = category.CategoryId;
                    categoryObj.CategoryName = category.CategoryName;
                    dbContext.Categories.Update(categoryObj);
                    dbContext.SaveChanges();
                    status = 1;
                }
            }
            catch (Exception)
            {
                status = -99;
            }
            return status;
        }
        public bool DeleteCategory(byte categoryId)
        {
            bool status = false;
            Category category = dbContext.Categories.Find(categoryId);

            try
            {
                if (category != null)
                {
                    dbContext.Categories.Remove(category);
                    dbContext.SaveChanges();
                    status = true;
                }
            }
            catch (Exception)
            {
                status = false;
            }
            return status;
        }

    }
}
