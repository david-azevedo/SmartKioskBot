using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using Newtonsoft.Json;

namespace SmartKioskBot.Models
{
    [Serializable, JsonObject]
    [BsonIgnoreExtraElements]
    public class User
    {
        [BsonId]
        public MongoDB.Bson.ObjectId Id { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("customerCard")]
        public string CustomerCard { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("country")]
        public string Country { get; set; }

        [BsonElement("gender")]
        public string Gender { get; set; }
    }
}