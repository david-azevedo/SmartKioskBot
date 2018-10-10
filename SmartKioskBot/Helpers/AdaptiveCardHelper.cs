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

        public static CardType getReplyType(JObject json)
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
    }
}