using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using SmartKioskBot.Logic;
using SmartKioskBot.Models;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class CompareDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as IMessageActivity;

            var reply = context.MakeMessage();
            reply.Text = "Hello, I'm comparing";
            await context.PostAsync(reply);

            Comparator.Test(context);

            context.Wait(MessageReceivedAsync);
        }
    }
}