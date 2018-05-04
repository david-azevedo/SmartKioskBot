using MongoDB.Bson;
using MongoDB.Driver;
using SmartKioskBot.Helpers;
using SmartKioskBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static SmartKioskBot.Models.Context;

namespace SmartKioskBot.Controllers
{
    public abstract class ContextController
    {
        /// <summary>
        /// Get a conversation context related to a user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static Context GetContext(ObjectId userId)
        {

            var contextCollection = DbSingleton.GetDatabase().GetCollection<Context>(AppSettings.ContextCollection);
            var filter = Builders<Context>.Filter.Eq(c => c.UserId, userId);

            List<Context> context = contextCollection.Find(filter).ToList();

            if (context.Count() == 0)
                return null;
            else
                return context[0];
        }

        /// <summary>
        /// Delete context
        /// </summary>
        /// <param name="userId"></param>
        public static void DeleteContext(Context context)
        {
            var contextCollection = DbSingleton.GetDatabase().GetCollection<Context>(AppSettings.ContextCollection);
            var filter = Builders<Context>.Filter.And(
                Builders<Context>.Filter.Eq(o=>o.Country,context.Country),
                Builders<Context>.Filter.Eq(o => o.Id, context.Id)
                );
            contextCollection.DeleteOne(filter);
        }
        /// <summary>
        /// Create a context related to a user.
        /// </summary>
        /// <param name="userId"></param>
        public static void CreateContext(User user)
        {
            var contextCollection = DbSingleton.GetDatabase().GetCollection<Context>(AppSettings.ContextCollection);

            Context c = new Context()
            {
                UserId = user.Id,
                Country = user.Country,
                Filters = new Filter[] { },
                WishList = new ObjectId[] { },
                Comparator = new ObjectId[] { }
            };
            contextCollection.InsertOne(c);
        }

        /// <summary>
        /// Adds a filter in the search of a product and saves it in the user context.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="filterName"></param>
        /// <param name="op"></param>
        /// <param name="value"></param>
        public static void AddFilter(User user, string filterName, string op, string value)
        {
            Filter f = new Filter()
            {
                FilterName = filterName,
                Operator = op,
                Value = value
            };

            var contextCollection = DbSingleton.GetDatabase().GetCollection<Context>(AppSettings.ContextCollection);

            var filter = Builders<Context>.Filter.And(
                Builders<Context>.Filter.Eq(o=>o.UserId,user.Id),           //same user id
                Builders<Context>.Filter.Eq(o=>o.Country,user.Country));    //same country (shard)
            var update = Builders<Context>.Update.Push(o => o.Filters, f);  //push new filter

            contextCollection.UpdateOne(filter, update);
        }

        /// <summary>
        /// Removes a filter frin the search of a product and saves it in the user context.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="filterName"></param>
        public static void RemFilter(User user, string filterName)
        {
            var contextCollection = DbSingleton.GetDatabase().GetCollection<Context>(AppSettings.ContextCollection);

            var filter = Builders<Context>.Filter.And(
                Builders<Context>.Filter.Eq(o => o.UserId, user.Id),           //same user id
                Builders<Context>.Filter.Eq(o => o.Country, user.Country));    //same country (shard)

            var tmp = contextCollection.Find(filter).ToList();

            //Context found
            if(tmp.Count != 0)
            {
                Filter[] filters = tmp[0].Filters;

                //remove filter of filters array
                var newFilters = filters.Where(val => val.FilterName != filterName).ToArray();

                var update = Builders<Context>.Update.Set(o => o.Filters, newFilters);
                contextCollection.UpdateOne(filter, update);
            }
        }

        /// <summary>
        /// Removes all the filters in the search of a product and saves is in the user context.
        /// </summary>
        /// <param name="user"></param>
        public static void CleanFilters(User user)
        {
            var contextCollection = DbSingleton.GetDatabase().GetCollection<Context>(AppSettings.ContextCollection);

            var filter = Builders<Context>.Filter.And(
                Builders<Context>.Filter.Eq(o => o.UserId, user.Id),           //same user id
                Builders<Context>.Filter.Eq(o => o.Country, user.Country));    //same country (shard)
            var update = Builders<Context>.Update.Set(o => o.Filters, new Filter[] { });

            contextCollection.UpdateOne(filter, update);
        }

