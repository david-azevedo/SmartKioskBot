using Microsoft.Bot.Builder.Dialogs;
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
    public class WishListDialog : IDialog
    {
        protected Context userContext;
        protected Action action;
        public enum Action { VIEW, ADD, REM };

        public WishListDialog(Action action)
        {
            this.action = action;
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var message = await activity;

            //fetch user
            var user = UserController.getUser(message.ChannelId);

            var userInput = (message.Text != null ? message.Text : "").Split(new[] { ' ' }, 2);
            string[] details = message.Text.Split(' ');

            //TMP - substituir por LUIS
            if (action.Equals(Action.ADD))
            {
                ContextController.AddWishList(user, details[1]);
                await context.PostAsync(BotDefaultAnswers.getAddWishList());
            }
            else if (action.Equals(Action.REM))
            {
                ContextController.RemWishList(user, details[1]);
                await context.PostAsync(BotDefaultAnswers.getRemWishList());

            }
            else if (action.Equals(Action.VIEW))
            {
                //fetch context
                this.userContext = ContextController.GetContext(user.Id);

                //getProducts
                var products = ProductController.getProducts(userContext.WishList);

                var reply = context.MakeMessage();

                if (products.Count == 0)
                    reply.Text = BotDefaultAnswers.getEmptyWishList();
                else
                    reply.Text = BotDefaultAnswers.getWishList();
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                List<Attachment> cards = new List<Attachment>();

                foreach (Product p in products)
                {
                    cards.Add(ProductCard.GetProductCard(p, ProductCard.CardType.WISHLIST).ToAttachment());
                }

                reply.Attachments = cards;
                await context.PostAsync(reply);
            }
            context.Done<object>(null);
        }
    }
}