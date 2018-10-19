using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json.Linq;
using SmartKioskBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartKioskBot.Logic
{
    public class AccountLogic
    {
        public enum CardField{ NAME, CLIENT_NUMBER, EMAIL, GENDER};

        /*
         * UI Cards
         */

        public static void SetAccountCardFields(JToken card, User user, bool edit_card)
        {
            var customer_card = user.CustomerCard.ToString();
            var info_tag = "text";
            if (customer_card == "")
                customer_card = "Desconhecido";

            if (edit_card)
                info_tag = "value";

            GetAccountCardSection(card, CardField.NAME)[info_tag] = user.Name;
            GetAccountCardSection(card, CardField.CLIENT_NUMBER)[info_tag] = customer_card;
            GetAccountCardSection(card, CardField.GENDER)[info_tag] = user.Gender;
            GetAccountCardSection(card, CardField.EMAIL)[info_tag] = user.Email;
        }

        private static JToken GetAccountCardSection(JToken card, CardField field)
        {
            switch (field)
            {
                case CardField.NAME:
                    return card.SelectToken("body[1].items[1]");
                case CardField.EMAIL:
                    return card.SelectToken("body[2].items[1]");
                case CardField.CLIENT_NUMBER:
                    return card.SelectToken("body[3].items[1]");
                case CardField.GENDER:
                    return card.SelectToken("body[4].items[1]");
            }

            return null;
        }
    }
}