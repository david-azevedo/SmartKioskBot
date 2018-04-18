using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using MongoDB.Driver;
using SmartKioskBot.Helpers;
using SmartKioskBot.Models;
using SmartKioskBot.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class FilterDialog : IDialog<object>
    {
#pragma warning disable 1998
        public async Task StartAsync(IDialogContext context)
#pragma warning restore 1998
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result as Activity;

            var userInput = (message.Text != null ? message.Text : "").Split(new[] { ' ' }, 2);

            string[] details = message.Text.Split(' ');

            // Get products and create a reply to reply back to the user.
            var brand = details[1];//message.Text; //TODO
            List<Product> products = GetProductsForUser(brand);
            ShowProducts(products, context);

           // await context.PostAsync(reply);
            context.Done<object>(null);
        }

        private static List<Product> GetProductsForUser(string brand)
        {
            var collection = DbSingleton.GetDatabase().GetCollection<Product>(AppSettings.CollectionName);
            var filter = Builders<Product>.Filter.Where(x => x.Brand.ToLower() == brand.ToLower());


            var products = collection.Find(filter).ToList();
            return products;
        }

        private async Task ShowProducts(List<Product> products, IDialogContext context)
        {
            

            var reply = context.MakeMessage();

            if (products.Count == 0)
            {
                reply.Text = "Não existem produtos com essas especificações.";
            }
            else
            {
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                List<Attachment> cards = new List<Attachment>();

                foreach (Product p in products)
                {
                    cards.Add(ProductCard.getCard(p).ToAttachment());
                }

                reply.Attachments = cards;
            }
            
            await context.PostAsync(reply);

        }
    }
}