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
using SmartKioskBot.Helpers;

namespace SmartKioskBot.Dialogs
{
    [LuisModel(AppSettings.LuisAppId, AppSettings.LuisSubscriptionKey, domain: AppSettings.LuisDomain)]
    [Serializable]
    public sealed class LuisProcessingDialog : LuisDialog<object>
    {
        public LuisProcessingDialog()
        {
        }

        private async void FilterIntentScore(IDialogContext context, LuisResult result) {
            if (result.TopScoringIntent.Score < INTENT_SCORE_THRESHOLD)
            {
                // Chamar QnA Maker
                QnAMakerResult qnaResult = await QnADialog.MakeRequest(result.Query);

                if (qnaResult != null && result.TopScoringIntent.Score >= INTENT_SCORE_THRESHOLD &&
                    qnaResult.Score >= INTENT_SCORE_THRESHOLD)
                    await context.PostAsync(qnaResult.Answer);
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

        [LuisIntent("Guidance")]
        public async Task Guidance(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);
            context.Call(new MenuDialog(MenuDialog.State.INIT), ResumeAfterDialogueCall);
        }

        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            var reply = context.MakeMessage();
            reply.Text = BotDefaultAnswers.getGreeting(context.Activity.From.Name);
            await Helpers.BotTranslator.PostTranslated(context, reply, reply.Locale);
            context.Done(new CODE(DIALOG_CODE.DONE));
        }

        /*
         * Wish List
         */

        [LuisIntent("ViewWishList")]
        public async Task ViewWishList(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            FilterIntentScore(context, result);
            context.Call(new WishListDialog(WishListDialog.State.INIT),ResumeAfterDialogueCall);
        }

        [LuisIntent("AddWishList")]
        public async Task AddWishList(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);

            WishListDialog.AddToWishList(context, result.Query);
            var reply = BotDefaultAnswers.getAddWishList();
            await Helpers.BotTranslator.PostTranslated(context, reply, context.MakeMessage().Locale);
            context.Done(new CODE(DIALOG_CODE.DONE));
        }

        [LuisIntent("RmvWishList")]
        public async Task RmvWishList(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);

            WishListDialog.RemoveFromWishList(context, result.Query);
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
            context.Call(new CompareDialog(CompareDialog.State.INIT), 
                ResumeAfterDialogueCall);
        }


        /*
         * Filter
         */

        [LuisIntent("Filter")]
        public async Task Filter(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            FilterIntentScore(context, result);

            List<Filter> filter_luis = FilterLogic.GetEntitiesFilter(result);

            foreach (Filter f in filter_luis)
            {
                if (StateHelper.AddFilter(f, context) == true)
                    StateHelper.AddFilterCount(context, f);
            }

            FilterDialog.State state = FilterDialog.State.INIT;

            if (filter_luis.Count != 0)
                state = FilterDialog.State.FILTER;
            else
                state = FilterDialog.State.INIT;

            context.Call(new FilterDialog(state), ResumeAfterDialogueCall);
        }

        [LuisIntent("CleanAllFilters")]
        public async Task CleanAllFilters(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);

            var reply = FilterDialog.CleanAllFilters(context);
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
            
            StateHelper.AddProductClick(context, id);

            await ProductLogic.ShowProductMessage(context, id);
            context.Done(new CODE(DIALOG_CODE.DONE));
        }

        [LuisIntent("StockInStore")]
        public async Task StockInStore(IDialogContext context, LuisResult result)
        {
	        FilterIntentScore(context, result);

            var idx = result.Query.LastIndexOf(":");
            string id = result.Query.Remove(0, idx + 1).Replace(" ", "");

            //hero card
            IMessageActivity r = StoreDialog.ShowStores(context, id);
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
            await ProductLogic.ShowInStoreLocation(context, productId, storeId);
            context.Done(new CODE(DIALOG_CODE.DONE));
        }

        [LuisIntent("ClosestStores")]
        public async Task ClosestStores(IDialogContext context, LuisResult result)
        {
            await StoreDialog.ShowClosestStores(context);
            context.Done(new CODE(DIALOG_CODE.DONE));
        }

        /*
         * RECOMMENDATIONS
         */

        [LuisIntent("Recommendation")]
        public async Task Recommendation(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);
            context.Call(new RecommendationDialog(RecommendationDialog.State.INIT), ResumeAfterDialogueCall);
        }

        /*
        * View Account
        */

        [LuisIntent("ViewAccount")]
        public async Task ViewAccount(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            FilterIntentScore(context, result);
            context.Call(new AccountDialog(AccountDialog.State.INIT), ResumeAfterDialogueCall);
        }
    }
}
