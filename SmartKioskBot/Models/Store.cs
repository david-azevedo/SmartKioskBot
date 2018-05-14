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

        public Double proximity { get; set; }

        [BsonElement("coordinates")]
        public Double[] Coordinates { get; set; }

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

            [BsonElement("InStoreLocation")]
            public ProductLocation InStoreLocation { get; set; }

        }

        public Double calculateProximity(Double[] position)
        {
            return (proximity = Math.Abs(Coordinates[0] - position[0]) + Math.Abs(Coordinates[1] - position[1]));
        }

        [Serializable, JsonObject]
        public class ProductLocation
        {
            [BsonElement("Corridor")]
            public int Corridor { get; set; }

            [BsonElement("Section")]
            public int Section { get; set; }

            [BsonElement("Shelf")]
            public int Shelf { get; set; }
        }
    }
}