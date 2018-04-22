using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using MongoDB.Bson;
using MongoDB.Driver;
using SmartKioskBot.Helpers;
using SmartKioskBot.Models;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class AddWishList : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var message = await activity;

            var userInput = (message.Text != null ? message.Text : "").Split(new[] { ' ' }, 2);
            string[] details = message.Text.Split(' ');
            var id = details[1];

            var collection = DbSingleton.GetDatabase().GetCollection<Product>(AppSettings.CollectionName);

            //get product
            var query_id = Builders<Product>.Filter.Eq("_id", ObjectId.Parse(id));
            var entity = collection.Find(query_id).ToList();
            var product = entity[0];


            //TODO adicionar na db o produto a wish list do utilizador 

            await context.PostAsync($"Produto adicionado com sucesso!");

            context.Wait(MessageReceivedAsync);
        }
    }
}