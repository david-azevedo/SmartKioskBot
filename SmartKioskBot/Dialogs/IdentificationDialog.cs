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
        [Prompt(BotDefaultAnswers.identification_name)]
        public string Name { get; set; }

        [Prompt(BotDefaultAnswers.identigication_email)]
        public string Email { get; set; }

        [Prompt(BotDefaultAnswers.identification_storeCard)]
        public string CustomerCard { get; set; }
    }

    [Serializable]
    public class IdentificationDialog : IDialog<object>
    {
        private User user = null;
        private int tries = 3;
        private bool try_again = false;

        public async Task StartAsync(IDialogContext context)
        {
            await MessageReceivedAsync(context, null);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            PromptDialog.Choice(context, this.DoIdentification, 
                new List<string>() { BotDefaultAnswers.Yes[1], BotDefaultAnswers.No[1] }, 
                BotDefaultAnswers.identication, BotDefaultAnswers.invalid_option, tries);
        }

        //Proceed to identification
        private async Task DoIdentification(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                //yes
                if (optionSelected.ToLower().Contains(BotDefaultAnswers.Yes[0]))
                {
                    PromptDialog.Choice(context, this.OnOptionSelected,
                    new List<string>() { BotDefaultAnswers.Yes[1], BotDefaultAnswers.No[1] },
                    BotDefaultAnswers.identification_identified, BotDefaultAnswers.invalid_option, tries);
                }
                //no
                else if (optionSelected.ToLower().Contains(BotDefaultAnswers.No[0]))
                {
                    context.Done<object>(null);
                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync(BotDefaultAnswers.tries_exceeded);
                context.Wait(this.MessageReceivedAsync);
            }
        }

        //User and bot have spoken before?
        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                //yes
                if (optionSelected.ToLower().Contains(BotDefaultAnswers.Yes[0]))
                {
                    PromptDialog.Choice(context, this.IdentificationOption,
                new List<string>() { BotDefaultAnswers.email, BotDefaultAnswers.store_card },
                BotDefaultAnswers.identification_option, BotDefaultAnswers.invalid_option, tries);
                }
                //no
                else if (optionSelected.ToLower().Contains(BotDefaultAnswers.No[0]))
                {
                    await context.PostAsync(BotDefaultAnswers.identification_start_registration);
                    var createUserDialog = FormDialog.FromForm(this.BuildCreateUserDialog, FormOptions.PromptInStart);
                    context.Call(createUserDialog, this.ResumeAfterUserCreation);
                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync(BotDefaultAnswers.tries_exceeded);
                context.Wait(this.MessageReceivedAsync);
            }
        }

        //Identification with email or store card ?
        private async Task IdentificationOption(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                //email
                if (optionSelected.ToLower().Contains(BotDefaultAnswers.email.ToLower())) { 
                    var f1 = FormDialog.FromForm(this.FindUserEmailDialog, FormOptions.PromptInStart);
                    context.Call(f1, this.ResumeAfterIdentificationAsync);
                }
                //store
                else if (optionSelected.ToLower().Contains(BotDefaultAnswers.store_card.ToLower())) {
                    var f2 = FormDialog.FromForm(this.FindUserCardDialog, FormOptions.PromptInStart);
                    context.Call(f2, this.ResumeAfterIdentificationAsync);
                }
                
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync(BotDefaultAnswers.tries_exceeded);
                context.Wait(this.MessageReceivedAsync);
            }
        }

        //Try identification again ?
        private async Task OnOptionRepeatIdentification(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                //yes
                if (optionSelected.ToLower().Contains(BotDefaultAnswers.Yes[0]))
                {
                    PromptDialog.Choice(context, this.OnOptionSelected,
                    new List<string>() { BotDefaultAnswers.Yes[1], BotDefaultAnswers.No[1] },
                    BotDefaultAnswers.identification_identified, BotDefaultAnswers.invalid_option, tries);
                }
                //no
                else if (optionSelected.ToLower().Contains(BotDefaultAnswers.No[0]))
                    context.Done<object>(null);
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync(BotDefaultAnswers.tries_exceeded);
                context.Wait(this.MessageReceivedAsync);
            }
        }

        //Associate store card?
        private async Task UpdateCustomerCardAsync(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                //yes
                if(optionSelected.ToLower().Contains(BotDefaultAnswers.Yes[0]))
                {
                    var customerCardDialog = FormDialog.FromForm(this.BuildCustomerCardDialog, FormOptions.PromptInStart);
                    context.Call(customerCardDialog, this.ResumeAfterCardUpdate);
                }
                //no
                else
                {
                    context.Done<object>(null);
                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync(BotDefaultAnswers.tries_exceeded);
                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task ResumeAfterUserCreation(IDialogContext context, IAwaitable<object> result)
        {
            var reply = context.MakeMessage();
            reply.Text = BotDefaultAnswers.identification_end_registration;
            await context.PostAsync(reply);
            PromptDialog.Choice(context, this.UpdateCustomerCardAsync, 
                new List<string>() { BotDefaultAnswers.Yes[1], BotDefaultAnswers.No[1] }, 
                BotDefaultAnswers.identification_card, BotDefaultAnswers.invalid_option, tries);
        }

        private async Task ResumeAfterIdentificationAsync(IDialogContext context, IAwaitable<object> result)
        {
            if (try_again)
            {
                await context.PostAsync(BotDefaultAnswers.identification_fail);

                PromptDialog.Choice(context, this.OnOptionRepeatIdentification,
                        new List<string>() { BotDefaultAnswers.Yes[1], BotDefaultAnswers.No[1] },
                        BotDefaultAnswers.identification_tryAgain, BotDefaultAnswers.invalid_option, tries);
            }
            else
                await context.PostAsync(BotDefaultAnswers.identification_conversation_retrieved);

            context.Done<object>(null);
        }

        private async Task ResumeAfterCardUpdate(IDialogContext context, IAwaitable<object> result)
        {
            var reply = context.MakeMessage();
            reply.Text = BotDefaultAnswers.identification_card_saved;
            await context.PostAsync(reply);
            context.Done<object>(null);
        }

        /*
         * FORMS
         */

        //New User
        private IForm<UserQuery> BuildCreateUserDialog()
        {
            OnCompletionAsyncDelegate<UserQuery> RegisterUser = async (context, state) =>
            {
                User user = UserController.getUser(context.Activity.ChannelId);
                UserController.SetCustomerName(user,state.Name);
                UserController.SetEmail(user,state.Email);
                ContextController.CreateContext(user);
            };

            var form = new FormBuilder<UserQuery>()
                .Field(nameof(UserQuery.Name))
                .Field(nameof(UserQuery.Email))
                .OnCompletion(RegisterUser)
                .Confirm(async (state) =>
                {
                    var r = BotDefaultAnswers.UserInfoConfirmation(state.Name, state.Email);

                    var p = new PromptAttribute(r);
                    return p;
                });
            form.Configuration.Yes = BotDefaultAnswers.Yes.ToArray();
            form.Configuration.No = BotDefaultAnswers.No.ToArray();

            return form.Build();
        }

        //User Identification
        private IForm<UserQuery> FindUserEmailDialog()
        {
            OnCompletionAsyncDelegate<UserQuery> AddChannelUser = async (context, state) =>
            {
                User realUser = UserController.getUserByEmail(state.Email);

                if (realUser != null)
                {
                    User tmpUser = UserController.getUser(context.Activity.ChannelId);

                    UserController.AddChannel(realUser, context.Activity.ChannelId);

                    UserController.MergeUsers(tmpUser, realUser);
                    UserController.DeleteUser(tmpUser);
                    ContextController.DeleteContext(ContextController.GetContext(tmpUser.Id));
                    //TODO delete CRM

                    try_again = false;
                }
                else
                {
                    try_again = true;
                }
            };

            var form = new FormBuilder<UserQuery>()
               .Field(nameof(UserQuery.Email))
               .OnCompletion(AddChannelUser)
               .Confirm(async (state) =>
               {
                   var r = BotDefaultAnswers.UserInfoConfirmation(state.Email);

                   var p = new PromptAttribute(r);
                   return p;
               });
            form.Configuration.Yes = BotDefaultAnswers.Yes.ToArray();
            form.Configuration.No = BotDefaultAnswers.No.ToArray();

            return form.Build();
        }

        //User Identification
        private IForm<UserQuery> FindUserCardDialog()
        {
            OnCompletionAsyncDelegate<UserQuery> AddChannelUser = async (context, state) =>
            {
                User realUser = UserController.getUserByCard(state.CustomerCard);

                if (realUser != null)
                {
                    User tmpUser = UserController.getUser(context.Activity.ChannelId);

                    UserController.AddChannel(realUser, context.Activity.ChannelId);

                    UserController.MergeUsers(tmpUser, realUser);
                    UserController.DeleteUser(tmpUser);
                    ContextController.DeleteContext(ContextController.GetContext(tmpUser.Id));
                    //TODO delete CRM

                    try_again = false;
                }
                else
                {
                    try_again = true;
                }
            };

            var form = new FormBuilder<UserQuery>()
               .Field(nameof(UserQuery.CustomerCard))
               .OnCompletion(AddChannelUser)
               .Confirm(async (state) =>
               {
                   var r = BotDefaultAnswers.UserInfoConfirmation(state.CustomerCard);

                   var p = new PromptAttribute(r);
                   return p;
               });
            form.Configuration.Yes = BotDefaultAnswers.Yes.ToArray();
            form.Configuration.No = BotDefaultAnswers.No.ToArray();

            return form.Build();
        }

        //Card Association
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
                    var r = BotDefaultAnswers.UserCardConfirmation(state.CustomerCard);
                    var p = new PromptAttribute(r);
                    return p;
                });
            form.Configuration.Yes = BotDefaultAnswers.Yes.ToArray();
            form.Configuration.No = BotDefaultAnswers.No.ToArray();

            return form.Build();
        }
    }
}