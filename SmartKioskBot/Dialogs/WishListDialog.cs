using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using MongoDB.Bson;
using SmartKioskBot.Controllers;
using SmartKioskBot.Models;
using SmartKioskBot.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class WishListDialog
    {

        public static void AddToWishList(string message, User user)
        {
            string[] parts = message.Split(':');
            var product = parts[1].Replace(" ", "");

            if (parts.Length >= 2)
                ContextController.AddWishList(user, product);
        }

        public static void RemoveFromWishList(string message, User user)
        {
            string[] parts = message.Split(':');
            var product = parts[1].Replace(" ", "");

            if (parts.Length >= 2)
                ContextController.RemWishList(user, product);
        }

        public static IMessageActivity ViewWishList(IDialogContext _context, Context context)
        {
            //getProducts
            var products = ProductController.getProducts(context.WishList);

            var reply = _context.MakeMessage();
            var text = "";

            if (products.Count == 0)
                text = BotDefaultAnswers.getWishList(BotDefaultAnswers.State.FAIL);
            else
                text = BotDefaultAnswers.getWishList(BotDefaultAnswers.State.SUCCESS);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            List<Attachment> cards = new List<Attachment>();

            for(var i=0; i < products.Count() && i < 7;i++)
            {
                cards.Add(ProductCard.GetProductCard(products[i], ProductCard.CardType.WISHLIST).ToAttachment());
            }

            reply.Attachments = cards;

            _context.PostAsync(text);
            return reply;
        }
    }
}