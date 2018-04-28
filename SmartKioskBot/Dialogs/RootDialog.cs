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
            
        }

        [LuisIntent("Filter")]
        public async Task Filter(IDialogContext context, LuisResult result)
        {
            FilterDialog w = new FilterDialog();
            IMessageActivity r = w.filtering(result.Entities, context.MakeMessage());
            context.PostAsync(r);
        }

        [LuisIntent("Recommendation")]
        public async Task Recommendation(IDialogContext context, LuisResult result)
        {
            
        }

        [LuisIntent("StoreLocation")]
        public async Task StoreLocation(IDialogContext context, LuisResult result)
        {
            
        }

        [LuisIntent("ViewWishList")]
        public async Task ViewWishList(IDialogContext context, LuisResult result)
        {
            // await context.Forward(new WishListDialog(WishListDialog.Action.VIEW), null);


            FilterDialog w = new FilterDialog();
            IMessageActivity r = w.filtering(result.Entities, context.MakeMessage());
        }
    }
}