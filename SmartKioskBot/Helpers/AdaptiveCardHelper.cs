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

        public enum CardType {FILTER, MENU, INFO_MENU, NONE};
        public enum ButtonType { PAGINATION, FILTER_AGAIN, ADD_PRODUCT, COMPARE};
        public enum ClickType {
            MENU,
            FILTER,
            PAGINATION,
            FILTER_AGAIN,
            ADD_PRODUCT,
            COMPARE,
            NONE };

        public static string REPLY_ATR = "reply_type";
        public static string DIALOG_ATR = "dialog";

       
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

        public static Attachment getCardButtonsAttachment(List<ButtonType> button_types, Constants.DialogType dialog)
        {
            AdaptiveCard card = new AdaptiveCard()
            {
                Version = "1.0",
                Body = { },
                Actions = { }
            };

            foreach (var t in button_types)
                card.Actions.Add(getButtonAction(t,dialog));

            Attachment att = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            return att;
        }
        
        private static AdaptiveSubmitAction getButtonAction(ButtonType type, Constants.DialogType dialog)
        {
            var action = new AdaptiveSubmitAction();
            var data = "{'reply_type' : '";

            switch (type)
            {
                case ButtonType.PAGINATION:
                    action.Title = "Ver Mais";
                    data += "pagination";
                    break;
                case ButtonType.FILTER_AGAIN:
                    action.Title = "Alterar Filtragem";
                    data += "filter_again";
                    break;
                case ButtonType.ADD_PRODUCT:
                    action.Title = "Adicionar Produto";
                    data += "add_product";
                    break;
                case ButtonType.COMPARE:
                    action.Title = "Comparar";
                    data += "compare";
                    break;
            }

            data += "', 'dialog' : '" + Constants.getDialogName(dialog) + "'}";
            action.DataJson = data;
            return action;
        }

        private static string getCardFileName(CardType type)
        {
            switch(type){
                case CardType.FILTER:
                    return "FilterCard";
                case CardType.MENU:
                    return "MenuCard";
                case CardType.INFO_MENU:
                    return "InfoMenuCard";
            }
            return "";
        }
       
        public static ClickType getClickType(string type)
        {
            switch (type)
            {
                case "pagination":
                    return ClickType.PAGINATION;
                case "filter_again":
                    return ClickType.FILTER_AGAIN;
                case "add_product":
                    return ClickType.ADD_PRODUCT;
                case "compare":
                    return ClickType.COMPARE;
                case "filter":
                    return ClickType.FILTER;
                case "menu_session":
                case "menu_filter":
                case "menu_comparator":
                case "menu_recommendations":
                case "menu_wishlist":
                case "menu_stores":
                case "menu_help":
                case "menu_info":
                    return ClickType.MENU;
            }

            return ClickType.NONE;
        }
        /*
         * Events data
         */

        public static List<InputData> getReplyData(JObject json)
        {
            List<InputData> data = new List<InputData>();
            List<JProperty> to_process = json.Children<JProperty>().ToList();
            
            //ignore reply type, i=0
            for(int i = 0; i < to_process.Count(); i++)
                data.Add(new InputData(to_process[i]));

            return data;
        }

        public class InputData
        {
            public string attribute = "";   //ex: cpu,  ex: reply_type              
            public string value = "";       //ex: i3 ,  ex: pagination
            public string input = "";       //ex: true

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

        /*
         * CARD SPECIFIC
         */

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

        private static List<JToken> GetFilterCardSection(JToken card, string section)
        {
            List<JToken> fields = new List<JToken>();

            switch (section)
            {
                case Constants.cpu_family_filter:
                    fields = card.SelectTokens("body[1].columns[0].items").Children().ToList();
                    fields.RemoveAt(0);
                    break;
                case Constants.gpu_filter:
                    fields = card.SelectTokens("body[1].columns[1].items").Children().ToList();
                    fields.RemoveAt(0);
                    break;
                case Constants.price_filter:
                    fields.Add(card.SelectToken("body[2].columns[0].items[1].columns[0].items[0]"));
                    fields.Add(card.SelectToken("body[2].columns[0].items[1].columns[1].items[0]"));
                    break;
                case Constants.storage_type_filter:
                    fields.Add(card.SelectToken("body[3].items[1]"));
                    fields.Add(card.SelectToken("body[3].items[2]"));
                    break;
                case Constants.storage_filter:
                    fields.Add(card.SelectToken("body[3].items[3].columns[0].items[0]"));
                    fields.Add(card.SelectToken("body[3].items[3].columns[1].items[0]"));
                    break;
                case Constants.ram_filter:
                    fields.Add(card.SelectToken("body[3].items[5].columns[0].items[0]"));
                    fields.Add(card.SelectToken("body[3].items[5].columns[1].items[0]"));
                    break;
                case Constants.brand_filter:
                    fields = card.SelectTokens("body[4].items[1].columns[0].items").Children().ToList();
                    fields = fields.Concat(card.SelectTokens("body[4].items[1].columns[1].items").Children().ToList()).ToList();
                    fields = fields.Concat(card.SelectTokens("body[4].items[1].columns[2].items").Children().ToList()).ToList();
                    break;
                case Constants.type_filter:
                    fields = card.SelectTokens("body[5].items[1].columns[0].items").Children().ToList();
                    fields = fields.Concat(card.SelectTokens("body[5].items[1].columns[1].items").Children().ToList()).ToList();
                    break;
            }

            return fields;
        }
    }
}