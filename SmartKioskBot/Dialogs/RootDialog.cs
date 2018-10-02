using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using static SmartKioskBot.Helpers.Constants;
using SmartKioskBot.Models;
using SmartKioskBot.Controllers;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<Object>
    {
        private User user = null;
        private bool identified = false;

        public async Task StartAsync(IDialogContext context)
        {
            TryIdentification(context);
            context.Wait(ProcessMessage);
        }

        private async Task ProcessMessage(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result;
            await context.Forward(new LuisProcessingDialog(user), ResumeAfterDialog, activity);
        }

        private async Task ResumeAfterDialog(IDialogContext context, IAwaitable<object> result)
        {
            if (result != null)
            {
                var c = await result as CODE;

                //handle child dialog response
                switch (c.value)
                {
                    case DIALOG_CODE.PROCESS_LUIS:
                        await context.Forward(new LuisProcessingDialog(user), ResumeAfterDialogInterrupt, c.message);
                        break;
                    case DIALOG_CODE.DONE:
                        context.Wait(ProcessMessage);
                        break;
                }
            }
            else
            {
                context.Wait(ProcessMessage);
            }
        }

        private async Task ResumeAfterDialogInterrupt(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result;
            context.Wait(ProcessMessage);
        }

        private void TryIdentification(IDialogContext context)
        {
            if (identified == false)
            {
                var activity = context.Activity;
                user = UserController.getUser(activity.ChannelId);
                if (user == null)
                {
                    var r = new Random();
                    UserController.CreateUser(activity.ChannelId, activity.From.Id, activity.From.Name, (r.Next(25) + 1).ToString());
                    user = UserController.getUser(activity.ChannelId);
                    ContextController.CreateContext(user);
                    CRMController.AddCustomer(user);
                    context.Call(new IdentificationDialog(), ResumeAfterIdent);
                }
                identified = true;
            }
        }

        private async Task ResumeAfterIdent(IDialogContext context, IAwaitable<object> result)
        {
            this.user = UserController.getUser(context.Activity.ChannelId);
            context.Wait(ProcessMessage);
        }

    }
}