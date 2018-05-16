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
    public abstract class ProductCard
    {
        public enum CardType { SEARCH, RECOMMENDATION, WISHLIST, COMPARATOR, PRODUCT_DETAILS, STORE_DETAILS};

        public static HeroCard GetProductCard(Product p, CardType type)
        {
            return new HeroCard
            {
                // title of the card  
                Title = p.Brand + "\n\n Modelo: " + p.Model,
                //subtitle of the card  
                Subtitle = p.Price + "€",
                // navigate to page , while tab on card  
                Tap = new CardAction(ActionTypes.ImBack, "Ver detalhes", value: BotDefaultAnswers.show_product_details + " " + p.Id.ToString()),
                //Detail Text  
                Text = p.Name,
                // list of  Large Image  
                Images = new List<CardImage> { new CardImage(p.Photo) },
                // list of buttons   
                Buttons = getButtonsCardType(type, p.Id.ToString())
            };
        }

        private static List<CardAction> getButtonsCardType(CardType type, string id)
        {
            var buttons = new List<CardAction>();

            switch (type)
            {
                case CardType.SEARCH:
                case CardType.RECOMMENDATION:
                    {
                        buttons.Add(new CardAction(ActionTypes.ImBack, "Adicionar à Wish List", value: BotDefaultAnswers.add_wish_list + id));
                        buttons.Add(new CardAction(ActionTypes.ImBack, "Adicionar ao Comparador", value: BotDefaultAnswers.add_to_comparator + id));
                        buttons.Add(new CardAction(ActionTypes.ImBack, "Verificar Disponibilidade", value: BotDefaultAnswers.show_store_with_stock + id));
                        break;
                    }
                case CardType.PRODUCT_DETAILS:
                    {
                        buttons.Add(new CardAction(ActionTypes.ImBack, "Adicionar à Wish List", value: BotDefaultAnswers.add_wish_list + id));
                        buttons.Add(new CardAction(ActionTypes.ImBack, "Adicionar ao Comparador", value: BotDefaultAnswers.add_to_comparator + id));
                        buttons.Add(new CardAction(ActionTypes.ImBack, "Ver Pacotes", value: BotDefaultAnswers.add_to_comparator + id));
                        buttons.Add(new CardAction(ActionTypes.ImBack, "Produtos Relacionados", value: BotDefaultAnswers.add_to_comparator + id));
                        buttons.Add(new CardAction(ActionTypes.ImBack, "Verificar Disponibilidade", value: BotDefaultAnswers.show_store_with_stock + id));
                        break;
                    }
                case CardType.WISHLIST:
                    {
                        buttons.Add(new CardAction(ActionTypes.ImBack, "Remover da Wish List", value: BotDefaultAnswers.rem_wish_list + id));
                        buttons.Add(new CardAction(ActionTypes.ImBack, "Adicionar ao Comparador", value: BotDefaultAnswers.add_to_comparator + id));
                        buttons.Add(new CardAction(ActionTypes.ImBack, "Verificar Disponibilidade", value: BotDefaultAnswers.show_store_with_stock + id));
                        break;
                    }
                case CardType.COMPARATOR:
                    {
                        buttons.Add(new CardAction(ActionTypes.ImBack, "Adicionar à Wish List", value: BotDefaultAnswers.add_wish_list + id));
                        buttons.Add(new CardAction(ActionTypes.ImBack, "Remover do Comparador", value: BotDefaultAnswers.rem_comparator + id));
                        break;
                    }
            }

            return buttons;
        }

    }
}