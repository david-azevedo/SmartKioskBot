using AdaptiveCards;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using SmartKioskBot.Logic;
using SmartKioskBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using static SmartKioskBot.Logic.AccountLogic;
using static SmartKioskBot.Models.Context;

namespace SmartKioskBot.Helpers
{
    public abstract class AdaptiveCardHelper
    {
        public static string CARDS_PATH = HostingEnvironment.MapPath(@"~/UI");

        
        public enum CardType {VIEW_ACCOUNT, EDIT_ACCOUNT, FILTER, MENU, INFO_MENU, NOT_LOGIN, REGISTER, LOGIN, NONE};
        public enum ButtonType { PAGINATION, FILTER_AGAIN, ADD_PRODUCT, COMPARE};
        //Clicked Button
        public enum ClickType {
            MENU,           //Is parsed better in the dialog
            FILTER,
            PAGINATION,
            FILTER_AGAIN,
            ADD_PRODUCT,
            COMPARE,
            ACCOUNT_EDIT,
            ACCOUNT_SAVE,
            LOGOUT,
            REGISTER,
            REGISTER_SAVE,
            LOGIN,
            LOGIN_START,
            NONE };

        public static string REPLY_ATR = "reply_type";
        public static string DIALOG_ATR = "dialog";

       
        public static async Task<Attachment> getCardAttachment(CardType type)
        {
            string path = getCardFileName(type);
            string json = await FileAsync.ReadAllTextAsync(CARDS_PATH + "/" + path + ".JSON");
            var content = JObject.Parse(@json);

            Attachment att = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = content
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
                case CardType.VIEW_ACCOUNT:
                    return "AccountCard";
                case CardType.EDIT_ACCOUNT:
                    return "EditAccountCard";
                case CardType.NOT_LOGIN:
                    return "NotLoginCard";
                case CardType.REGISTER:
                    return "RegisterCard";
                case CardType.LOGIN:
                    return "LoginCard";
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
                case "account_logout":
                    return ClickType.LOGOUT;
                case "account_edit":
                    return ClickType.ACCOUNT_EDIT;
                case "account_save":
                    return ClickType.ACCOUNT_SAVE;
                case "register":
                    return ClickType.REGISTER;
                case "login":
                    return ClickType.LOGIN;
                case "register_save":
                    return ClickType.REGISTER_SAVE;
                case "login_start":
                    return ClickType.LOGIN_START;
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

       
    }
}