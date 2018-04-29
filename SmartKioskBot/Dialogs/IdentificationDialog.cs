using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using SmartKioskBot.Controllers;
using SmartKioskBot.UI;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class IdentificationDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {

            var message = await result as Activity;

            var userInput = (message.Text != null ? message.Text : "").Split(new[] { ' ' }, 3);
            var command = userInput[0];
            var channelId = message.ChannelId;
            var currentUser = UserController.getUser(channelId);

            // customer-card/set-customer-card <number> 
            // customer -email/set-customer-email <email>
            if (command.Equals("set-customer-card"))
            {
                var customerCard = userInput[1];
                await IdentificationCards.DisplayHeroCard(context, "set-customer-card", "Cartão cliente", customerCard);
            }
            else if (command.Equals("set-customer-email"))
            {
                var customerEmail = userInput[1];
                await IdentificationCards.DisplayHeroCard(context, "set-customer-email", "E-mail", customerEmail);
            }
            else if (command.Equals("customer-card"))
            {
                currentUser = UserController.getUserByCustomerCard(userInput[1]);
                await IdentificationCards.DisplayHeroCard(context, "customer-card", "Cartão cliente", userInput[1]);
            }
            else if (command.Equals("customer-email"))
            {
                currentUser = UserController.getUserByEmail(userInput[1]);
                await IdentificationCards.DisplayHeroCard(context, "customer-email", "E-mail", userInput[1]);
            }
            else if (command.Equals("customer-info"))
            {
                await context.PostAsync(BotDefaultAnswers.getCustomerInfo(currentUser));
            }
            else if (command.Equals("set-customer-name")) 
            {
                await IdentificationCards.DisplayHeroCard(context, "set-customer-name", "Nome", userInput[1]);
            }
            else if (command.Equals("first-dialog"))
            {
                await context.PostAsync(BotDefaultAnswers.getCountry());
            }
            else if (command.Equals("set-customer-country"))
            {
                await IdentificationCards.DisplayHeroCard(context, "set-customer-country", "País", userInput[1]);
            }


            //Save values on DB
            if (userInput[1].Equals("yes"))
            {
                if (command.Equals("SaveEmail"))
                {
                    UserController.SetEmail(currentUser, userInput[2]);
                    await context.PostAsync(BotDefaultAnswers.getAddIdentifier("email", userInput[2]));
                }
                else if (command.Equals("SaveCard"))
                {
                    UserController.SetCustomerCard(currentUser, userInput[2]);
                    await context.PostAsync(BotDefaultAnswers.getAddIdentifier("numero de cliente", userInput[2]));
                }
                else if (command.Equals("AddChannel"))
                {
                    UserController.AddChannel(currentUser, channelId);
                }
                else if (command.Equals("SaveName"))
                {
                    UserController.SetCustomerName(currentUser, userInput[2]);
                    await context.PostAsync(BotDefaultAnswers.getAddIdentifier("Nome", userInput[2]));
                }
                else if (command.Equals("SaveCountry"))
                {
                    UserController.CreateUser(channelId, message.From.Id, message.From.Name, userInput[2]);
                    currentUser = UserController.getUser(channelId);
                    ContextController.CreateContext(currentUser);

                    await context.PostAsync(BotDefaultAnswers.getAddUser());
                }

            }
            else if (userInput[1].Equals("no"))
            {
                await context.PostAsync(BotDefaultAnswers.getActionCanceled());
            }

        }

    }
}