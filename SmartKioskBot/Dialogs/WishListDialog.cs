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
using static SmartKioskBot.Models.Context;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class WishListDialog : IDialog<object>
    {
        private ObjectId[] wishes;
        private int skip = 0;
        private User user = null;

        public WishListDialog(User user)
        {
            this.user = user;
            this.wishes = ContextController.GetContext(user.Id).WishList;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await ShowWishesAsync(context, null);
        }

        //SHOW WISHES
        public async Task ShowWishesAsync(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {

            //Retrive wishes information
            var to_retrieve = wishes;

            //fetch only a limited number of wishes
            if (wishes.Length > Constants.N_ITEMS_CARROUSSEL)
            {
                to_retrieve = wishes.Skip(this.skip)               
                                    .Take(Constants.N_ITEMS_CARROUSSEL) 
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

                List<ButtonType> buttons = new List<ButtonType>();

                //display products 
                reply = context.MakeMessage();
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                List<Attachment> cards = new List<Attachment>();

                //limit 
                for (var i = 0; i < products.Count && i < Constants.N_ITEMS_CARROUSSEL; i++)
                    cards.Add(ProductCard.GetProductCard(products[i], ProductCard.CardType.WISHLIST).ToAttachment());

                reply.Attachments = cards;
                await context.PostAsync(reply);

                //Check if pagination is needed and display wishes
                if (wishes.Length > this.skip + Constants.N_ITEMS_CARROUSSEL)
                {
                    buttons.Add(ButtonType.PAGINATION);
                    skip += skip + Constants.N_ITEMS_CARROUSSEL;
                }

                //add option add more products
                buttons.Add(ButtonType.ADD_PRODUCT);

                //show options
                reply = context.MakeMessage();
                reply.Attachments.Add(getCardButtonsAttachment(buttons, DialogType.WISHLIST));
                await context.PostAsync(reply);

                context.Wait(this.InputHandler);
            }
        }
        
        private async Task InputHandler(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var activity = await argument as Activity;

            //close dialog at the end without more processing
            bool done_ok = true;

            //received message
            if(activity.Text != null)
            {
                done_ok = false;
                context.Done(new CODE(DIALOG_CODE.PROCESS_LUIS, activity as IMessageActivity));
            }
            //received event
            else if (activity.Value != null)
            {
                
                JObject json = activity.Value as JObject;
                List<InputData> data = getReplyData(json);

                //have mandatory info
                if(data.Count >= 2)
                {
                    //json structure is correct
                    if(data[0].attribute == REPLY_ATR && data[1].attribute == DIALOG_ATR)
                    {
                        ClickType click = getClickType(data[0].value);

                        if (data[1].value.Equals(getDialogName(DialogType.WISHLIST)) &&
                            click != ClickType.NONE)
                        {
                            switch (click)
                            {
                                case ClickType.PAGINATION:
                                    await StartAsync(context);
                                    done_ok = false;
                                    break;
                                case ClickType.ADD_PRODUCT:
                                    var dialog = new FilterDialog(user, new List<Filter>(), FilterDialog.State.INIT);
                                    context.Call(dialog, ResumeAfterDialogCall);
                                    done_ok = false;
                                    break;
                            }
                        }
                    }
                }
            }

            if(done_ok)
                context.Done(new CODE(DIALOG_CODE.DONE));
        }

        private async Task ResumeAfterDialogCall(IDialogContext context, IAwaitable<object> result)
        {
            CODE code = await result as CODE;
            context.Done(code);
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