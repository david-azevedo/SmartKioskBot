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

    public abstract class UserController
    {
        /// <summary>
        /// Get user by email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static User getUser(ObjectId id)
        {
            var userCollection = DbSingleton.GetDatabase().GetCollection<User>(AppSettings.UserCollection);
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);

            List<User> user = userCollection.Find(filter).ToList();

            if (user.Count() == 0)
                return null;
            else
                return user[0];
        }

        /// <summary>
        /// Get user by email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static User getUserByEmail(string email)
        {
            var userCollection = DbSingleton.GetDatabase().GetCollection<User>(AppSettings.UserCollection);
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);

            List<User> user = userCollection.Find(filter).ToList();

            if (user.Count() == 0)
                return null;
            else
                return user[0];
        }

        /// <summary>
        /// Gets a user by store card
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public static User getUserByCard(string card)
        {
            var userCollection = DbSingleton.GetDatabase().GetCollection<User>(AppSettings.UserCollection);
            var filter = Builders<User>.Filter.Eq(u => u.CustomerCard, card);

            List<User> user = userCollection.Find(filter).ToList();

            if (user.Count() == 0)
                return null;
            else
                return user[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public static void DeleteUser(User user)
        {
            var userCollection = DbSingleton.GetDatabase().GetCollection<User>(AppSettings.UserCollection);
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(o => o.Country, user.Country),
                Builders<User>.Filter.Eq(o => o.Id, user.Id));
            userCollection.DeleteOne(filter);
        }

        /// <summary>
        /// Get user by customer card.
        /// </summary>
        /// <param name="customerCard"></param>
        /// <returns></returns>
        public static User getUserByCustomerCard(string customerCard)
        {
            var userCollection = DbSingleton.GetDatabase().GetCollection<User>(AppSettings.UserCollection);
            var filter = Builders<User>.Filter.Eq(u => u.CustomerCard, customerCard);

            List<User> user = userCollection.Find(filter).ToList();

            if (user.Count() == 0)
                return null;
            else
                return user[0];
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="email"></param>
        /// <param name="name"></param>
        /// <param name="country"></param>
        public static void CreateUser(string email, string name, string country, string gender)
        {
            User u = new User()
            {
                Name = name,
                Email = email,
                Country = country,
                CustomerCard = "",
                Gender = gender
            };

            var userCollection = DbSingleton.GetDatabase().GetCollection<User>(AppSettings.UserCollection);
            userCollection.InsertOne(u);
        }
        
        /// <summary>
        /// Sets the user information.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="customerId"></param>
        public static void SetUserInfo(User user, string name, string email, string customerId, string gender)
        {
            var userCollection = DbSingleton.GetDatabase().GetCollection<User>(AppSettings.UserCollection);
            var update = Builders<User>.Update.Set(o => o.Name, name).Set(o => o.Email, email).Set(o => o.CustomerCard, customerId).Set(o => o.Gender, gender);
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(o => o.Id, user.Id),
                Builders<User>.Filter.Eq(o => o.Country, user.Country));

            userCollection.UpdateOne(filter, update);
        }

        /// <summary>
        /// Sets the user customer card id.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="card"></param>
        public static void SetCustomerCard(User user, string card)
        {
            var userCollection = DbSingleton.GetDatabase().GetCollection<User>(AppSettings.UserCollection);
            var update = Builders<User>.Update.Set(o => o.CustomerCard, card);
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(o => o.Id, user.Id),
                Builders<User>.Filter.Eq(o => o.Country, user.Country));

            userCollection.UpdateOne(filter, update);
        }

        /// <summary>
        /// Sets the user email.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="email"></param>
        public static void SetEmail(User user, string email)
        {
            var userCollection = DbSingleton.GetDatabase().GetCollection<User>(AppSettings.UserCollection);
            var update = Builders<User>.Update.Set(o => o.Email, email);
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(o => o.Id, user.Id),
                Builders<User>.Filter.Eq(o => o.Country, user.Country));

            userCollection.UpdateOne(filter, update);
        }

        public static void SetGender(User user, string gender)
        {
            var userCollection = DbSingleton.GetDatabase().GetCollection<User>(AppSettings.UserCollection);
            var update = Builders<User>.Update.Set(o => o.Gender, gender);
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(o => o.Id, user.Id),
                Builders<User>.Filter.Eq(o => o.Country, user.Country));

            userCollection.UpdateOne(filter, update);
        }

        /// <summary>
        /// Sets the user email.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="name"></param>
        public static void SetCustomerName(User user, string name)
        {
            var userCollection = DbSingleton.GetDatabase().GetCollection<User>(AppSettings.UserCollection);
            var update = Builders<User>.Update.Set(o => o.Name, name);
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(o => o.Id, user.Id),
                Builders<User>.Filter.Eq(o => o.Country, user.Country));

            userCollection.UpdateOne(filter, update);
        }

        public static void MergeUsers(User from_user, User into_user)
        {
            var from_context = ContextController.GetContext(from_user.Id);

            //Merge Context
            foreach (var f in from_context.Filters)
            {
                ContextController.AddFilter(into_user, f.FilterName, f.Operator, f.Value);
            }

            foreach (var w in from_context.WishList)
            {
                ContextController.AddWishList(into_user, w.ToString());
            }

            foreach (var c in from_context.Comparator)
            {
                ContextController.AddComparator(into_user, c.ToString());
            }

            //Merge CRM
            var from_customer = CRMController.GetCustomer(from_user.Id);
            foreach (var f in from_customer.FiltersCount)
            {
                CRMController.AddFilterUsage(into_user.Id, into_user.Country, f.Filter);
            }

            foreach (var pb in from_customer.ProductsBought)
            {
                CRMController.AddPurchase(into_user.Id, into_user.Country, pb.ProductId);
            }

            foreach (var pc in from_customer.ProductsClicks)
            {
                CRMController.AddPurchase(into_user.Id, into_user.Country, pc.ProductId);
            }
        }

        /// <summary>
        /// DEBUG - Prints the user's information.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public static string PrintUser(User u)
        {
            string o = "User Information: \n\n" +
                "ID: " + u.Id.ToString() + "\n\n" +
                "Name: " + u.Name + "\n\n" +
                "Country: " + u.Country + "\n\n" +
                "Email: " + u.Email + "\n\n" +
                "CustomerCard: " + u.CustomerCard + "\n\n" + 
                "Gender: " + u.Gender + "\n\n";

            return o;
        }

    }
}