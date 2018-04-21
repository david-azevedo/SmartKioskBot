using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

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

            //ADD PRODUCT TO DB (TESTING)
            if (details[0].Equals("add", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new AddProductDialog(), this.StartAsync, message, CancellationToken.None);
            }
            //FILTER PRODUCT
            else if (details[0].Equals("filter", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.Forward(new FilterDialog(), this.StartAsync, message, CancellationToken.None);
            }
            else
            {
                // calculate something for us to return
                int length = (message.Text ?? string.Empty).Length;

                // return our reply to the user
                await context.PostAsync(BotDefaultAnswers.getGreeting("João"));
                // await context.PostAsync($"Hello {name}! You sent {activity.Text} which was {length} characters");
            }
        }
    }
}