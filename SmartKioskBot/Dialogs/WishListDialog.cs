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

            if (products.Count == 0)
                reply.Text = BotDefaultAnswers.getWishList(BotDefaultAnswers.State.FAIL);
            else
                reply.Text = BotDefaultAnswers.getWishList(BotDefaultAnswers.State.SUCCESS);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            List<Attachment> cards = new List<Attachment>();

            foreach (Product p in products)
            {
                cards.Add(ProductCard.GetProductCard(p, ProductCard.CardType.WISHLIST).ToAttachment());
            }

            reply.Attachments = cards;
            return reply;
        }
    }
}