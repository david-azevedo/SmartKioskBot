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

        public CompareDialog(User user)
        {
            this.user = user;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await InitAsync(context, null);
        }

        public async Task InitAsync(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {

            await context.PostAsync("Bem vindo ao comparador, estes são os produtos a comparar: ");
            
            var products = new List<Product>();

            var itemsToCompare = ContextController.GetContext(this.user.Id).Comparator;

            await context.PostAsync(itemsToCompare.Length.ToString());

            foreach (ObjectId o in itemsToCompare)
            {
                products.Add(ProductController.getProduct(o.ToString()));
            }
        

            var reply = context.MakeMessage();

            if (products.Count > 0)
            {
                //display products 
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                List<Attachment> cards = new List<Attachment>();

                for (var i = 0; i < products.Count && i < Constants.N_ITEMS_CARROUSSEL; i++)
                    cards.Add(ProductCard.GetProductCard(products[i], ProductCard.CardType.COMPARATOR).ToAttachment());

                reply.Attachments = cards;
                await context.PostAsync(reply);

                context.Wait(InputHandler);

                //Check if pagination is needed
                if (products.Count > Constants.N_ITEMS_CARROUSSEL)
                {
                    //pagination card
                    reply = context.MakeMessage();
                    reply.Attachments.Add(await getCardAttachment(CardType.PAGINATION));
                    await context.PostAsync(reply);
                }

                //Show compare button
                    //pagination card
                    reply = context.MakeMessage();
                    reply.Attachments.Add(await getCardAttachment(CardType.COMPARATOR));
                    await context.PostAsync(reply);

            } else
            {
                //TODO 
            }
        }

        public async Task InputHandler(IDialogContext context, IAwaitable<object> argument)
        {
           var activity = await argument as Activity;

            
            //Received a Message
            if (activity.Text != null)
            {
                /*if (activity.Text == BotDefaultAnswers.next_pagination)
                    context.Done(new CODE(DIALOG_CODE.DONE));
                else*/
                    context.Done(new CODE(DIALOG_CODE.PROCESS_LUIS, activity as IMessageActivity));
            }
            //Received an Event
            else if (activity.Value != null)
            {
                JObject json = activity.Value as JObject;
                CardType type = getReplyType(json);

                switch (type)
                {
                    case CardType.COMPARATOR:
                        await ViewComparator(context);
                        context.Done(new CODE(DIALOG_CODE.DONE));
                        break;
                    default:
                        context.Done(new CODE(DIALOG_CODE.DONE));
                        break;
                }
            }
            else
                context.Done(new CODE(DIALOG_CODE.DONE));
        }

        public static async Task ViewComparator(IDialogContext _context)
        {
            //fetch user and context
            var currentUser = UserController.getUser(_context.Activity.ChannelId);
            var context = ContextController.GetContext(currentUser.Id);

            var reply = _context.MakeMessage();
            reply.Text = BotDefaultAnswers.getOngoingComp();
            await _context.PostAsync(reply);

            // OBTER PRODUTOS
            var productsToCompare = new List<Product>();
            foreach (ObjectId o in context.Comparator)
            {
                productsToCompare.Add(ProductController.getProduct(o.ToString()));
            }
            Comparator.ShowProductComparison(_context, productsToCompare);
            return;
        }

        public static async Task AddComparator(IDialogContext _context, string message)
        {
            string[] parts = message.Split(':');
            var product = parts[1].Replace(" ", "");

            if (parts.Length >= 2)
            {
                //fetch user and context
                var currentUser = UserController.getUser(_context.Activity.ChannelId);
                var context = ContextController.GetContext(currentUser.Id);

                Product productToAdd = ProductController.getProduct(product);
                //MOSTRA PRODUTO ADICIONADO
                var reply = _context.MakeMessage();
                reply.Text = String.Format(BotDefaultAnswers.getAddComparator());

                await _context.PostAsync(reply);

                //UPDATE AO COMPARADOR DO USER
                ContextController.AddComparator(currentUser, product);
            }

            return;

        }

        public static async Task RmvComparator(IDialogContext _context, string message)
        {
            string[] parts = message.Split(':');
            var product = parts[1].Replace(" ", "");

            if (parts.Length >= 2)
            {
                //fetch user and context
                var currentUser = UserController.getUser(_context.Activity.ChannelId);
                var context = ContextController.GetContext(currentUser.Id);

                var reply = _context.MakeMessage();
                reply.Text = BotDefaultAnswers.getRemComparator();
                await _context.PostAsync(reply);

                //REMOVER PRODUTO
                ContextController.RemComparator(currentUser, product);
            }

            return;
        }

        
    }
}
