using Microsoft.EntityFrameworkCore;
using ProductMicroservices.Models;

namespace ProductMicroservices.Repository
{
    public class ProductRepository
    {
        public readonly ProductDBContext _context;
        public ProductRepository(ProductDBContext context)
        {
            _context = context;
        }
        public List<Product> GetAllProducts()
        {
            List<Product> listOfProducts = _context.Products.AsNoTracking().ToList();
            return listOfProducts;
        }
        public bool AddNewProduct(Product product)
        {
            bool status = false;
            try
            {
                _context.Products.Add(product);
                _context.SaveChanges();
                status = true;
            }
            catch (Exception)
            {
                status = false;
            }
            return status;
        }

        public int UpdateProductDetails(Product product)
        {
            int status = -1;
            Product productObj = _context.Products.Find(product.ProductId);
            try
            {
                if (productObj != null)
                {
                    productObj.ProductName = product.ProductName;
                    productObj.CategoryId = product.CategoryId;
                    productObj.Price = product.Price;
                    productObj.QuantityAvailable = product.QuantityAvailable;
                    _context.Products.Update(productObj);
                    _context.SaveChanges();
                    status = 1;
                }

            }
            catch (Exception)
            {
                status = -99;
            }
            return status;
        }
        public bool DeleteProduct(string productId)
        {
            bool status = false;
            Product product = _context.Products.Find(productId);
            try
            {
                if (product != null)
                {
                    _context.Products.Remove(product);
                    _context.SaveChanges();
                    status = true;
                }
            }
            catch (Exception)
            {
                status = false;
            }
            return status;
        }
        public Product GetProductById(string productId)
        {
            Product product = _context.Products.Find(productId);
            return product;
        }
        public async Task<decimal> GetPrice(string productId)
        {
            decimal price = 0;
            try
            {
                var product = await _context.Products
                    .Where(p => p.ProductId == productId)
                    .FirstOrDefaultAsync();
                if (product != null)
                {
                    price = product.Price;
                }
            }
            catch (Exception)
            {
                price = -99;
            }
            return price;
        }
        public async Task<int> UpdateQuantity(string productId, int quantityPurchased)
        {
            int result = 0;
            try
            {
                Product product = await _context.Products
                    .Where(p => p.ProductId == productId)
                    .FirstOrDefaultAsync();
                if (product != null)
                {
                    if (product.QuantityAvailable >= quantityPurchased)
                    {
                        product.QuantityAvailable -= quantityPurchased;
                        _context.Products.Update(product);
                        var rowsAffected = await _context.SaveChangesAsync();
                        if (rowsAffected > 0)
                        {
                            // On successful update of QuantityAvailable
                            result = 1;
                        }
                        else
                        {
                            // On unsuccessful update of QuantityAvailable
                            result = -1;
                        }
                    }
                    else
                    {
                        // If QuantityAvailable less than the quantity to be purchased
                        result = -2;
                    }
                }
                else
                {
                    // If the product to update is not found 
                    result = -3;
                }
            }
            catch (Exception)
            {
                // On any exception raised while connecting to database or updating the database
                result = -99;
            }
            return result;
        }

    }
}
