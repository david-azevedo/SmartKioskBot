using MongoDB.Bson;
using MongoDB.Driver;
using SmartKioskBot.Helpers;
using SmartKioskBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartKioskBot.Controllers
{
    public abstract class ProductController
    {
        public static Product getProduct(string id)
        {
            var productCollection = DbSingleton.GetDatabase().GetCollection<Product>(AppSettings.ProductsCollection);
            var filter = MongoDB.Driver.Builders<Product>.Filter.Eq(p => p.Id, ObjectId.Parse(id));

            List<Product> context = productCollection.Find(filter).ToList();

            if (context.Count() == 0)
                return null;
            else
                return context[0];
        }

        public static List<Product> getProducts(ObjectId[] productsIds)
        {
            var productCollection = DbSingleton.GetDatabase().GetCollection<Product>(AppSettings.ProductsCollection);
            var filter = MongoDB.Driver.Builders<Product>.Filter.In(p => p.Id, productsIds);

           return productCollection.Find(filter).ToList();
        }

        public static List<Product> getProductsFilter(FilterDefinition<Product> filter, int limit, ObjectId last_id_fetch)
        {
            if(last_id_fetch != null)
                filter &= Builders<Product>.Filter.Gt(p => p.Id, last_id_fetch);

            var productCollection = DbSingleton.GetDatabase().GetCollection<Product>(AppSettings.ProductsCollection);
            return productCollection.Find(filter)
                .SortBy(p => p.Id)
                .Limit(limit).ToList();
        }
    }
}