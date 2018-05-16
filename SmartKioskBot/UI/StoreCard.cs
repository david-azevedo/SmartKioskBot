using Microsoft.Bot.Connector;
using SmartKioskBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartKioskBot.Dialogs;
using SmartKioskBot.Controllers;
using MongoDB.Driver;
using static SmartKioskBot.UI.ProductCard;

namespace SmartKioskBot.UI
{
    public abstract class StoreCard
    {

        public static HeroCard GetStoreCard(Store s)
        {
            string details = "";
            details += "Nome: " + s.Name + "\n" +
                        "Morada: " + s.Address + "\n" +
                        "Telefone: " + s.PhoneNumber + "\n";
            return new HeroCard
            {
                Title = s.Name,
                Text = details
            };
        }

        public static HeroCard GetStoreDetailsCard(Store s, string productId)
        {
            string details = "";
            details += "Nome: " + s.Name + "\n\n" +
                        "Morada: " + s.Address + "\n\n" +
                        "Telefone: " + s.PhoneNumber + "\n\n";

            var buttons = new List<CardAction>();
            buttons.Add(new CardAction(ActionTypes.ImBack, "Encontrar produto dentro da loja", value: BotDefaultAnswers.in_store_location1 + productId + ":" + s.Id.ToString()));

            return new HeroCard
            {
                Title = s.Name,
                Text = details,
                Buttons = buttons
            };
        }

    }
}