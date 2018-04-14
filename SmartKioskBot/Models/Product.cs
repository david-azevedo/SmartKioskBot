using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace SmartKioskBot.Models
{
    public class Product
    {
        [BsonId(IdGenerator = typeof(CombGuidGenerator))]
        public Guid Id { get; set; }

        [BsonElement("Brand")]
        public string Brand { get; set; }

        [BsonElement("Model")]
        public string Model { get; set; }

        [BsonElement("Category")]
        public string Category { get; set; }

        [BsonElement("Price")]
        public Double Price { get; set; }

        [BsonElement("Weight")]
        public Double Weight { get; set; }

        [BsonElement("Height")]
        public Double Height { get; set; }

        [BsonElement("Colour")]
        public string Colour { get; set; }
    }
}