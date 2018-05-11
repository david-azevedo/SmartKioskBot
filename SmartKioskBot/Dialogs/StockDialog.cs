using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using MongoDB.Bson;
using SmartKioskBot.Controllers;
using SmartKioskBot.Models;
using SmartKioskBot.Helpers;
using SmartKioskBot.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MongoDB.Driver;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class StockDialog : LuisDialog<object>
    {

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
                        if (stores[i].ProductsInStock[j].Stock > 0)
                        {
                            storesWStock.Add(stores[i]);
                            break;
                        }
                    }
                }
            }

            if (storesWStock.Count() == 0)
            {
                reply.AttachmentLayout = AttachmentLayoutTypes.List;
                reply.Text = BotDefaultAnswers.getStockFail();
            }
            else
            {
                reply.Text = BotDefaultAnswers.getStockSuccess();
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                List<Attachment> cards = new List<Attachment>();

                foreach (Store s in storesWStock)
                {
                    cards.Add(ProductCard.getStoreDetailsCard(s, productId).ToAttachment());
                }

                reply.Attachments = cards;
            }
            return reply;
        }
    }
}