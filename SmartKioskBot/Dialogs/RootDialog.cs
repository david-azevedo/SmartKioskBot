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
        private static double INTENT_SCORE_THRESHOLD = 0.6;

        private User user;
        private bool identification;

        public RootDialog(Activity activity)
        {
            user = UserController.getUser(activity.ChannelId);
            if (user == null)
                identification = false;
            else
                identification = true;
        }

        //ATENÇÃO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // CHAMAR ESTA FUNÇÃO NO INICIO DE CADA INTENT
        private async void FilterIntentScore(IDialogContext context, LuisResult result) {
            if (result.TopScoringIntent.Score < INTENT_SCORE_THRESHOLD)
            {
                await None(context, result);
            }
        }

        //ATENÇÃO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!1
        //USAR ISTO EM VEZ DE CONTEXT.WAIT NO FINAL
        private void Next(IDialogContext context)
        {
            if (!identification)
                context.Call(new IdentificationDialog(), ResumeAfterIdent);
            else
                 context.Wait(this.MessageReceived);
        }

        private async Task ResumeAfterIdent(IDialogContext context, IAwaitable<object> result)
        {
            this.user = UserController.getUser(context.Activity.ChannelId);
            this.identification = true;
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            // Chamar QnA Maker
            QnAMakerResult qnaResult = await QnADialog.MakeRequest(result.Query);
            
            if (qnaResult != null)
            {
                await context.PostAsync(qnaResult.Answer);
            }
            else {
                string message = $"Sorry, I did not understand '{result.Query}'. Type 'help' if you need assistance.";
                await context.PostAsync(message);
            }

            Next(context);
        }

        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);
            
            var reply = context.MakeMessage();
            reply.Text = BotDefaultAnswers.getGreeting(context.Activity.From.Name);
            await Helpers.BotTranslator.PostTranslated(context, reply, reply.Locale);
            Next(context);
        }

        /*
         * Wish List
         */

        [LuisIntent("ViewWishList")]
        public async Task ViewWishList(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);
            var reply = WishListDialog.ViewWishList(context, ContextController.GetContext(user.Id));
            await Helpers.BotTranslator.PostTranslated(context, reply, context.MakeMessage().Locale);
            Next(context);
        }
        
        [LuisIntent("AddWishList")]
        public async Task AddWishList(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);

            WishListDialog.AddToWishList(result.Query, user);
            var reply = BotDefaultAnswers.getAddWishList();
            await Helpers.BotTranslator.PostTranslated(context, reply, context.MakeMessage().Locale);
            Next(context);
        }
        [LuisIntent("RmvWishList")]
        public async Task RmvWishList(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);

            WishListDialog.RemoveFromWishList(result.Query, user);
            var reply = BotDefaultAnswers.getRemWishList();
            await Helpers.BotTranslator.PostTranslated(context, reply, context.MakeMessage().Locale);
            Next(context);
        }

        /*
         * Filter
         */

        [LuisIntent("Filter")]
        public async Task Filter(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);

            IMessageActivity r = FilterDialog.Filter(context, this.user, ContextController.GetContext(user.Id), result);
            await Helpers.BotTranslator.PostTranslated(context, r, context.MakeMessage().Locale);
            Next(context);
        }

        [LuisIntent("CleanAllFilters")]
        public async Task CleanAllFilters(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);

            var reply = FilterDialog.CleanAllFilters(context, user);
            await Helpers.BotTranslator.PostTranslated(context, reply, context.MakeMessage().Locale);
            Next(context);
        }

        [LuisIntent("RmvFilter")]
        public async Task RmvFilter(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);
            var reply = FilterDialog.CleanFilter(context, this.user, ContextController.GetContext(user.Id), result.Entities);
            await Helpers.BotTranslator.PostTranslated(context, reply, context.MakeMessage().Locale);
            Next(context);
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
            Next(context);
        }

        /*
         * Stock
         */
        [LuisIntent("StockInStore")]
        public async Task StockInStore(IDialogContext context, LuisResult result)
        {
            var idx = result.Query.LastIndexOf(":");
            string id = result.Query.Remove(0, idx + 1).Replace(" ", "");

            //hero card
            IMessageActivity r = StockDialog.ShowStores(context, id);
            await context.PostAsync(r);
            Next(context);
        }

        
        [LuisIntent("Recommendation")]
        public async Task Recommendation(IDialogContext context, LuisResult result)
        {
            try
            {
                FilterIntentScore(context, result);

                var reply = RecommendationDialog.ShowRecommendations(context, this.user);
                await Helpers.BotTranslator.PostTranslated(context, reply, context.MakeMessage().Locale);

                Next(context);
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        /*
        [LuisIntent("StoreLocation")]
        public void StoreLocation(IDialogContext context, LuisResult result)
        {
            FilterIntentScore(context, result);
            
            Next(context);
        }

        private Task Done(IDialogContext context, IAwaitable<object> result)
        {

            context.Done<object>(null);
            return null;
        }*/

        /* [LuisIntent("Recommendation")]
         public void Recommendation(IDialogContext context, LuisResult result)
         {
            FilterIntentScore(context, result);
            
            Next(context);
         }*/

        /* [LuisIntent("StoreLocation")]
         public void StoreLocation(IDialogContext context, LuisResult result)
         {
            FilterIntentScore(context, result);
            
            Next(context);
         }*/

        /* [LuisIntent("ViewWishList")]
         public void ViewWishList(IDialogContext context, LuisResult result)
         {
            FilterIntentScore(context, result);
            
            FilterDialog w = new FilterDialog();
            IMessageActivity r = w.filtering(result.Entities, context.MakeMessage());

            Next(context);
         }*/


        /*
        var message = await activity as Activity;


        // Get the command, or the first word, that the user typed in.
        var userInput = message.Text != null ? message.Text : "";
        var command = (userInput.Split(new[] { ' ' }, 3))[0];


        reply.Text = "Comandos:\n\n" +
                "filter [marca/preço/nome] [operator] [valor] \n\n" +
                "filter-rem [filter] \n\n" + 
                "filter-clean \n\n" + 
                "wishlist\n\n";
            await context.PostAsync(reply);
        }
        //ADD PRODUCT TO DB (TESTING)
        else if(details[0].Equals("add", StringComparison.CurrentCultureIgnoreCase))
        {
            await context.Forward(new AddProductDialog(), this.StartAsync, message, CancellationToken.None);
        }
        //FILTER PRODUCT
        else if (details[0].Equals("filter", StringComparison.CurrentCultureIgnoreCase))
        {
            await context.Forward(new FilterDialog(), this.StartAsync, message, CancellationToken.None);
        }
        //FILTER REMOVE
        else if (details[0].Equals("filter-rem", StringComparison.CurrentCultureIgnoreCase))
        {
            await context.Forward(new FilterDialog(), this.StartAsync, message, CancellationToken.None);
        }
        //FILTER CLEAN
        else if (details[0].Equals("filter-clean", StringComparison.CurrentCultureIgnoreCase))
        {
            await context.Forward(new FilterDialog(), this.StartAsync, message, CancellationToken.None);
        }
        //PRODUCT DETAILS
        else if(details[0].Equals(BotDefaultAnswers.show_product_details, StringComparison.CurrentCultureIgnoreCase))
        {
            await context.Forward(new ProductDetails(), this.StartAsync, message, CancellationToken.None);
        }
        //VIEW WISH LIST
        else if (details[0].Equals("wishlist", StringComparison.CurrentCultureIgnoreCase))
        {
            await context.Forward(new WishListDialog(WishListDialog.Action.VIEW), this.StartAsync, message, CancellationToken.None);
        }
        //ADD PRODUCT TO WISH LIST
        else if (details[0].Equals(BotDefaultAnswers.add_wish_list, StringComparison.CurrentCultureIgnoreCase))
        {
            await context.Forward(new WishListDialog(WishListDialog.Action.ADD), this.StartAsync,message, CancellationToken.None);
        }
        //REM PRODUCT FROM WISH LIST
        else if (details[0].Equals(BotDefaultAnswers.rem_wish_list, StringComparison.CurrentCultureIgnoreCase))
        {
            await context.Forward(new WishListDialog(WishListDialog.Action.REM), this.StartAsync, message, CancellationToken.None);
        }
        //ADD PRODUCT TO COMPARATOR
        else if (details[0].Equals(BotDefaultAnswers.add_to_comparator, StringComparison.CurrentCultureIgnoreCase))
        {
            //TODO
        }
        //ADD CUSTOMER CARD
        else if (details[0].Equals("set-customer-card", StringComparison.CurrentCultureIgnoreCase))
        {
            await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
        }
        //ADD CUSTOMER EMAIL
        else if (details[0].Equals("set-customer-email", StringComparison.CurrentCultureIgnoreCase))
        {
            await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
        }
        //IDENTIFICATION BY CUSTOMER CARD
        else if (details[0].Equals("customer-card", StringComparison.CurrentCultureIgnoreCase))
        {
            await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
        }
        //IDENTIFICATION BY EMAIL
        else if (details[0].Equals("customer-email", StringComparison.CurrentCultureIgnoreCase))
        {
            await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
        }
        //FIRST DIALOG
        else if (details[0].Equals("first-dialog", StringComparison.CurrentCultureIgnoreCase))
        {
            await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
        }
        //CUSTOMER INFO
        else if (details[0].Equals("customer-info", StringComparison.CurrentCultureIgnoreCase))
        {
            await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
        }
        //SET CUSTOMER NAME
        else if (details[0].Equals("set-customer-name", StringComparison.CurrentCultureIgnoreCase))
        {
            await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
        }
        //SET CUSTOMER COUNTRY
        else if (details[0].Equals("set-customer-country", StringComparison.CurrentCultureIgnoreCase))
        {
            await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
        }
        //ADD CUSTOMER CARD CONFIRMATION
        else if (details[0].Equals("SaveCard", StringComparison.CurrentCultureIgnoreCase))
        {
            await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
        }
        //ADD CUSTOMER EMAIL CONFIRMATION
        else if (details[0].Equals("SaveEmail", StringComparison.CurrentCultureIgnoreCase))
        {
            await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
        }
        //ADD CHANNEL CONFIRMATION
        else if (details[0].Equals("AddChannel", StringComparison.CurrentCultureIgnoreCase))
        {
            await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
        }
        //ADD CUSTOMER NAME 
        else if (details[0].Equals("SaveName", StringComparison.CurrentCultureIgnoreCase))
        {
            await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
        }
        //ADD CUSTOMER COUNTRY 
        else if (details[0].Equals("SaveCountry", StringComparison.CurrentCultureIgnoreCase))
        {
            await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
        }
        else
        {
            var reply = context.MakeMessage();
            */
    }
}