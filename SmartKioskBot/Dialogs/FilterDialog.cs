using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using MongoDB.Driver;
using SmartKioskBot.Helpers;
using SmartKioskBot.Models;
using SmartKioskBot.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class FilterDialog : IDialog<object>
    {
        protected List<FilterDefinition<Product>> filters { get; set; }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var message = await activity;

            //parse message
            var userInput = (message.Text != null ? message.Text : "").Split(new[] { ' ' }, 3);
            string[] details = message.Text.Split(' ');

            if (filters == null)
                filters = new List<FilterDefinition<Product>>();

            if (details[0] != "filter")
                context.Done<object>(null);

            // Get products and create a reply to reply back to the user.
            this.filters.Add(GetFilter(details[1], details[2]));
            List<Product> products = GetProductsForUser();
            await ShowProducts(products, context);

            context.Wait(MessageReceivedAsync);
        }

        private List<Product> GetProductsForUser()
        {
            var collection = DbSingleton.GetDatabase().GetCollection<Product>(AppSettings.CollectionName);
            var total_filter = Builders<Product>.Filter.Empty;

            foreach (FilterDefinition<Product> f in filters)
            {
                total_filter = total_filter & f;
            }
            
            var products = collection.Find(total_filter).ToList();
            return products;
        }

        private async Task ShowProducts(List<Product> products, IDialogContext context)
        {
            var reply = context.MakeMessage();

            if (products.Count == 0)
            {
                reply.AttachmentLayout = AttachmentLayoutTypes.List;
                reply.Text = BotDefaultAnswers.getFilterFail();
            }
            else
            {
                reply.Text = BotDefaultAnswers.getFilterSuccess();
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                List<Attachment> cards = new List<Attachment>();

                foreach (Product p in products)
                {
                    cards.Add(ProductCard.getProductCard(p).ToAttachment());
                }

                reply.Attachments = cards;
            }
            await context.PostAsync(reply);
        }

        private FilterDefinition<Product> GetFilter(string filter, string value){
            switch (filter.ToLower())
            {
                case "nome":
                    return Builders<Product>.Filter.Where(x => x.Name.ToLower() == value.ToLower());
                case "preço":
                    return Builders<Product>.Filter.Where(x => x.Price.ToLower() == value.ToLower());
                case "marca":
                    return Builders<Product>.Filter.Where(x => x.Brand.ToLower() == value.ToLower());
            }

            return null;
        }
    }
}