using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using MongoDB.Driver;
using SmartKioskBot.Controllers;
using SmartKioskBot.Helpers;
using SmartKioskBot.Models;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public sealed class RootDialog : IDialog<object>
    {
#pragma warning disable 1998
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task StartAsync(IDialogContext context, IAwaitable<object> activity)
#pragma warning restore 1998
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var message = await activity as Activity;


            // Get the command, or the first word, that the user typed in.
            var userInput = message.Text != null ? message.Text : "";
            var command = (userInput.Split(new[] { ' ' }, 3))[0];

            // getting the senders name
            string name = message.From.Name.ToString();

            //testing purposes only: getting the command (filter) and the argument (brand)
            string[] details = message.Text.Split(' ');

            if (details[0].Equals("help", StringComparison.CurrentCultureIgnoreCase))
            {
                var reply = context.MakeMessage();

                reply.Text = "Comandos:\n\n" +
                    "filter [marca/preço/nome] [operator] [valor] \n\n" +
                    "filter-rem [filter] \n\n" + 
                    "filter-clean \n\n" + 
                    "wishlist\n\n";
                await context.PostAsync(reply);
            }
            //ADD PRODUCT TO DB (TESTING)
            else if(details[0].Equals("add", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new AddProductDialog(), this.StartAsync, message, CancellationToken.None);
            }
            //FILTER PRODUCT
            else if (details[0].Equals("filter", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new FilterDialog(), this.StartAsync, message, CancellationToken.None);
            }
            //FILTER REMOVE
            else if (details[0].Equals("filter-rem", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new FilterDialog(), this.StartAsync, message, CancellationToken.None);
            }
            //FILTER CLEAN
            else if (details[0].Equals("filter-clean", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new FilterDialog(), this.StartAsync, message, CancellationToken.None);
            }
            //PRODUCT DETAILS
            else if(details[0].Equals(BotDefaultAnswers.show_product_details, StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new ProductDetails(), this.StartAsync, message, CancellationToken.None);
            }
            //VIEW WISH LIST
            else if (details[0].Equals("wishlist", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new WishListDialog(WishListDialog.Action.VIEW), this.StartAsync, message, CancellationToken.None);
            }
            //ADD PRODUCT TO WISH LIST
            else if (details[0].Equals(BotDefaultAnswers.add_wish_list, StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new WishListDialog(WishListDialog.Action.ADD), this.StartAsync,message, CancellationToken.None);
            }
            //REM PRODUCT FROM WISH LIST
            else if (details[0].Equals(BotDefaultAnswers.rem_wish_list, StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new WishListDialog(WishListDialog.Action.REM), this.StartAsync, message, CancellationToken.None);
            }
            //ADD PRODUCT TO COMPARATOR
            else if (details[0].Equals(BotDefaultAnswers.add_to_comparator, StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new CompareDialog(), this.StartAsync, message, CancellationToken.None);
            }
            //ADD CUSTOMER CARD
            else if (details[0].Equals("set-customer-card", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
            }
            //ADD CUSTOMER EMAIL
            else if (details[0].Equals("set-customer-email", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
            }
            //IDENTIFICATION BY CUSTOMER CARD
            else if (details[0].Equals("customer-card", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
            }
            //IDENTIFICATION BY EMAIL
            else if (details[0].Equals("customer-email", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
            }
            //FIRST DIALOG
            else if (details[0].Equals("first-dialog", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
            }
            //CUSTOMER INFO
            else if (details[0].Equals("customer-info", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
            }
            //SET CUSTOMER NAME
            else if (details[0].Equals("set-customer-name", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
            }
            //SET CUSTOMER COUNTRY
            else if (details[0].Equals("set-customer-country", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
            }
            //ADD CUSTOMER CARD CONFIRMATION
            else if (details[0].Equals("SaveCard", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
            }
            //ADD CUSTOMER EMAIL CONFIRMATION
            else if (details[0].Equals("SaveEmail", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
            }
            //ADD CHANNEL CONFIRMATION
            else if (details[0].Equals("AddChannel", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
            }
            //ADD CUSTOMER NAME 
            else if (details[0].Equals("SaveName", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
            }
            //ADD CUSTOMER COUNTRY 
            else if (details[0].Equals("SaveCountry", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new IdentificationDialog(), this.StartAsync, message, CancellationToken.None);
            }
            //COMPARE
            else if (details[0].Equals("compare", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new CompareDialog(), this.StartAsync, message, CancellationToken.None);
            }
            //REMOVE COMPARATOR
            else if (details[0].Equals("RemoverComparador", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new CompareDialog(), this.StartAsync, message, CancellationToken.None);
            }
            else
            {
                var reply = context.MakeMessage();

                reply.Text = "Comandos:\n\n" +
                    "filter [marca/preço/nome] [operator] [valor] \n\n" +
                    "filter-rem [filter] \n\n" +
                    "filter-clean \n\n" +
                    "wishlist\n\n";
                await context.PostAsync(reply);
            }
        }
    }
}