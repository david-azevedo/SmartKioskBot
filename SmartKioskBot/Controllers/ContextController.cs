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

        public static void CreateContext(ObjectId userId)
        {
            var contextCollection = DbSingleton.GetDatabase().GetCollection<Context>(AppSettings.ContextCollection);

            Context c = new Context(userId);
            contextCollection.InsertOne(c);
        }

        public static void AddFilter(string filter, string op, string value)
        {
            Filter f = new Filter(filter, op, value);
            //TODO
        }

        public static void RemFilter(string filter)
        {
            //TODO
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