using MongoDB.Bson;
using MongoDB.Driver;
using SmartKioskBot.Controllers;
using SmartKioskBot.Helpers;
using SmartKioskBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static SmartKioskBot.Models.Context;
using static SmartKioskBot.Models.Customer;

namespace SmartKioskBot.Logic
{
    public abstract class RecommendationsLogic
    {
        public const int MAX_N_FILTERS_POPULAR = 5;
        public static Filter DEFAULT_RECOMMENDATION = new Filter()
        {
            FilterName = FilterLogic.brand_filter,
            Operator = "=",
            Value = "asus"
        };

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

        public static List<Filter> GetPopularFilters(List<FilterCount> counts)
        {
            List<Filter> popular = new List<Filter>();

            counts.Sort(
                delegate (FilterCount l1, FilterCount l2)
                {
                    return l1.NSearches.CompareTo(l2.NSearches);
                }
            );

            for (int i = 0; i < MAX_N_FILTERS_POPULAR && i < counts.Count; i++)
            {
                popular.Add(counts[i].Filter);
            }

            if (popular.Count == 0)
                popular.Add(DEFAULT_RECOMMENDATION);

            return popular;
        }
}
}