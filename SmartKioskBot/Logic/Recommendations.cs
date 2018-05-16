using MongoDB.Bson;
using MongoDB.Driver;
using SmartKioskBot.Controllers;
using SmartKioskBot.Helpers;
using SmartKioskBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartKioskBot.Logic
{
    public class Recommendations
    {
        public static List<Product> GetSimilarProducts(ObjectId productId)
        {
            const double PRICE_RANGE = 50;

            List<Product> similarProducts = new List<Product>();

            Product p = ProductController.getProduct(productId.ToString());

            var collection = DbSingleton.GetDatabase().GetCollection<Product>(AppSettings.ProductsCollection);
            
            var filter = Builders<Product>.Filter.And(
               Builders<Product>.Filter.Gte(o => o.Price, p.Price - PRICE_RANGE),
               Builders<Product>.Filter.Lte(o => o.Price, p.Price + PRICE_RANGE),
               Builders<Product>.Filter.Eq(o => o.ScreenDiagonal, p.ScreenDiagonal));

            similarProducts = collection.Find(filter).ToList();

            return similarProducts;
        }
    }
}