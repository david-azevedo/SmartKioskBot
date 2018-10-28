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
    public class StoreDialog : LuisDialog<object>
    {
        const int N_STORES_MAX = 5;

        public async static Task ShowClosestStores(IDialogContext context)
        {
            //simulate user position
            Random r = new Random();
            Double[] coords = new Double[]{
                r.NextDouble() * 180 - 90,
                r.NextDouble() * 180 - 90
             };

            List<Store> stores = StoreController.getClosesStores(coords);

            var reply = context.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            List<Attachment> attachments = new List<Attachment>();

            for (var i = 0; i < stores.Count() && i <= N_STORES_MAX; i++)
            {
                attachments.Add(StoreCard.GetStoreCard(stores[i]).ToAttachment());
            }

            reply.Attachments = attachments;

            await context.PostAsync(Interactions.getClosesStore());
            await context.PostAsync(reply);
        }

        public static IMessageActivity ShowStores(IDialogContext context, string productId)
        {
            var reply = context.MakeMessage();

            var storeCollection = DbSingleton.GetDatabase().GetCollection<Store>(AppSettings.StoreCollection);
            var filter = Builders<Store>.Filter.Empty;

            List<Store> stores = storeCollection.Find(filter).ToList();
            List<Store> storesWStock = new List<Store>();

            for (int i = 0; i < stores.Count(); i++)
            {
                for (int j = 0; j < stores[i].ProductsInStock.Count(); j++)
                {
                    if (stores[i].ProductsInStock[j].ProductId.ToString().Equals(productId))
                    {
                        if (stores[i].ProductsInStock[j].Stock > 0 && storesWStock.Count() <= N_STORES_MAX)
                        {
                            storesWStock.Add(stores[i]);
                            break;
                        }
                    }
                }
            }

            var text = "";

            if (storesWStock.Count() == 0)
            {
                reply.AttachmentLayout = AttachmentLayoutTypes.List;
                text = Interactions.getStockFail();
            }
            else
            {
                text = Interactions.getStockSuccess();
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                List<Attachment> cards = new List<Attachment>();

                for (var i = 0; i < storesWStock.Count() && i < 7; i++)
                {
                    cards.Add(StoreCard.GetStoreDetailsCard(storesWStock[i], productId).ToAttachment());
                }

                reply.Attachments = cards;
            }
            context.PostAsync(text);
            return reply;
        }

    }
}