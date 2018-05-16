using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using MongoDB.Bson;
using MongoDB.Driver;
using SmartKioskBot.Controllers;
using SmartKioskBot.Helpers;
using SmartKioskBot.Models;
using SmartKioskBot.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class ClosestStoresDialog : LuisDialog<object>
    {

        public async static Task ShowClosestStores(IDialogContext context, Double[] coords, int n_stores)
        {
            List<Store> stores = StoreController.getClosesStores(coords);

            var reply = context.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            List<Attachment> attachments = new List<Attachment>();

            for (var i = 0; i < stores.Count() && i < n_stores && i <7; i++)
            {
                attachments.Add(StoreCard.GetStoreCard(stores[i]).ToAttachment());
            }

            reply.Attachments = attachments;

            await context.PostAsync(BotDefaultAnswers.getClosesStore());
            await context.PostAsync(reply);
        }

    }
}