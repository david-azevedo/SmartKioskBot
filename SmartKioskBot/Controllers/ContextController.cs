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
    public class ContextController
    {
        /// <summary>
        /// Get a conversation context related to a user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static Context GetContext(ObjectId userId)
        {

            var contextCollection = DbSingleton.GetDatabase().GetCollection<Context>(AppSettings.ContextCollection);
            var filter = MongoDB.Driver.Builders<Context>.Filter.Eq(c => c.UserId, userId);

            List<Context> context = contextCollection.Find(filter).ToList();

            if (context.Count() == 0)
                return null;
            else
                return context[0];
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
                WishList = new string[] { },
                Comparator = new string[] { }
            };
            contextCollection.InsertOne(c);
        }

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
                var newFilters = filters.Where(val => val.FilterName == filterName).ToArray();

                var update = Builders<Context>.Update.Set(o => o.Filters, newFilters);
                contextCollection.UpdateOne(filter, update);
            }
        }

        public static void cleanFilters(User user)
        {
            var contextCollection = DbSingleton.GetDatabase().GetCollection<Context>(AppSettings.ContextCollection);

            var filter = Builders<Context>.Filter.And(
                Builders<Context>.Filter.Eq(o => o.UserId, user.Id),           //same user id
                Builders<Context>.Filter.Eq(o => o.Country, user.Country));    //same country (shard)
            var update = Builders<Context>.Update.Set(o => o.Filters, new Filter[] { });

            contextCollection.UpdateOne(filter, update);
        }

        public static void AddWishList(string productId)
        {
            //TODO
        }

        public static void RemWishList(string productId)
        {
            //TODO
        }

        public static void AddComparator(string productId)
        {
            //TODO
        }

        public static void RemComparator(string productId)
        {
            //TODO
        }
    }
}