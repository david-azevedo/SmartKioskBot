using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using MongoDB.Bson;
using MongoDB.Driver;
using SmartKioskBot.Controllers;
using SmartKioskBot.Helpers;
using SmartKioskBot.Logic;
using SmartKioskBot.Models;
using SmartKioskBot.UI;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class CompareDialog : IDialog<object>
    {
        private List<Product> productsToCompare;
        private Context context;

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            /*var activity = await result as IMessageActivity;

            var reply = context.MakeMessage();
            reply.Text = BotDefaultAnswers.getOngoinComp();
            await context.PostAsync(reply);

            // OBTER PRODUTOS -- INÍCIO
            var productCollection = DbSingleton.GetDatabase().GetCollection<Product>(AppSettings.ProductsCollection);

            List<Product> products = productCollection.Find(new BsonDocument()).ToList();

            var collection = DbSingleton.GetDatabase().GetCollection<Product>(AppSettings.CollectionName);
            var builder = Builders<Product>.Filter;

            var filter1 = builder.Eq("_id", ObjectId.Parse("5ad6628186e5482fb04ea97f"));
            var filter2 = builder.Eq("_id", ObjectId.Parse("5ad6628186e5482fb04ea981"));

            var product1 = collection.Find(filter1).FirstOrDefault();
            var product2 = collection.Find(filter2).FirstOrDefault();

            //list of the products to compares
            var productsToCompare = new List<Product>() { product1, product2 };

            // OBTER PRODUTOS -- FIM

            Comparator.ShowProductComparison(context, productsToCompare);

            context.Wait(MessageReceivedAsync);*/

            // TODO check comparator, not working when retrieving products from Context.Comparator

            var activity = await result as IMessageActivity;

            //fetch context
            var currentUser = UserController.getUser(activity.ChannelId);
            this.context = ContextController.GetContext(currentUser.Id);

            var userInput = (activity.Text != null ? activity.Text : "").Split(new[] { ' ' }, 4);
            string[] details = activity.Text.Split(' ');

            if (details[0] == BotDefaultAnswers.do_comparator)
            {
                var reply = context.MakeMessage();
                reply.Text = BotDefaultAnswers.getOngoingComp();
                await context.PostAsync(reply);
                // OBTER PRODUTOS
                this.productsToCompare = new List<Product>();
                foreach (ObjectId o in this.context.Comparator)
                {
                    this.productsToCompare.Add(ProductController.getProduct(o.ToString()));
                }
                Comparator.ShowProductComparison(context, this.productsToCompare);
                context.Wait(MessageReceivedAsync);

            }
            else if (details[0] == BotDefaultAnswers.add_to_comparator)
            {
                Product productToAdd = ProductController.getProduct(details[1]);
                //MOSTRA PRODUTO ADICIONADO
                var reply = context.MakeMessage();
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                reply.Text = String.Format(BotDefaultAnswers.getAddComparator());
                reply.Attachments = new List<Attachment>() { ProductCard.GetProductCard(productToAdd, ProductCard.CardType.SEARCH).ToAttachment() };

                await context.PostAsync(reply);

                //UPDATE AO COMPARADOR DO USER
                ContextController.AddComparator(currentUser, details[1]);
            }
            else if (details[0] == BotDefaultAnswers.rem_comparator)
            {
                var reply = context.MakeMessage();
                reply.Text = BotDefaultAnswers.getRemComparator();
                await context.PostAsync(reply);
                //REMOVER PRODUTO
                ContextController.RemComparator(currentUser, details[1]);
            }
            context.Done<object>(null);
            
        }
    }
}
