using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartKioskBot.Models
{
    [Serializable, JsonObject]
    public class Store
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("coordinates")]
        public string[] Coordinates { get; set; }

        [BsonElement("productsInStock")]
        public ProductStock[] ProductsInStock { get; set; }

        [BsonElement("address")]
        public string Address { get; set; }

        [BsonElement("phoneNumber")]
        public string PhoneNumber { get; set; }

        [Serializable, JsonObject]
        public class ProductStock
        {
            [BsonElement("productId")]
            public ObjectId ProductId { get; set; }

            [BsonElement("stock")]
            public int Stock { get; set; }
        }
    }
}