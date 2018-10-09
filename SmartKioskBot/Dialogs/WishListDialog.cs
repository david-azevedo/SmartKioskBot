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
using SmartKioskBot.Helpers;
using AdaptiveCards;
using static SmartKioskBot.Helpers.Constants;
using static SmartKioskBot.Helpers.AdaptiveCardHelper;
using Newtonsoft.Json.Linq;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class WishListDialog : IDialog<object>
    {
        private ObjectId[] wishes;
        private int skip = 0;

        public WishListDialog(User user)
        {
            this.wishes = ContextController.GetContext(user.Id).WishList;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await ShowWishesAsync(context, null);
        }

        //SHOW WISHES
        public async Task ShowWishesAsync(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            //Retriece wishes information
            var to_retrieve = wishes;

            if (wishes.Length > Constants.N_ITEMS_CARROUSSEL)
            {
                to_retrieve = wishes.Skip(this.skip)                    //skip id's already fetched
                                    .Take(Constants.N_ITEMS_CARROUSSEL) //fetch only 7 elements
                                    .ToArray();
            }

            var products = ProductController.getProducts(to_retrieve);

            //Prepare answer

            var reply = context.MakeMessage();
            var text = "";

            // No products on wishlsit
            if (products.Count == 0)
            {
                text = BotDefaultAnswers.getWishList(BotDefaultAnswers.State.FAIL,0);
                await context.PostAsync(text);
            }
            // Has Products
            else
            {
                text = BotDefaultAnswers.getWishList(BotDefaultAnswers.State.SUCCESS,skip/Constants.N_ITEMS_CARROUSSEL + 1);
                await context.PostAsync(text);

                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                List<Attachment> cards = new List<Attachment>();

                for (var i = 0; i < products.Count() && i < Constants.N_ITEMS_CARROUSSEL; i++)
                {
                    cards.Add(ProductCard.GetProductCard(products[i], ProductCard.CardType.WISHLIST).ToAttachment());
                }

                reply.Attachments = cards;

                await context.PostAsync(reply);

                //Check if pagination is needed
                if (wishes.Length <= this.skip + Constants.N_ITEMS_CARROUSSEL)
                    context.Done(new CODE(DIALOG_CODE.DONE));
                else
                {
                    reply = context.MakeMessage();
                    reply.Attachments.Add(await getCardAttachment(CardType.PAGINATION));
                    await context.PostAsync(reply);

                    skip += skip + Constants.N_ITEMS_CARROUSSEL;

                    context.Wait(this.PaginationHandler);
                }
            }
        }

        //PAGINATION
        private async Task PaginationHandler(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var activity = await argument as Activity;

            if(activity.Text != null)
            {
                if(activity.Text == BotDefaultAnswers.next_pagination)
                    context.Done(new CODE(DIALOG_CODE.DONE));
                else
                    context.Done(new CODE(DIALOG_CODE.PROCESS_LUIS, activity as IMessageActivity));
            }
            else if (activity.Value != null)
            {
                JObject json = activity.Value as JObject;
                CardType type = getReplyType(json);

                switch (type)
                {
                    case CardType.PAGINATION:
                        {
                            await StartAsync(context);
                            break;
                        }
                    default:
                        context.Done(new CODE(DIALOG_CODE.DONE));
                        break;
                }
            }
            else
                context.Done(new CODE(DIALOG_CODE.DONE));
        }
        
      
        /*
         * Auxiliary Methods
         */

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
    }
}