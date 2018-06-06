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
    public class CompareDialog
    {

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
