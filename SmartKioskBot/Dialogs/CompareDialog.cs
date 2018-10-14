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
using static SmartKioskBot.Helpers.Constants;
using static SmartKioskBot.Helpers.AdaptiveCardHelper;
using Newtonsoft.Json.Linq;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class CompareDialog : IDialog<Object>
    {
        public User user;
        public List<Product> products;

        public CompareDialog(User user)
        {
            this.user = user;
            this.products = new List<Product>();
        }

        public async Task StartAsync(IDialogContext context)
        {
            await InitAsync(context, null);
        }

        public async Task InitAsync(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            List<ButtonType> buttons = new List<ButtonType>();

            // fetch products
            products = new List<Product>();
            var itemsToCompare = ContextController.GetContext(this.user.Id).Comparator;

            foreach (ObjectId o in itemsToCompare)
                products.Add(ProductController.getProduct(o.ToString()));

            var reply = context.MakeMessage();

            if (products.Count > 0)
            {
                await context.PostAsync("Bem vindo ao comparador, estes são os produtos que adicionou ao comparador: ");
                
                //display products 
                reply = context.MakeMessage();
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                List<Attachment> cards = new List<Attachment>();

                //limit 
                for (var i = 0; i < products.Count && i < Constants.N_ITEMS_CARROUSSEL; i++)
                    cards.Add(ProductCard.GetProductCard(products[i], ProductCard.CardType.COMPARATOR).ToAttachment());

                reply.Attachments = cards;
                await context.PostAsync(reply);

                //Check if pagination is needed
                if (products.Count > Constants.N_ITEMS_CARROUSSEL)
                    buttons.Add(ButtonType.PAGINATION);
            }
            else
            {
                await context.PostAsync("Não tem produtos para comparar.");
                buttons.Add(ButtonType.COMPARE);
            }

            buttons.Add(ButtonType.ADD_PRODUCT);

            //show options
            reply = context.MakeMessage();
            reply.Attachments.Add(getCardButtonsAttachment(buttons, DialogType.COMPARE));
            await context.PostAsync(reply);

            context.Wait(InputHandler);
        }

        public async Task InputHandler(IDialogContext context, IAwaitable<object> argument)
        {
           var activity = await argument as Activity;

            //close dialog at the end without more processing
            bool done_ok = true;
            
            //Received a Message
            if (activity.Text != null)
            {
                done_ok = false;
                context.Done(new CODE(DIALOG_CODE.PROCESS_LUIS, activity as IMessageActivity));
            }
            //Received an Event
            else if (activity.Value != null)
            {
                JObject json = activity.Value as JObject;
                List<InputData> data = getReplyData(json);

                //have mandatory info
                if (data.Count >= 2)
                {
                    //json structure is correct
                    if (data[0].attribute == REPLY_ATR && data[1].attribute == DIALOG_ATR)
                    {
                        ClickType click = getClickType(data[0].value);

                        if (data[1].value.Equals(getDialogName(DialogType.COMPARE)) &&
                            click != ClickType.NONE)
                        {
                            switch (click)
                            {
                                case ClickType.COMPARE:
                                    done_ok = false;

                                    var reply = context.MakeMessage();
                                    reply.Text = BotDefaultAnswers.getOngoingComp();
                                    await context.PostAsync(reply);

                                    ComparatorLogic.ShowProductComparison(context, products);
                                    context.Wait(InputHandler);
                                    break;
                            }
                        }
                    }
                }
            }

            if(done_ok)
                context.Done(new CODE(DIALOG_CODE.DONE));
        }

        public static async Task AddComparator(IDialogContext context, string message, User user)
        {
            string[] parts = message.Split(':');
            var product = parts[1].Replace(" ", "");

            if (parts.Length >= 2)
            {
                var user_context = ContextController.GetContext(user.Id);

                if (ComparatorLogic.MAX_PRODUCTS_ON_COMPARATOR <= user_context.Comparator.Length)
                    await context.PostAsync("Lamento mas o número máximo de produtos permitidos no comparador é de " + 
                        ComparatorLogic.MAX_PRODUCTS_ON_COMPARATOR.ToString() + "produtos.");
                else
                {
                    Product productToAdd = ProductController.getProduct(product);
                    
                    var reply = context.MakeMessage();
                    reply.Text = String.Format(BotDefaultAnswers.getAddComparator());
                    await context.PostAsync(reply);

                    //UPDATE AO COMPARADOR DO USER
                    ContextController.AddComparator(user, product);
                }
            }
        }

        public static async Task RmvComparator(IDialogContext _context, string message, User user)
        {
            string[] parts = message.Split(':');
            var product = parts[1].Replace(" ", "");

            if (parts.Length >= 2)
            {
                var context = ContextController.GetContext(user.Id);

                var reply = _context.MakeMessage();
                reply.Text = BotDefaultAnswers.getRemComparator();
                await _context.PostAsync(reply);

                //REMOVER PRODUTO
                ContextController.RemComparator(user, product);
            }
        }

        
    }
}
