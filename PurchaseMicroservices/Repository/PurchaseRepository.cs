using Microsoft.EntityFrameworkCore;
using PurchaseMicroservices.Models;

namespace PurchaseMicroservices.Repository
{
    public class PurchaseRepository
    {
        public readonly PurchaseDBContext _context;
        public PurchaseRepository(PurchaseDBContext context) 
        {
            _context = context;
        }
        public List<Purchase> GetAllProducts()
        {
            List<Purchase> listOfPurchase = _context.Purchases.ToList();
            return listOfPurchase;
        }
        public bool AddNewProduct(Purchase purchase)
        {
            bool status = false;
            try
            {
                _context.Purchases.Add(purchase);
                _context.SaveChanges();
                status = true;
            }
            catch (Exception)
            {
                status = false;
            }
            return status;
        }

        public int UpdateProductDetails(Purchase purchase)
        {
            int status = -1;
            Purchase purchaseObj = _context.Purchases.Find(purchase);

            try
            {
                if (purchaseObj != null)
                {
                    purchaseObj.PurchaseId = purchase.PurchaseId;
                    purchaseObj.EmailId = purchase.EmailId;
                    purchaseObj.PurchaseId = purchase.PurchaseId;
                    purchaseObj.QuantityPurchased = purchase.QuantityPurchased;
                    purchaseObj.TotalPrice = purchase.TotalPrice;
                    _context.Purchases.Update(purchaseObj);
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
        public bool DeleteProduct(int PurchaseId)
        {
            bool status = false;
            Purchase purchase = _context.Purchases.Find(PurchaseId);
            try
            {
                if (purchase != null)
                {
                    _context.Purchases.Remove(purchase);
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
        public async Task<bool?> AddPurchaseDetails(Purchase purchase)
        {
            bool? result = false;
            try
            {
                _context.Purchases.Add(purchase);
                int rowsAffected = await _context.SaveChangesAsync();
                if (rowsAffected > 0)
                {
                    result = true;
                }
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }

    }
}
