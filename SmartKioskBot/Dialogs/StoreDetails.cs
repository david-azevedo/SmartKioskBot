using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using MongoDB.Bson;
using MongoDB.Driver;
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
    public class StoreDetails : LuisDialog<object>
    {

        public async static Task ShowStoresMessage(IDialogContext context, string productId)
        {
            var collection = DbSingleton.GetDatabase().GetCollection<Product>(AppSettings.ProductsCollection);

            var filter = Builders<Product>.Filter.Eq(p => p.Id, ObjectId.Parse(productId));
            Product product = collection.Find(filter).FirstOrDefault();

            List<Attachment> cards = new List<Attachment>();
            cards.Add(ProductCard.getStoresCard(product).ToAttachment());

            var reply = context.MakeMessage();
            reply.Attachments = cards;

            await context.PostAsync(reply);
        }
    }
}

