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
    public class ProductDetails : IDialog<object> { 

        public async Task StartAsync(IDialogContext context) {
            context.Wait(ShowProductMessage);
        }

        public async Task ShowProductMessage(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var message = await activity;
            
            var userInput = (message.Text != null ? message.Text : "").Split(new[] { ' ' }, 2);
            string[] details = message.Text.Split(' ');
            var id = details[1];

            var collection = DbSingleton.GetDatabase().GetCollection<Product>(AppSettings.ProductsCollection);

            //get product
            var query_id = Builders<Product>.Filter.Eq("_id", ObjectId.Parse(id));
            var entity = collection.Find(query_id).ToList();        

            var product = entity[0];

            List<Attachment> cards = new List<Attachment>();
            cards.Add(ProductCard.getProductDetailsCard(product).ToAttachment());

            var reply = context.MakeMessage();
            reply.Attachments = cards;

            await context.PostAsync(reply);

            context.Done<object>(new object());

        }
    }
}