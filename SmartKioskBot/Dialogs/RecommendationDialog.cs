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
        private List<Filter> filtersApplied;
        private User user;
        private ObjectId lastFetchId;
        private int page = 1;

        public RecommendationDialog(User user)
        {
            this.user = user;

            //recommendation type
            this. filtersApplied = new List<Filter>(CRMController.GetMostPopularFilters(user.Id, Constants.MAX_N_FILTERS_RECOMM));

            //Default
            if (filtersApplied == null || filtersApplied.Count == 0)
                this.filtersApplied.Add(FilterLogic.DEFAULT_RECOMMENDATION_FILTER);
        }

        public async Task StartAsync(IDialogContext context)
        {
            await ShowRecommendations(context, null);
        }

        private async Task ShowRecommendations(IDialogContext context, IAwaitable<object> result)
        {
            List<Product> products = new List<Product>();

            while (true)
            {
                FilterDefinition<Product> joinedFilters = FilterLogic.GetJoinedFilter(this.filtersApplied);

                //fetch +1 product to see if pagination is needed
                products = ProductController.getProductsFilter(
                joinedFilters,
                Constants.N_ITEMS_CARROUSSEL + 1,
                this.lastFetchId);

                //filters didn't retrieved any products at the first try
                if (products.Count == 0 && lastFetchId == null)
                    filtersApplied.RemoveAt(filtersApplied.Count - 1);
                else
                    break;
            }
            
            if(products.Count > Constants.N_ITEMS_CARROUSSEL) 
                lastFetchId = products[products.Count - 2].Id;
            
            var reply = context.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            List<Attachment> cards = new List<Attachment>();

            for (int i = 0; i < products.Count && i < Constants.N_ITEMS_CARROUSSEL; i++)
            {
                cards.Add(ProductCard.GetProductCard(products[i], ProductCard.CardType.RECOMMENDATION).ToAttachment());
            }

            reply.Attachments = cards;
            
            await context.PostAsync(reply);

            //Check if pagination is needed
            if (products.Count <= Constants.N_ITEMS_CARROUSSEL)
                context.Done(new CODE(DIALOG_CODE.DONE));
            else
            {
                reply = context.MakeMessage();
                
                reply.Attachments.Add(await getCardAttachment(CardType.PAGINATION));
                await context.PostAsync(reply);
                

                context.Wait(this.InputHandler);
            }
        }

        public async Task InputHandler(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var activity = await argument as Activity;

            if (activity.Text != null)
            {
                if (activity.Text == BotDefaultAnswers.next_pagination)
                    context.Done(new CODE(DIALOG_CODE.DONE));
                else
                    context.Done(new CODE(DIALOG_CODE.PROCESS_LUIS, activity as IMessageActivity));
            }
            else if (activity.Value != null)
            {
                JObject json = activity.Value as JObject;
                CardType type = getCardTypeReply(json);

                switch (type)
                {
                    case CardType.PAGINATION:
                        {
                            page++;
                            await ShowRecommendations(context,null);
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

    }
}