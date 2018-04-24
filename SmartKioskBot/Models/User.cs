using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using Newtonsoft.Json;

namespace SmartKioskBot.Models
{
    [Serializable, JsonObject]
    [BsonDiscriminator(Required = true)]
    [BsonKnownTypes(typeof(User))]
    public class User
    {
        [BsonId]
        public MongoDB.Bson.ObjectId Id { get; set; }

        [BsonElement("channelsIds")]
        public string[] ChannelsIds { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("customerCard")]
        public string CustomerCard { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("country")]
        public string Country { get; set; }

        [BsonConstructor]
        public User(string name, string email, string country, string channelId)
        {
            this.Name = name;
            this.Email = email;
            this.Country = country;
            this.ChannelsIds = new string[] { channelId };
            this.CustomerCard = "";
        }
    }
}