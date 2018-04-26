using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using MongoDB.Driver;
using SmartKioskBot.Controllers;
using SmartKioskBot.Helpers;
using SmartKioskBot.Models;
using SmartKioskBot.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SmartKioskBot.Models.Context;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class FilterDialog : IDialog<object>
    {
        private List<FilterDefinition<Product>> filters;
        private Context context;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var message = await activity;

            //fetch context
            var currentUser = UserController.getUser(message.ChannelId);
            this.context = ContextController.GetContext(currentUser.Id);

            //parse message ->TEMPORARIO
            var userInput = (message.Text != null ? message.Text : "").Split(new[] { ' ' }, 4);
            string[] details = message.Text.Split(' ');

            //FILTER PRODUCT
            if (details[0] == "filter")
            {
                //parse filters
                this.filters = new List<FilterDefinition<Product>>();
                foreach (Filter f in this.context.Filters)
                {
                    this.filters.Add(GetFilter(f.FilterName, f.Operator, f.Value));
                }

                // Get products and create a reply back to the user.
                this.filters.Add(GetFilter(details[1], details[2], details[3]));
                List<Product> products = GetProductsForUser();
                await ShowProducts(products, context);

                //update filters
                ContextController.AddFilter(currentUser, details[1], details[2], details[3]);

            }
            //REMOVE A FILTER
            else if(details[0] == "filter-rem")
            {
                ContextController.RemFilter(currentUser, details[1]);

                //show products
                this.filters = new List<FilterDefinition<Product>>();
                foreach (Filter f in this.context.Filters)
                {
                    this.filters.Add(GetFilter(f.FilterName, f.Operator, f.Value));
                }

                // Get products and create a reply back to the user.
                List<Product> products = GetProductsForUser();
                await ShowProducts(products, context);
            }
            //CLEAN ALL FILTERS
            else if (details[0] == "filter-clean")
            {
                ContextController.CleanFilters(currentUser);
            }

            context.Done<object>(null);
        }

        private List<Product> GetProductsForUser()
        {
            var total_filter = Builders<Product>.Filter.Empty;

            //combine all filters
            foreach (FilterDefinition<Product> f in filters)
            {
                total_filter = total_filter & f;
            }

            return ProductController.getProductsFilter(total_filter);
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
                    cards.Add(ProductCard.GetProductCard(p,ProductCard.CardType.SEARCH).ToAttachment());
                }

                reply.Attachments = cards;
            }
            await context.PostAsync(reply);
        }

        private FilterDefinition<Product> GetFilter(string filter, string op,string value){
            switch (filter.ToLower())
            {
                case "nome":
                    return Builders<Product>.Filter.Where(x => x.Name.ToLower() == value.ToLower());
                case "preço":
                       if (op == "=")
                            return Builders<Product>.Filter.Where(x => x.Price == short.Parse(value.ToLower()));
                        else if (op == ">") 
                            return Builders<Product>.Filter.Gte(x => x.Price,short.Parse(value));
                        else if (op == "<")
                            return Builders<Product>.Filter.Lte(x => x.Price, short.Parse(value));
                        break;
                case "marca":
                    return Builders<Product>.Filter.Where(x => x.Brand.ToLower() == value.ToLower());
                case "processador":
                    return Builders<Product>.Filter.Where(x => x.CPU.ToLower() == value.ToLower());
                case "familia_cpu":
                    return Builders<Product>.Filter.Where(x => x.CPUFamily.ToLower() == value.ToLower()); ;
                case "velocidade_cpu":
                    return Builders<Product>.Filter.Where(x => x.CPUSpeed.ToLower() == value.ToLower());
                case "nrNucleos":
                    return Builders<Product>.Filter.Where(x => x.CoreNr.ToLower() == value.ToLower());
                case "ram":
                    return Builders<Product>.Filter.Where(x => x.RAM.ToLower() == value.ToLower());
                case "tipo_armazenamento":
                    return Builders<Product>.Filter.Where(x => x.StorageType.ToLower() == value.ToLower());
                case "armazenamento":
                    return Builders<Product>.Filter.Where(x => x.StorageAmount.ToLower() == value.ToLower());
                case "placa_grafica":
                    return Builders<Product>.Filter.Where(x => x.GraphicsCardType.ToLower() == value.ToLower());
                case "autonomia":
                    return Builders<Product>.Filter.Where(x => x.Autonomy.ToLower() == value.ToLower());
                case "placa_som":
                    return Builders<Product>.Filter.Where(x => x.SoundCard.ToLower() == value.ToLower());
                case "camera":
                    return Builders<Product>.Filter.Where(x => x.HasCamera.ToLower() == value.ToLower());
                case "software":
                    return Builders<Product>.Filter.Where(x => x.Software.ToLower() == value.ToLower());
                case "os":
                    return Builders<Product>.Filter.Where(x => x.OS.ToLower() == value.ToLower());
                case "tamanho_ecra":
                    return Builders<Product>.Filter.Where(x => x.ScreenDiagonal.ToLower() == value.ToLower());
                case "ecra_tactil":
                    return Builders<Product>.Filter.Where(x => x.TouchScreen.ToLower() == value.ToLower());
                case "garantia":
                    return Builders<Product>.Filter.Where(x => x.Warranty.ToLower() == value.ToLower());
                case "cor":
                    return Builders<Product>.Filter.Where(x => x.Colour.ToLower() == value.ToLower());
            }

            return null;
        }


    }
}