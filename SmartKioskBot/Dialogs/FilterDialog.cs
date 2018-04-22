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

            //Get client & user data
            StateClient client = message.GetStateClient();
            BotData userData = await client.BotState.GetUserDataAsync(message.ChannelId, message.From.Id);

            var reply = context.MakeMessage();
            //reply.Text = "CHANNEL ID: " + message.ChannelId + "\n USER ID: " + message.From.Id;
            //await context.PostAsync(reply);

            reply.Text = "STATE BEFORE: \n\n" + userData.Data.ToString();
            await context.PostAsync(reply);

            //load filters
            string[] filtersStored = userData.GetProperty<string[]>("Filter");
            if (filtersStored == null)
                filtersStored = new String[] { };
            else
                this.filters = ParseFilters(filtersStored);

            //parse message ->TEMPORARIO
            var userInput = (message.Text != null ? message.Text : "").Split(new[] { ' ' }, 3);
            string[] details = message.Text.Split(' ');

            if (this.filters == null)
                this.filters = new List<FilterDefinition<Product>>();

            //FILTER PRODUCT
            if (details[0] == "filter")
            {
                // Get products and create a reply to reply back to the user.
                this.filters.Add(GetFilter(details[1], details[2]));
                List<Product> products = GetProductsForUser();
                await ShowProducts(products, context);

                //update filters
                var tmp = filtersStored.ToList<String>();
                tmp.Add(details[1] + "^=^" + details[2]);
                userData.SetProperty<string[]>("Filter", tmp.ToArray<string>());
                await client.BotState.SetUserDataAsync(message.ChannelId, message.From.Id, userData);
            }
            //CLEAN ALL FILTERS
            else if (details[0] == "filter-clean")
            {
                userData.SetProperty<string[]>("Filter", new String[] { });
                await client.BotState.SetUserDataAsync(message.ChannelId, message.From.Id, userData);
            }

            //TESTE
            reply.Text = "STATE AFTER: \n\n" + userData.Data.ToString();
            await context.PostAsync(reply);

            context.Done<object>(null);
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

        private List<FilterDefinition<Product>> ParseFilters(string[] filters)
        {
            List<FilterDefinition<Product>> parsed = new List<FilterDefinition<Product>>();

            foreach (string a in filters)
            {
                string[] parts = a.Split('^');
                parsed.Add(GetFilter(parts[0], parts[2]));
            }

            return parsed;
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