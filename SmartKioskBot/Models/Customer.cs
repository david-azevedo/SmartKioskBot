using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartKioskBot.Models
{
    /**
     * Customer vs User
     * 
     * A Customer is an object belonging to the CRM while a User is related to a dialog. 
     * 
     * A Customer contains information about the purchasing activity of a person.
     * A User contains information about the interaction of a person with the bot.
     */

    [Serializable, JsonObject]
    public class Customer
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("userId")]
        public ObjectId UserId{ get; set; }

        [BsonElement("country")]
        public string Country { get; set; }

        [BsonElement("productsBought")]
        public ProductBought[] ProductsBought { get; set; }

        [BsonElement("filtersCount")]
        public FilterCount[] FiltersCount { get; set; }

        [BsonElement("productsClick")]
        public ProductClicks[] ProductsClicks { get; set; }

        [Serializable, JsonObject]
        public class FilterCount
        {
            [BsonElement("filter")]
            public string Filter { get; set; }

            [BsonElement("nSearches")]
            public int NSearches { get; set; }
        }

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