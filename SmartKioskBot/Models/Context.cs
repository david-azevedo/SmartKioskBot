using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using Newtonsoft.Json;

namespace SmartKioskBot.Models
{

    [Serializable, JsonObject]
    public class Context
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("userId")]
        public ObjectId UserId { get; set; }

        [BsonElement("country")]
        public string Country { get; set; }

        [BsonElement("filters")]
        public Filter[] Filters { get; set; }

        [BsonElement("wishList")]
        public ObjectId[] WishList { get; set; }

        [BsonElement("comparator")]
        public ObjectId[] Comparator { get; set; }

        [Serializable, JsonObject]
        public class Filter
        {
            [BsonElement("filterName")]
            public string FilterName { get; set; }

            [BsonElement("operator")]
            public string Operator { get; set; }

            [BsonElement("value")]
            public string Value { get; set; }
        }

    }
}