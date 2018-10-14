using AdaptiveCards;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using SmartKioskBot.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using static SmartKioskBot.Models.Context;

namespace SmartKioskBot.Helpers
{
    public abstract class AdaptiveCardHelper
    {
        public static string CARDS_PATH = HostingEnvironment.MapPath(@"~/UI");

        public enum CardType {
            PAGINATION,
            FILTER,
            FILTER_AGAIN,
            NONE,
            MENU,
            INFO_MENU
        };

        private static string getCardFileName(CardType type)
        {
            switch(type){
                case CardType.PAGINATION:
                    return "PaginationCard";
                case CardType.FILTER:
                    return "FilterCard";
                case CardType.FILTER_AGAIN:
                    return "FilterAgainCard";
                case CardType.MENU:
                    return "MenuCard";
                case CardType.INFO_MENU:
                    return "InfoMenuCard";
            }
            return "";
        }

        public static async Task<Attachment> getCardAttachment(CardType type)
        {
            string path = getCardFileName(type);
            string json = await FileAsync.ReadAllTextAsync(CARDS_PATH + "/" + path + ".JSON");

            Attachment att = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JObject.Parse(@json)
            };

            return att;
        }

        public static CardType getCardTypeReply(JObject json)
        {
            JProperty prop = json.First as JProperty;
            InputData type = new InputData(prop);

            if (type.attribute == "reply_type")
            {
                switch (type.value)
                {
                    case "pagination":
                        return CardType.PAGINATION;
                    case "filter":
                        return CardType.FILTER;
                    case "filter_again":
                        return CardType.FILTER_AGAIN;
                    case "menu_session":
                    case "menu_filter":
                    case "menu_comparator":
                    case "menu_recommendations":
                    case "menu_wishlist":
                    case "menu_stores":
                    case "menu_help":
                    case "menu_info":
                        return CardType.MENU;
                }
            }
            return CardType.NONE;
        }

        public static List<InputData> getReplyData(JObject json)
        {
            List<InputData> data = new List<InputData>();
            List<JProperty> to_process = json.Children<JProperty>().ToList();
            
            //ignore reply type, i=0
            for(int i = 1; i < to_process.Count(); i++)
                data.Add(new InputData(to_process[i]));

            return data;
        }

        public class InputData
        {
            public string attribute = "";
            public string value = "";
            public string input = "";

            public InputData(JProperty property)
            {
                if (property.Name.Contains(":"))
                {
                    string[] parts = property.Name.Split(':');
                    this.attribute = parts[0];
                    this.value = parts[1];
                    this.input = (property.Value as JValue).Value.ToString();
                }
                else
                {
                    this.attribute = property.Name;
                    this.value = (property.Value as JValue).Value.ToString();
                }
            }
        }
        
        public static void SetFilterCardValue(JToken card, List<Filter> applied_filters)
        {
            List<JToken> card_fields = new List<JToken>();
            string last_retrieved = "";

            for(int i = 0; i < applied_filters.Count(); i++)
            {
                var f = applied_filters[i];

                if(last_retrieved != f.FilterName)
                    card_fields = GetFilterCardSection(card, f.FilterName);

                string lookup = f.FilterName + ":";
                bool checkbox = false;

                if (f.Operator.Equals("<"))
                    lookup += "max";
                else if (f.Operator.Equals(">"))
                    lookup += "min";
                else
                {
                    lookup += f.Value;
                    checkbox = true;
                }

                for (int j = 0; j < card_fields.Count; j++)
                {
                    if (card_fields[j]["id"].ToString().Equals(lookup))
                    {
                        if (!checkbox)
                            card_fields[j]["value"] = f.Value;
                        else
                            card_fields[j]["value"] = "true";
                        break;
                    }
                }
            }
        }

        /*
         *  CARD SPECIFIC
         */
         
        private static List<JToken> GetFilterCardSection(JToken card, string section)
        {
            List<JToken> fields = new List<JToken>();

            switch (section)
            {
                case FilterLogic.cpu_family_filter:
                    fields = card.SelectTokens("body[1].columns[0].items").Children().ToList();
                    fields.RemoveAt(0);
                    break;
                case FilterLogic.gpu_filter:
                    fields = card.SelectTokens("body[1].columns[1].items").Children().ToList();
                    fields.RemoveAt(0);
                    break;
                case FilterLogic.price_filter:
                    fields.Add(card.SelectToken("body[2].columns[0].items[1].columns[0].items[0]"));
                    fields.Add(card.SelectToken("body[2].columns[0].items[1].columns[1].items[0]"));
                    break;
                case FilterLogic.storage_type_filter:
                    fields.Add(card.SelectToken("body[3].items[1]"));
                    fields.Add(card.SelectToken("body[3].items[2]"));
                    break;
                case FilterLogic.storage_filter:
                    fields.Add(card.SelectToken("body[3].items[3].columns[0].items[0]"));
                    fields.Add(card.SelectToken("body[3].items[3].columns[1].items[0]"));
                    break;
                case FilterLogic.ram_filter:
                    fields.Add(card.SelectToken("body[3].items[5].columns[0].items[0]"));
                    fields.Add(card.SelectToken("body[3].items[5].columns[1].items[0]"));
                    break;
                case FilterLogic.brand_filter:
                    fields = card.SelectTokens("body[4].items[1].columns[0].items").Children().ToList();
                    fields = fields.Concat(card.SelectTokens("body[4].items[1].columns[1].items").Children().ToList()).ToList();
                    fields = fields.Concat(card.SelectTokens("body[4].items[1].columns[2].items").Children().ToList()).ToList();
                    break;
                case FilterLogic.type_filter:
                    fields = card.SelectTokens("body[5].items[1].columns[0].items").Children().ToList();
                    fields = fields.Concat(card.SelectTokens("body[5].items[1].columns[1].items").Children().ToList()).ToList();
                    break;
            }

            return fields;
        }
    }
}