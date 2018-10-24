using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using SmartKioskBot.Controllers;
using SmartKioskBot.Models;
using SmartKioskBot.UI;
using static SmartKioskBot.Models.Context;
using SmartKioskBot.Logic;
using MongoDB.Driver;
using SmartKioskBot.Helpers;
using MongoDB.Bson;
using static SmartKioskBot.Helpers.Constants;
using static SmartKioskBot.Helpers.AdaptiveCardHelper;
using Newtonsoft.Json.Linq;

namespace SmartKioskBot.Dialogs 
{
    [Serializable]
    public class RecommendationDialog : IDialog<object>
    {
        private ObjectId lastFetchId;
        private int page = 1;
        private State state;

        public enum State { INIT, INPUT_HANDLE}

        public RecommendationDialog(State state)
        {
            this.state = state;
        }

        public async Task StartAsync(IDialogContext context)
        {
            switch (state)
            {
                case State.INIT:
                    await ShowRecommendations(context, null);
                    break;
                case State.INPUT_HANDLE:
                    context.Wait(InputHandler);
                    break;
            }
        }

        private async Task ShowRecommendations(IDialogContext context, IAwaitable<object> result)
        {
            List<Product> products = new List<Product>();
            List<Filter> popular = RecommendationsLogic.GetPopularFilters(StateHelper.GetFiltersCount(context));

            while (true)
            {
                FilterDefinition<Product> joinedFilters = FilterLogic.GetJoinedFilter(popular);

                //fetch +1 product to see if pagination is needed
                products = ProductController.getProductsFilter(
                joinedFilters,
                Constants.N_ITEMS_CARROUSSEL + 1,
                this.lastFetchId);

                //filters didn't retrieved any products at the first try
                if (products.Count == 0 && lastFetchId == null)
                    popular.RemoveAt(popular.Count - 1);
                else
                    break;
            }

            if (StateHelper.IsLoggedIn(context))
            {
                User contextUser = StateHelper.GetUser(context);

                // Add recommendations based on Wish List products
                ObjectId[] wishes = ContextController.GetContext(contextUser.Id).WishList;

                foreach (ObjectId obj in wishes)
                {
                    List<Product> l = Logic.RecommendationsLogic.GetSimilarProducts(obj);
                    products.InsertRange(0, l);
                }
            }

            if (products.Count > Constants.N_ITEMS_CARROUSSEL) 
                lastFetchId = products[products.Count - 2].Id;
            
            var reply = context.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            List<Attachment> cards = new List<Attachment>();

            for (int i = 0; i < products.Count && i < Constants.N_ITEMS_CARROUSSEL; i++)
                cards.Add(ProductCard.GetProductCard(products[i], ProductCard.CardType.RECOMMENDATION).ToAttachment());

            reply.Attachments = cards;
            await context.PostAsync(reply);

            //Check if pagination is needed
            if (products.Count <= Constants.N_ITEMS_CARROUSSEL)
                context.Done(new CODE(DIALOG_CODE.DONE));
            else
            {
                reply = context.MakeMessage();
                
                reply.Attachments.Add(
                    getCardButtonsAttachment(new List<ButtonType> { ButtonType.PAGINATION },DialogType.RECOMMENDATION));
                await context.PostAsync(reply);
                

                context.Wait(this.InputHandler);
            }
        }

        public async Task InputHandler(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var activity = await argument as Activity;

            //message
            if (activity.Text != null)
                context.Done(new CODE(DIALOG_CODE.PROCESS_LUIS, activity));
            //event 
            else if (activity.Value != null)
                await EventHandler(context, activity);
            else
                context.Done(new CODE(DIALOG_CODE.DONE));
        }

        public async Task EventHandler(IDialogContext context, Activity activity)
        {
            JObject json = activity.Value as JObject;
            List<InputData> data = getReplyData(json);

            //have mandatory info
            if (data.Count >= 2)
            {
                //json structure is correct
                if (data[0].attribute == REPLY_ATR && data[1].attribute == DIALOG_ATR)
                {
                    ClickType click = getClickType(data[0].value);

                    if (data[1].value.Equals(getDialogName(DialogType.RECOMMENDATION)) &&
                        click != ClickType.NONE)
                    {
                        switch (click)
                        {
                            case ClickType.PAGINATION:
                                {
                                    page++;
                                    state = State.INIT;
                                    await ShowRecommendations(context, null);
                                    break;
                                }
                        }
                    }
                    else
                        context.Done(new CODE(DIALOG_CODE.PROCESS_EVENT, activity));
                }
                else
                    context.Done(new CODE(DIALOG_CODE.DONE));
            }
            else
                context.Done(new CODE(DIALOG_CODE.DONE));
        }
    }
}