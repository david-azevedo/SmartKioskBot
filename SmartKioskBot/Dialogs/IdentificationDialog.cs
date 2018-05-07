using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using Microsoft.Bot.Connector;
using SmartKioskBot.Controllers;
using SmartKioskBot.Models;
using SmartKioskBot.UI;



namespace SmartKioskBot.Dialogs
{

    [Serializable]
    public class UserQuery
    {
        [Prompt("Qual é o seu nome?")]
        public string Name { get; set; }

        [Prompt("Qual é o seu email?")]
        public string Email { get; set; }
        
        [Prompt("Qual é o seu país?")]
        public string Country { get; set; }

        [Prompt("Qual é o seu número de cartão da loja?")]
        public string CustomerCard { get; set; }
    }

    [Serializable]
    public class IdentificationDialog : IDialog<object>
    {
        private User user = null;

        public async Task StartAsync(IDialogContext context)
        {
            await MessageReceivedAsync(context, null);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            PromptDialog.Choice(context, this.OnOptionSelected, new List<string>() { "Sim", "Não" }, "Já falei consigo?", "Opção inválida", 3);
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected.ToLower())
                {
                    case "sim":
                        {
                            var createUserDialog = FormDialog.FromForm(this.FindUserDialog, FormOptions.PromptInStart);
                            context.Call(createUserDialog, this.ResumeAfterIdentification);
                            break;
                        }
                    case "não":
                        {
                            await context.PostAsync("Vamos proceder ao registo dos seus dados.");
                            var createUserDialog = FormDialog.FromForm(this.BuildCreateUserDialog, FormOptions.PromptInStart);
                            context.Call(createUserDialog, this.ResumeAfterUserCreation);
                            break;
                        }
                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Ooops! Ultrapassou o número máximo de tentativas!");
                context.Wait(this.MessageReceivedAsync);
            }
        }   

        private async Task UpdateCustomerCardAsync(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                if(optionSelected.ToLower() == "sim")
                {
                    var customerCardDialog = FormDialog.FromForm(this.BuildCustomerCardDialog, FormOptions.PromptInStart);
                    context.Call(customerCardDialog, this.ResumeAfterCardUpdate);
                }
                else
                {
                    context.Done<object>(null);
                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Ooops! Ultrapassou o número máximo de tentativas!");
                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task ResumeAfterUserCreation(IDialogContext context, IAwaitable<object> result)
        {
            var reply = context.MakeMessage();
            reply.Text = "Os seus dados foram registados!";
            await context.PostAsync(reply);
            PromptDialog.Choice(context, this.UpdateCustomerCardAsync, new List<string>() { "Sim", "Não" }, "Tem cartão da loja?", "Opção inválida", 3);

        }

        private async Task ResumeAfterIdentification(IDialogContext context, IAwaitable<object> result)
        {
            var reply = context.MakeMessage();
            reply.Text = "A sua conversa foi restaurada!";
            await context.PostAsync(reply);
            context.Done<object>(null);
        }

        private async Task ResumeAfterCardUpdate(IDialogContext context, IAwaitable<object> result)
        {
            var reply = context.MakeMessage();
            reply.Text = "O seu cartão da loja foi guardado.";
            await context.PostAsync(reply);
            context.Done<object>(null);
        }

        /*
         * FORMS
         */

        private IForm<UserQuery> BuildCreateUserDialog()
        {
            OnCompletionAsyncDelegate<UserQuery> RegisterUser = async (context, state) =>
            {
                UserController.CreateUser(context.Activity.ChannelId, state.Email, state.Name, state.Country);
                this.user = UserController.getUser(context.Activity.ChannelId);
                ContextController.CreateContext(user);
                CRMController.AddCustomer(this.user);
            };

            var form = new FormBuilder<UserQuery>()
                .Field(nameof(UserQuery.Name))
                .Field(nameof(UserQuery.Email))
                .Field(nameof(UserQuery.Country))
                .OnCompletion(RegisterUser)
                .Confirm(async (state) =>
                {
                    var r = "É esta a sua informação?\n\n" +
                    "Nome: " + state.Name + "\n\n" +
                    "Email: " + state.Email + "\n\n" +
                    "País: " + state.Email + "\n\n" +
                    "{||}";

                    var p = new PromptAttribute(r);
                    return p;
                });
            form.Configuration.Yes = BotDefaultAnswers.Yes;
            form.Configuration.No = BotDefaultAnswers.No;

            return form.Build();
        }

        private IForm<UserQuery> FindUserDialog()
        {
            OnCompletionAsyncDelegate<UserQuery> AddChannelUser = async (context, state) =>
            {
                User realUser = UserController.getUserByEmail(state.Email);

                if (realUser != null)
                {
                    UserController.AddChannel(realUser, context.Activity.ChannelId);
                }
                else
                {
                    await context.PostAsync("Não tenho conhecimento de nenhuma conversa com esse email." +
                        "Vamos efectuar um novo registo.");
                    await context.PostAsync("Vamos proceder ao registo dos seus dados.");
                    var createUserDialog = FormDialog.FromForm(this.BuildCreateUserDialog, FormOptions.PromptInStart);
                    context.Call(createUserDialog, this.ResumeAfterIdentification);
                }

                context.Done<object>(realUser);
            };

            var form = new FormBuilder<UserQuery>()
               .Field(nameof(UserQuery.Email))
               .OnCompletion(AddChannelUser)
               .Confirm(async (state) =>
               {
                   var r = "É esta a sua informação?\n\n" +
                   "Email: " + state.Email + "\n\n" +
                   "{||}";

                   var p = new PromptAttribute(r);
                   return p;
               });
            form.Configuration.Yes = BotDefaultAnswers.Yes;
            form.Configuration.No = BotDefaultAnswers.No;

            return form.Build();
        }

        private IForm<UserQuery> BuildCustomerCardDialog()
        {
            OnCompletionAsyncDelegate<UserQuery> CustomerCardSet = async (context, state) =>
            {
                UserController.SetCustomerCard(user, state.CustomerCard);
            };

            var form = new FormBuilder<UserQuery>()
                .Field(nameof(UserQuery.CustomerCard))
                .OnCompletion(CustomerCardSet)
                .Confirm(async (state) =>
                {
                    var r = "É este o seu cartão da loja?\n\n" +
                    "Nome: " + state.CustomerCard + "\n\n" +
                    "{||}";
                    var p = new PromptAttribute(r);
                    return p;
                });
            form.Configuration.Yes = BotDefaultAnswers.Yes;
            form.Configuration.No = BotDefaultAnswers.No;

            return form.Build();
        }

        /*
         * ANTIGO
         */
        /*public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
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

        }*/
    }
}