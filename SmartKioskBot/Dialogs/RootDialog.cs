using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using MongoDB.Driver;
using SmartKioskBot.Controllers;
using SmartKioskBot.Helpers;
using SmartKioskBot.Models;

namespace SmartKioskBot.Dialogs
{
    [LuisModel(AppSettings.LuisAppId, AppSettings.LuisSubscriptionKey, domain: AppSettings.LuisDomain)]
    [Serializable]
    public sealed class RootDialog : LuisDialog<object>
    {
        private User user;
        private Context context;

        public RootDialog(string channelId)
        {
            //fetch context
            this.user = UserController.getUser(channelId);
            this.context = ContextController.GetContext(user.Id);
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'. Type 'help' if you need assistance.";
            await context.PostAsync(message);
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            var reply = context.MakeMessage();
            reply.Text = BotDefaultAnswers.getGreeting(context.Activity.From.Name);
            await context.PostAsync(reply);
            context.Wait(this.MessageReceived);
        }

        /*
         * Wish List
         */

        [LuisIntent("ViewWishList")]
        public async Task ViewWishList(IDialogContext context, LuisResult result)
        {
            await context.PostAsync(WishListDialog.ViewWishList(context, this.context));
            context.Wait(MessageReceived);
        }
       [LuisIntent("AddWishList")]
        public async Task AddWishList(IDialogContext context, LuisResult result)
        {
            WishListDialog.AddToWishList(result.Query, user);
            await context.PostAsync(BotDefaultAnswers.getAddWishList());
            context.Wait(MessageReceived);
        }
        [LuisIntent("RmvWishList")]
        public async Task RmvWishList(IDialogContext context, LuisResult result)
        {
            WishListDialog.RemoveFromWishList(result.Query, user);
            await context.PostAsync(BotDefaultAnswers.getRemWishList());
            context.Wait(MessageReceived);
        }

        /*
         * Filter
         */

        [LuisIntent("Filter")]
        public async Task Filter(IDialogContext context, LuisResult result)
        {
            FilterDialog w = new FilterDialog();
            IMessageActivity r = w.filtering(result.Entities, context.MakeMessage());
            await context.PostAsync(r);
            context.Wait(MessageReceived);
        }

        [LuisIntent("CleanAllFilters")]
        public async Task CleanAllFilters(IDialogContext context, LuisResult result)
        {
            await context.PostAsync(FilterDialog.CleanAllFilters(context,user));
            context.Wait(MessageReceived);
        }

        [LuisIntent("RmvFilter")]
        public async Task RmvFilter(IDialogContext context, LuisResult result)
        {
            await context.PostAsync(FilterDialog.CleanFilter(context, this.user, this.context, result.Entities));
            context.Wait(MessageReceived);
        }


        /*
        [LuisIntent("Recommendation")]
        public void Recommendation(IDialogContext context, LuisResult result)
        {

        }

        [LuisIntent("StoreLocation")]
        public void StoreLocation(IDialogContext context, LuisResult result)
        {

        }

        private Task Done(IDialogContext context, IAwaitable<object> result)
        {
            context.Done<object>(null);
            return null;
        }*/
    }
}