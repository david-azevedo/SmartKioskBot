using Microsoft.Bot.Connector;
using SmartKioskBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartKioskBot.UI
{
    public abstract class ProductCard
    {
        public static HeroCard getCard(Product p)
        {
            return new HeroCard
            {
                // title of the card  
                Title = p.Brand + "\nModelo: " + p.Model,
                //subtitle of the card  
                Subtitle = p.Price + "€",
                // navigate to page , while tab on card  
                //Tap = new CardAction(ActionTypes.OpenUrl, "Learn More", value: "http://www.devenvexe.com"),
                //Detail Text  
                Text = p.Name,
                // list of  Large Image  
                Images = new List<CardImage> { new CardImage(p.Photo) },
                // list of buttons   
                Buttons = new List<CardAction> {
                        new CardAction(ActionTypes.Invoke, "Detalhes", value: ""),
                        new CardAction(ActionTypes.Invoke, "Wish List", value: ""),
                        new CardAction(ActionTypes.Invoke, "Comparar", value: "") }
            };
        }

    }
}