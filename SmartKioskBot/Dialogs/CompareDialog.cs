using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using MongoDB.Bson;
using MongoDB.Driver;
using SmartKioskBot.Helpers;
using SmartKioskBot.Logic;
using SmartKioskBot.Models;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class CompareDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as IMessageActivity;

            var reply = context.MakeMessage();
            reply.Text = "Estou a analisar os computadores...";
            await context.PostAsync(reply);

            // OBTER PRODUTOS -- INÍCIO
            var productCollection = DbSingleton.GetDatabase().GetCollection<Product>(AppSettings.ProductsCollection);

            List<Product> products = productCollection.Find(new BsonDocument()).ToList();

            var collection = DbSingleton.GetDatabase().GetCollection<Product>(AppSettings.CollectionName);
            var builder = Builders<Product>.Filter;

            var filter1 = builder.Eq("_id", ObjectId.Parse("5ad6628086e5482fb04ea97b"));
            var filter2 = builder.Eq("_id", ObjectId.Parse("5ad6628186e5482fb04ea97e"));

            var product1 = collection.Find(filter1).FirstOrDefault();
            var product2 = collection.Find(filter2).FirstOrDefault();

            //list of the products to compares
            var productsToCompare = new List<Product>() { product1, product2 };

            // OBTER PRODUTOS -- FIM

            Comparator.ShowProductComparison(context, productsToCompare);

            context.Wait(MessageReceivedAsync);
        }
    }
}