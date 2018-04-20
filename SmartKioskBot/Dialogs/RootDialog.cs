using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using SmartKioskBot.Services;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            // getting the senders name
            string name = activity.From.Name.ToString();

            // calculate something for us to return
            int length = (activity.Text ?? string.Empty).Length;

            // return our reply to the user
            await context.PostAsync(BotDefaultAnswers.getGreeting("João"));
            // await context.PostAsync($"Hello {name}! You sent {activity.Text} which was {length} characters");

            await HandleIntent(context, activity.Text);

            context.Wait(MessageReceivedAsync);
        }

        private async Task HandleIntent(IDialogContext context, string message)
        {
            LuisResult result = await LUIS.GetResult(message);

            Debug.WriteLine("LUIS RESULT");
            Debug.WriteLine(result);

            if (result.TopScoringIntent.Intent.ToString() == "none") // if LUIS cannot detect an intent, call QnA
            {
                QnAMaker.Result qnaResult = QnAMaker.GetResult(message);
                await context.PostAsync(qnaResult.Answer);
            }
        }
    }
}