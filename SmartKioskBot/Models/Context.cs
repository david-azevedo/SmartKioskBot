using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using Newtonsoft.Json;

namespace SmartKioskBot.Models
{

    [Serializable, JsonObject]
    [BsonDiscriminator(Required = true)]
    [BsonKnownTypes(typeof(Context))]
    public class Context
    {
        [BsonId]
        public MongoDB.Bson.ObjectId Id { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("filters")]
        public string[] Filters { get; set; }

        [BsonElement("wishList")]
        public string[] WishList { get; set; }

        [BsonElement("comparator")]
        public string[] Comparator { get; set; }

    }
}