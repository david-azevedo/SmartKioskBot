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
        private int skip = 0;
        private State state;

        public enum State { INIT, INPUT_HANDLER };

        public WishListDialog(State state)
        {
            this.state = state;
        }

        public async Task StartAsync(IDialogContext context)
        {
            switch (state)
            {
                case State.INIT:
                    await ShowWishesAsync(context, null);
                    break;
                case State.INPUT_HANDLER:
                    context.Wait(InputHandler);
                    break;
            }
        }

        public async Task ShowWishesAsync(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            List<string> wishes = StateHelper.GetWishlistItems(context);

            //Retrive wishes information
            var to_retrieve = wishes;

            //options
            List<ButtonType> buttons = new List<ButtonType>();

            //fetch only a limited number of wishes
            if (wishes.Count() > Constants.N_ITEMS_CARROUSSEL)
            {
                to_retrieve = wishes.Skip(this.skip)
                                    .Take(Constants.N_ITEMS_CARROUSSEL)
                                    .ToList();
            }

            var products = new List<Product>();

            foreach (string i in to_retrieve)
                products.Add(ProductController.getProduct(i));

            //Prepare answer

            var reply = context.MakeMessage();
            var text = "";

            var button_text = "";

            // No products on wishlsit
            if (products.Count == 0)
            {
                text = Interactions.getWishList(Interactions.State.FAIL,0);
                await context.PostAsync(text);
            }
            // Has Products
            else
            {
                text = Interactions.getWishList(Interactions.State.SUCCESS,skip/Constants.N_ITEMS_CARROUSSEL + 1);
                await Interactions.SendMessage(context, text, 0, 2500);
                
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
                if (wishes.Count() > this.skip + Constants.N_ITEMS_CARROUSSEL)
                {
                    buttons.Add(ButtonType.PAGINATION);
                    skip += skip + Constants.N_ITEMS_CARROUSSEL;
                    button_text += "Não consegui exibir todos os seus produtos favoritos. Se desejar ver mais clique no botão abaixo.";
                }
            }

            //add option add more products
            buttons.Add(ButtonType.ADD_PRODUCT);
            button_text += "\nSe desejar adicionar mais produtos aos seus favoritos faça uma pesquisa no nosso catálogo. Para isso, clique no botão abaixo.";

            await Interactions.SendMessage(context, button_text, 2000, 2000);

            //show options
            reply = context.MakeMessage();
            reply.Attachments.Add(getCardButtonsAttachment(buttons, DialogType.WISHLIST));
            await context.PostAsync(reply);

            context.Wait(this.InputHandler);
        }
        
        private async Task InputHandler(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var activity = await argument as Activity;

            //received message
            if (activity.Text != null)
                context.Done(new CODE(DIALOG_CODE.PROCESS_LUIS, activity));
            //received event
            else if (activity.Value != null)
                await EventHandler(context, activity);
            else
                context.Done(new CODE(DIALOG_CODE.DONE));
        }

        private async Task EventHandler(IDialogContext context, Activity activity)
        {

            JObject json = activity.Value as JObject;
            List<InputData> data = getReplyData(json);

            //have mandatory info
            if (data.Count >= 2)
            {
                //json structure is correct
                if (data[0].attribute == REPLY_ATR && data[1].attribute == DIALOG_ATR)
                {
                    ClickType event_click = getClickType(data[0].value);
                    DialogType event_dialog = getDialogType(data[1].value);

                    //process in this dialog
                    if (event_dialog == DialogType.WISHLIST &&
                        event_click != ClickType.NONE)
                    {
                        switch (event_click)
                        {
                            case ClickType.PAGINATION:
                                await StartAsync(context);
                                break;
                            case ClickType.ADD_PRODUCT:
                                var dialog = new FilterDialog(FilterDialog.State.INIT);
                                context.Call(dialog, ResumeAfterDialogCall);
                                break;
                        }
                    }
                    //process in parent dialog
                    else
                        context.Done(new CODE(DIALOG_CODE.PROCESS_EVENT, activity,event_dialog));
                }
                else
                    context.Done(new CODE(DIALOG_CODE.DONE));
            }
            else
                context.Done(new CODE(DIALOG_CODE.DONE));
        }

        private async Task ResumeAfterDialogCall(IDialogContext context, IAwaitable<object> result)
        {
            CODE code = await result as CODE;
            if (code.dialog == DialogType.WISHLIST)
                await EventHandler(context, code.activity);
            context.Done(code);
        }

        /*
         * Auxiliary Methods
         */

        public static void AddToWishList(IDialogContext context, string message)
        {
            string[] parts = message.Split(':');
            var product = parts[1].Replace(" ", "");

            if (parts.Length >= 2)
                StateHelper.AddItemWishList(context, product);
        }

        public static void RemoveFromWishList(IDialogContext context, string message)
        {
            string[] parts = message.Split(':');
            var product = parts[1].Replace(" ", "");

            if (parts.Length >= 2)
                StateHelper.RemItemWishlist(context, product);
        }
    }
}