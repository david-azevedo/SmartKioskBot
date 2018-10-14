using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using MongoDB.Bson;
using SmartKioskBot.Controllers;
using SmartKioskBot.Dialogs.QnA;
using SmartKioskBot.Models;
using System.Collections.Generic;
using static SmartKioskBot.Models.Context;
using SmartKioskBot.Logic;
using static SmartKioskBot.Helpers.Constants;

namespace SmartKioskBot.Dialogs
{
    [LuisModel(AppSettings.LuisAppId, AppSettings.LuisSubscriptionKey, domain: AppSettings.LuisDomain)]
    [Serializable]
    public sealed class LuisProcessingDialog : LuisDialog<object>
    {
        private User user = null;

        public LuisProcessingDialog(User user)
        {
            this.user = user;
        }

        private async void FilterIntentScore(IDialogContext context, LuisResult result) {
            if (result.TopScoringIntent.Score < INTENT_SCORE_THRESHOLD)
            {
                // Chamar QnA Maker
                QnAMakerResult qnaResult = await QnADialog.MakeRequest(result.Query);

                if (qnaResult != null && result.TopScoringIntent.Score >= INTENT_SCORE_THRESHOLD &&
                    qnaResult.Score >= INTENT_SCORE_THRESHOLD)
                {
                    await context.PostAsync(qnaResult.Answer);
                }
                else
                {
                    string message = "Desculpa mas não entendi aquilo que disseste. Podes refrasear por favor? :)";
                    await context.PostAsync(message);
                }
            }
        }

        private async Task ResumeAfterDialogueCall(IDialogContext context, IAwaitable<object> result)
        {
            //var tmp = result;
            //await Helpers.BotTranslator.PostTranslated(context, tmp.ToString(), context.MakeMessage().Locale);

            CODE code = await result as CODE;
            context.Done(code);
        }

        [LuisIntent("")]
        [LuisIntent("None")]
        //Not yet processed
        [LuisIntent("Confirmation")]
        [LuisIntent("Identification")]
        [LuisIntent("Negation")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);

            string message = BotDefaultAnswers.unknown_intention;
            await context.PostAsync(message);

            context.Done(new CODE(DIALOG_CODE.DONE));
        }

        /*
         * Others
         */

        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            //just for test
            FilterIntentScore(context, result);
            context.Call(new MenuDialog(user), ResumeAfterDialogueCall);
            
           /* var reply = context.MakeMessage();
            reply.Text = BotDefaultAnswers.getGreeting(context.Activity.From.Name);
            await Helpers.BotTranslator.PostTranslated(context, reply, reply.Locale);
            context.Done(new CODE(DIALOG_CODE.DONE));*/
        }

        /*
         * Wish List
         */

        [LuisIntent("ViewWishList")]
        public async Task ViewWishList(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            FilterIntentScore(context, result);

            context.Call(new WishListDialog(user),ResumeAfterDialogueCall);
        }


        [LuisIntent("AddWishList")]
        public async Task AddWishList(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);

            WishListDialog.AddToWishList(result.Query, user);
            var reply = BotDefaultAnswers.getAddWishList();
            await Helpers.BotTranslator.PostTranslated(context, reply, context.MakeMessage().Locale);
            context.Done(new CODE(DIALOG_CODE.DONE));
        }
        [LuisIntent("RmvWishList")]
        public async Task RmvWishList(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);

            WishListDialog.RemoveFromWishList(result.Query, user);
            var reply = BotDefaultAnswers.getRemWishList();
            await Helpers.BotTranslator.PostTranslated(context, reply, context.MakeMessage().Locale);
            context.Done(new CODE(DIALOG_CODE.DONE));
        }

        /*
         * Comparator
         */

        [LuisIntent("AddComparator")]
        public async Task AddComparator(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);

            await CompareDialog.AddComparator(context, result.Query);

            context.Done(new CODE(DIALOG_CODE.DONE));
        }

        [LuisIntent("RmvComparator")]
        public async Task RmvComparator(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);

            await CompareDialog.RmvComparator(context, result.Query);

            context.Done(new CODE(DIALOG_CODE.DONE));
        }

        [LuisIntent("ViewComparator")]
        public async Task ViewComparator(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);

            await CompareDialog.ViewComparator(context);

            context.Done(new CODE(DIALOG_CODE.DONE));
        }


        /*
         * Filter
         */

        [LuisIntent("Filter")]
        public async Task Filter(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            FilterIntentScore(context, result);
            
            List<Filter> luis_filters = FilterLogic.GetEntitiesFilter(result); 
            context.Call(new FilterDialog(user,luis_filters,FilterDialog.State.FILTER_PREVIOUS), ResumeAfterDialogueCall);
        }

        [LuisIntent("CleanAllFilters")]
        public async Task CleanAllFilters(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);

            var reply = FilterDialog.CleanAllFilters(context, user);
            await Helpers.BotTranslator.PostTranslated(context, reply, context.MakeMessage().Locale);
            context.Done(new CODE(DIALOG_CODE.DONE));
        }

        [LuisIntent("RmvFilter")]
        public async Task RmvFilter(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);

            var reply = FilterDialog.CleanFilter(context, this.user, ContextController.GetContext(user.Id), result.Entities);
            await Helpers.BotTranslator.PostTranslated(context, reply, context.MakeMessage().Locale);
            context.Done(new CODE(DIALOG_CODE.DONE));
        }

        /*
         * Product
         */
         [LuisIntent("ViewProductDetails")]
        public async Task ViewProductDetails(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);

            var idx = result.Query.LastIndexOf(":");
            string id = result.Query.Remove(0, idx + 1).Replace(" ", "");

            // Add click to CRM
            CRMController.AddProductClick(this.user.Id, this.user.Country, ObjectId.Parse(id));

            await ProductDetails.ShowProductMessage(context, id);
            context.Done(new CODE(DIALOG_CODE.DONE));
        }

        [LuisIntent("StockInStore")]
        public async Task StockInStore(IDialogContext context, LuisResult result)
        {
	        FilterIntentScore(context, result);

            var idx = result.Query.LastIndexOf(":");
            string id = result.Query.Remove(0, idx + 1).Replace(" ", "");

            //hero card
            IMessageActivity r = StockDialog.ShowStores(context, id);
            await context.PostAsync(r);
            context.Done(new CODE(DIALOG_CODE.DONE));
        }

        /*
         * Store Information
         */
        [LuisIntent("InStoreLocation")]
        public async Task InStoreLocation(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);

            var args = result.Query.Split(':');
            string productId = args[1];
            string storeId = args[2];
            await ProductDetails.ShowInStoreLocation(context, productId, storeId);
            context.Done(new CODE(DIALOG_CODE.DONE));
        }

        [LuisIntent("ClosestStores")]
        public async Task ClosestStores(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);

            //simulate user position
            Random r = new Random();
            Double[] coords = new Double[]{
                r.NextDouble() * 180 - 90,
                r.NextDouble() * 180 - 90
             };

            await ClosestStoresDialog.ShowClosestStores(context, coords, 3);
            context.Done(new CODE(DIALOG_CODE.DONE));
        }

        [LuisIntent("Recommendation")]
        public async Task Recommendation(IDialogContext context, LuisResult result)
        {
            try
            {
                FilterIntentScore(context, result);
                context.Call(new RecommendationDialog(user), ResumeAfterDialogueCall);
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
    }
}
