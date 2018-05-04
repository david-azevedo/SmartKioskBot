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
    public class CRM
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("userId")]
        public ObjectId UserId{ get; set; }

        [BsonElement("productsBought")]
        public ProductBought[] ProductsBought { get; set; }

        [BsonElement("filtersCount")]
        public string[] FiltersCount { get; set; }

        [BsonElement("productsClick")]
        public ProductClicks[] ProductsClicks { get; set; }

        [Serializable, JsonObject]
        public class ProductClicks
        {
            [BsonElement("productId")]
            public ObjectId ProductId { get; set; }

            [BsonElement("nClicks")]
            public int NClicks { get; set; }
        }

        [Serializable, JsonObject]
        public class ProductBought
        {
            [BsonElement("productId")]
            public ObjectId ProductId { get; set; }

            [BsonElement("date")]
            public DateTime Date { get; set; }
        }
    }
}