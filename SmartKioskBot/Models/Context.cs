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
    [BsonDiscriminator(Required = true)]
    [BsonKnownTypes(typeof(Context))]
    public class Context
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("userId")]
        public ObjectId UserId { get; set; }

        [BsonElement("filters")]
        public Filter[] Filters { get; set; }

        [BsonElement("wishList")]
        public string[] WishList { get; set; }

        [BsonElement("comparator")]
        public string[] Comparator { get; set; }

        [BsonConstructor]
        public Context(ObjectId userId)
        {
            this.UserId = userId;
            this.Filters = new Filter[] { };
            this.WishList = new string[] { };
            this.Comparator = new string[] { };
        }

        [Serializable, JsonObject]
        public class Filter
        {
            [BsonElement("filterName")]
            public string FilterName { get; set; }

            [BsonElement("operator")]
            public string Operator { get; set; }

            [BsonElement("value")]
            public string Value { get; set; }

            [BsonConstructor]
            public Filter(string filterName, string op, string value)
            {
                this.FilterName = filterName;
                this.Operator = op;
                this.Value = value;
            }
        }

    }
}