        /// <summary>
        /// Adds a wish product and saves it in the user context.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="productId"></param>
        public static void AddWishList(User user, string productId)
        {
            var contextCollection = DbSingleton.GetDatabase().GetCollection<Context>(AppSettings.ContextCollection);

            var filter = Builders<Context>.Filter.And(
                Builders<Context>.Filter.Eq(o => o.UserId, user.Id),                                        //same user id
                Builders<Context>.Filter.Eq(o => o.Country, user.Country),                                  //same country (shard)
                Builders<Context>.Filter.Not(
                    Builders<Context>.Filter.AnyEq(o => o.WishList, ObjectId.Parse(productId))));  //don't contain the product already     
            var update = Builders<Context>.Update.Push(o => o.WishList, ObjectId.Parse(productId));     //push new wish 

            contextCollection.UpdateOne(filter, update);
        }

        /// <summary>
        /// Removes a wish product and updates it in the user context.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="productId"></param>
        public static void RemWishList(User user, string productId)
        {
            var contextCollection = DbSingleton.GetDatabase().GetCollection<Context>(AppSettings.ContextCollection);

            var filter = Builders<Context>.Filter.And(
                Builders<Context>.Filter.Eq(o => o.UserId, user.Id),           //same user id
                Builders<Context>.Filter.Eq(o => o.Country, user.Country));    //same country (shard)

            var update = Builders<Context>.Update.Pull(o => o.WishList, ObjectId.Parse(productId));
            contextCollection.UpdateOne(filter, update);
        }

        /// <summary>
        /// Gets all the product wishes from the user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static ObjectId[] getWishes(User user, string productId)
        {
            var contextCollection = DbSingleton.GetDatabase().GetCollection<Context>(AppSettings.ContextCollection);

            var filter = Builders<Context>.Filter.And(
                Builders<Context>.Filter.Eq(o => o.UserId, user.Id),           //same user id
                Builders<Context>.Filter.Eq(o => o.Country, user.Country));    //same country (shard)

            var tmp = contextCollection.Find(filter).ToList();

            if (tmp.Count != 0)
                return tmp[0].WishList;
            return new ObjectId[] { };
        }

        public static void AddComparator(User user, string productId)
        {
            var contextCollection = DbSingleton.GetDatabase().GetCollection<Context>(AppSettings.ContextCollection);

            var filter = Builders<Context>.Filter.And(
                Builders<Context>.Filter.Eq(o => o.UserId, user.Id),                                        //same user id
                Builders<Context>.Filter.Eq(o => o.Country, user.Country),                                  //same country (shard)
                Builders<Context>.Filter.Not(
                    Builders<Context>.Filter.AnyEq(o => o.Comparator, ObjectId.Parse(productId))));  //don't contain the product already     
            var update = Builders<Context>.Update.Push(o => o.Comparator, ObjectId.Parse(productId));     //push new wish 

            contextCollection.UpdateOne(filter, update);
        }

        public static void RemComparator(User user, string productId)
        {
            var contextCollection = DbSingleton.GetDatabase().GetCollection<Context>(AppSettings.ContextCollection);

            var filter = Builders<Context>.Filter.And(
                Builders<Context>.Filter.Eq(o => o.UserId, user.Id),           //same user id
                Builders<Context>.Filter.Eq(o => o.Country, user.Country));    //same country (shard)

            var update = Builders<Context>.Update.Pull(o => o.Comparator, ObjectId.Parse(productId));
            contextCollection.UpdateOne(filter, update);
        }

        public static List<string> CheckAvailability(string productId)
        {
            var storeCollection = DbSingleton.GetDatabase().GetCollection<Store>(AppSettings.StoreCollection);
            var filter = Builders<Store>.Filter.Empty;

            List<Store> stores = storeCollection.Find(filter).ToList();
            List<string> storeNames = new List<string>();

            for (int i = 0; i < stores.Count(); i++)
            {
                for (int j = 0; j < stores[i].ProductsInStock.Count(); j++)
                {
                    if (stores[i].ProductsInStock[j].ProductId.ToString().Equals(productId))
                    {
                        if (stores[i].ProductsInStock[j].Stock > 0)
                        {
                            storeNames.Add(stores[i].Name);
                            break;
                        }
                    }
                }
            }

            return storeNames;
        }
    }
}