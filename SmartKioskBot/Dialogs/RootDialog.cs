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


            //USER IDENTIFICATION
            var channelId = message.ChannelId;
            var currentUser = UserController.getUser(channelId);

            //user doesn't exist
            if (currentUser == null)
            {
                UserController.CreateUser(channelId, message.From.Id, message.From.Name, "Portugal");
                currentUser = UserController.getUser(channelId);
                ContextController.CreateContext(currentUser);
            }
            // IDENTIFICATION


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
                    "filter [marca/preço/nome] [valor] \n\n" +
                    "filter-clean \n\n";
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
            //ADD PRODUCT TO WISH LIST
            else if (details[0].Equals(BotDefaultAnswers.add_wish_list, StringComparison.CurrentCultureIgnoreCase))
            {
                //TODO
            }
            //ADD PRODUCT TO COMPARATOR
            else if (details[0].Equals(BotDefaultAnswers.add_to_comparator, StringComparison.CurrentCultureIgnoreCase))
            {
                //TODO
            }
            else
            {
                // calculate something for us to return
                int length = (message.Text ?? string.Empty).Length;

                // return our reply to the user
                await context.PostAsync(BotDefaultAnswers.getGreeting(name));
                // await context.PostAsync($"Hello {name}! You sent {activity.Text} which was {length} characters");
            }
        }
    }
}