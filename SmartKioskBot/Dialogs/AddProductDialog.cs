using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using MongoDB.Driver;
using SmartKioskBot.Helpers;
using SmartKioskBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//Temporary!!! Mais facil criar a collection assim (não sei o que é uma shard key)
namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class AddProductDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result as Activity;
            
            var userInput = (message.Text != null ? message.Text : "").Split(new[] { ' ' }, 2);
            var command = userInput[0];
            var content = userInput.Length < 2 ? "" : userInput[1];
            string reply = "";


            // When users types in "add" without the content, give them instructions.
            if (String.IsNullOrWhiteSpace(content))
            {
                reply = "usage: add <brand>;<model>;<price>";
            }
            // Save the product.
            else
            {
                var productsCollection = DbSingleton.GetDatabase().GetCollection<Product>(AppSettings.ProductsCollection);
                productsCollection.InsertOne(CreateProduct(content));

                reply = "Added product to DB";

            }

            await context.PostAsync(reply);
            context.Done<object>(null);
        }

        private Product CreateProduct(string content)
        {
            string[] details = content.Split(';');
            return new Product
            {
                Brand = details[0],
                Model = details[1],
                Price = Convert.ToDouble(details[2])
            };
        }
    }
}