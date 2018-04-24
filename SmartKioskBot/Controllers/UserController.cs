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

        public static void CreateUser(string channelId, string email, string name, string country)
        {
            User u = new User(name, email, country, channelId);
            var userCollection = DbSingleton.GetDatabase().GetCollection<User>(AppSettings.UserCollection);
            userCollection.InsertOne(u);
        }

        public static void AddChannel(User user, string channelId)
        {
            if (!user.ChannelsIds.Contains(channelId))
            {
                var userCollection = DbSingleton.GetDatabase().GetCollection<User>(AppSettings.UserCollection);
                var update = Builders<User>.Update.Push(o => o.ChannelsIds,channelId);
                var filter = Builders<User>.Filter.Eq(o => o.Id,user.Id);

                userCollection.UpdateOne(filter, update);
            }
        }

        public static void SetCustomerCard(string card)
        {
            //TODO
        }

        public static string PrintUser(User u)
        {
            var channels = "";

            for(var i = 0; i < u.ChannelsIds.Length; i++)
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