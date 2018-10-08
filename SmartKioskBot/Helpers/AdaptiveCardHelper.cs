using AdaptiveCards;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace SmartKioskBot.Helpers
{
    public abstract class AdaptiveCardHelper
    {
        public static string CARDS_PATH = HostingEnvironment.MapPath(@"~/UI");

        public enum CardType {PAGINATION, FILTER, NONE};

        private static string getCardPath(CardType type)
        {
            switch(type){
                case CardType.PAGINATION:
                    return "PaginationCard";
                case CardType.FILTER:
                    return "FilterCard";
            }
            return "";
        }

        public static async Task<Attachment> getCardAttachment(CardType type)
        {
            string path = getCardPath(type);
            string json = await FileAsync.ReadAllTextAsync(CARDS_PATH + "/" + path + ".JSON");

            Attachment att = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JObject.Parse(@json)
            };

            return att;
        }
    }
}