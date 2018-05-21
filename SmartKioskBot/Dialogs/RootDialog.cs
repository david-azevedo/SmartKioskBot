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

namespace SmartKioskBot.Dialogs
{
    [LuisModel(AppSettings.LuisAppId, AppSettings.LuisSubscriptionKey, domain: AppSettings.LuisDomain)]
    [Serializable]
    public sealed class RootDialog : LuisDialog<object>
    {
        // We will only accept intents which score higher than this value
        private static double INTENT_SCORE_THRESHOLD = 0.4;

        private User user;
        private bool identified = false;

        public RootDialog()
        {
            identified = false;
        }

        //ATENÇÃO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // CHAMAR ESTA FUNÇÃO NO INICIO DE CADA INTENT
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

                context.Done<object>(null);
            }
        }

        //ATENÇÃO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!1
        //USAR ISTO EM VEZ DE CONTEXT.WAIT NO FINAL
        private void TryIdentification(IDialogContext context)
        {
            if (identified == false)
            {
                var activity = context.Activity;
                user = UserController.getUser(activity.ChannelId);
                if (user == null)
                {
                    var r = new Random();
                    UserController.CreateUser(activity.ChannelId, activity.From.Id, activity.From.Name, (r.Next(25) + 1).ToString());
                    user = UserController.getUser(activity.ChannelId);
                    ContextController.CreateContext(user);
                    CRMController.AddCustomer(user);
                    context.Call(new IdentificationDialog(), ResumeAfterIdent);
                }
                identified = true;
            }
        }

        private async Task ResumeAfterIdent(IDialogContext context, IAwaitable<object> result)
        {
            this.user = UserController.getUser(context.Activity.ChannelId);
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("")]
        [LuisIntent("None")]
        //Not yet processed
        [LuisIntent("Confirmation")]
        [LuisIntent("Identification")]
        [LuisIntent("Negation")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            TryIdentification(context);
            FilterIntentScore(context, result);

            string message = "Desculpa mas não entendi aquilo que disseste. Podes refrasear por favor? :)";
            await context.PostAsync(message);

            context.Done<object>(null);
        }

        /*
         * Others
         */

        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            TryIdentification(context);
            FilterIntentScore(context, result);
            
            var reply = context.MakeMessage();
            reply.Text = BotDefaultAnswers.getGreeting(context.Activity.From.Name);
            await Helpers.BotTranslator.PostTranslated(context, reply, reply.Locale);
            context.Done<object>(null);
        }

        /*
         * Wish List
         */

        [LuisIntent("ViewWishList")]
        public async Task ViewWishList(IDialogContext context, LuisResult result)
        {
            TryIdentification(context);
            FilterIntentScore(context, result);

            var reply = WishListDialog.ViewWishList(context, ContextController.GetContext(user.Id));
            await Helpers.BotTranslator.PostTranslated(context, reply, context.MakeMessage().Locale);
            context.Done<object>(null);
        }
        
        [LuisIntent("AddWishList")]
        public async Task AddWishList(IDialogContext context, LuisResult result)
        {
            TryIdentification(context);
            FilterIntentScore(context, result);

            WishListDialog.AddToWishList(result.Query, user);
            var reply = BotDefaultAnswers.getAddWishList();
            await Helpers.BotTranslator.PostTranslated(context, reply, context.MakeMessage().Locale);
            context.Done<object>(null);
        }
        [LuisIntent("RmvWishList")]
        public async Task RmvWishList(IDialogContext context, LuisResult result)
        {
            TryIdentification(context);
            FilterIntentScore(context, result);

            WishListDialog.RemoveFromWishList(result.Query, user);
            var reply = BotDefaultAnswers.getRemWishList();
            await Helpers.BotTranslator.PostTranslated(context, reply, context.MakeMessage().Locale);
            context.Done<object>(null);
        }

        /*
         * Comparator
         */

        [LuisIntent("AddComparator")]
        public async Task AddComparator(IDialogContext context, LuisResult result)
        {
            TryIdentification(context);
            FilterIntentScore(context, result);

            CompareDialog.AddComparator(context, result.Query);

            context.Done<object>(null);
        }

        [LuisIntent("RmvComparator")]
        public async Task RmvComparator(IDialogContext context, LuisResult result)
        {
            TryIdentification(context);
            FilterIntentScore(context, result);

            CompareDialog.RmvComparator(context, result.Query);

            context.Done<object>(null);
        }

        [LuisIntent("ViewComparator")]
        public async Task ViewComparator(IDialogContext context, LuisResult result)
        {
            TryIdentification(context);
            FilterIntentScore(context, result);

            await CompareDialog.ViewComparator(context);

            context.Done<object>(null);
        }


        /*
         * Filter
         */

        [LuisIntent("Filter")]
        public async Task Filter(IDialogContext context, LuisResult result)
        {
            TryIdentification(context);
            FilterIntentScore(context, result);

            IMessageActivity r = FilterDialog.Filter(context, this.user, ContextController.GetContext(user.Id), result);
            await Helpers.BotTranslator.PostTranslated(context, r, context.MakeMessage().Locale);
            context.Done<object>(null);
        }

        [LuisIntent("CleanAllFilters")]
        public async Task CleanAllFilters(IDialogContext context, LuisResult result)
        {
            TryIdentification(context);
            FilterIntentScore(context, result);

            var reply = FilterDialog.CleanAllFilters(context, user);
            await Helpers.BotTranslator.PostTranslated(context, reply, context.MakeMessage().Locale);
            context.Done<object>(null);
        }

        [LuisIntent("RmvFilter")]
        public async Task RmvFilter(IDialogContext context, LuisResult result)
        {
            TryIdentification(context);
            FilterIntentScore(context, result);

            var reply = FilterDialog.CleanFilter(context, this.user, ContextController.GetContext(user.Id), result.Entities);
            await Helpers.BotTranslator.PostTranslated(context, reply, context.MakeMessage().Locale);
            context.Done<object>(null);
        }

        /*
         * Product
         */
         [LuisIntent("ViewProductDetails")]
        public async Task ViewProductDetails(IDialogContext context, LuisResult result)
        {
            TryIdentification(context);
            FilterIntentScore(context, result);

            var idx = result.Query.LastIndexOf(":");
            string id = result.Query.Remove(0, idx + 1).Replace(" ", "");

            // Add click to CRM
            CRMController.AddProductClick(this.user.Id, this.user.Country, ObjectId.Parse(id));

            await ProductDetails.ShowProductMessage(context, id);
            context.Done<object>(null);
        }

        [LuisIntent("StockInStore")]
        public async Task StockInStore(IDialogContext context, LuisResult result)
        {
            TryIdentification(context);
	    FilterIntentScore(context, result);

            var idx = result.Query.LastIndexOf(":");
            string id = result.Query.Remove(0, idx + 1).Replace(" ", "");

            //hero card
            IMessageActivity r = StockDialog.ShowStores(context, id);
            await context.PostAsync(r);
            context.Done<object>(null);
        }

        /*
         * Store Information
         */
        [LuisIntent("InStoreLocation")]
        public async Task InStoreLocation(IDialogContext context, LuisResult result)
        {
            TryIdentification(context);
            FilterIntentScore(context, result);

            var args = result.Query.Split(':');
            string productId = args[1];
            string storeId = args[2];
            await ProductDetails.ShowInStoreLocation(context, productId, storeId);
            context.Done<object>(null);

        }

        [LuisIntent("ClosestStores")]
        public async Task ClosestStores(IDialogContext context, LuisResult result)
        {
            TryIdentification(context);
            FilterIntentScore(context, result);

            //simulate user position
            Random r = new Random();
            Double[] coords = new Double[]{
                r.NextDouble() * 180 - 90,
                r.NextDouble() * 180 - 90
             };

            await ClosestStoresDialog.ShowClosestStores(context, coords, 3);
            context.Done<object>(null);
        }

        [LuisIntent("Recommendation")]
        public async Task Recommendation(IDialogContext context, LuisResult result)
        {
            try
            {
                TryIdentification(context);
                FilterIntentScore(context, result);

                var reply = RecommendationDialog.ShowRecommendations(context, this.user);
                await Helpers.BotTranslator.PostTranslated(context, reply, context.MakeMessage().Locale);

                context.Done<object>(null);
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
    }
}
