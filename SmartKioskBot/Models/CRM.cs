using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartKioskBot.Models
{

    [Serializable, JsonObject]
    [BsonDiscriminator(Required = true)]
    [BsonKnownTypes(typeof(CRM))]
    public class CRM
    {
        [BsonId]
        public MongoDB.Bson.ObjectId Id { get; set; }

        [BsonElement("productsBought")]
        public string[] ProductsBought { get; set; }

        [BsonElement("filtersCount")]
        public string[] FiltersCount { get; set; }

        [BsonElement("productsClick")]
        public string ProductsClick { get; set; }
    }
}