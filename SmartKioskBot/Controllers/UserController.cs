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
        /// Gets a user that is related to a comunication channel conversation.
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public static User getUser(string channelId)
        {
            var userCollection = DbSingleton.GetDatabase().GetCollection<User>(AppSettings.UserCollection);
            var filter = Builders<User>.Filter.AnyEq(u => u.ChannelsIds, channelId);

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
        public static void CreateUser(string channelId, string email, string name, string country)
        {
            User u = new User()
            {
                Name = name,
                Email = email,
                Country = country,
                ChannelsIds = new string[] { channelId },
                CustomerCard = ""
            };

            var userCollection = DbSingleton.GetDatabase().GetCollection<User>(AppSettings.UserCollection);
            userCollection.InsertOne(u);
        }

        /// <summary>
        /// Adds a new communication channel conversation to the user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="channelId"></param>
        public static void AddChannel(User user, string channelId)
        {
            if (!user.ChannelsIds.Contains(channelId))
            {
                var userCollection = DbSingleton.GetDatabase().GetCollection<User>(AppSettings.UserCollection);
                var update = Builders<User>.Update.Push(o => o.ChannelsIds, channelId);
                var filter = Builders<User>.Filter.And(
                    Builders<User>.Filter.Eq(o => o.Id, user.Id),
                    Builders<User>.Filter.Eq(o => o.Country, user.Country));

                userCollection.UpdateOne(filter, update);
            }
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
        /// <summary>
        /// DEBUG - Prints the user's information.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public static string PrintUser(User u)
        {
            var channels = "";

            for (var i = 0; i < u.ChannelsIds.Length; i++)
            {
                channels += u.ChannelsIds[i] + " ";
            }

            string o = "User Information: \n\n" +
                "ID: " + u.Id.ToString() + "\n\n" +
                "Name: " + u.Name + "\n\n" +
                "Country: " + u.Country + "\n\n" +
                "Channels: " + channels + "\n\n" +
                "Email: " + u.Email + "\n\n" +
                "CustomerCard: " + u.CustomerCard + "\n\n";

            return o;
        }

    }
}