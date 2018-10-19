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
using static SmartKioskBot.Helpers.AdaptiveCardHelper;
using Newtonsoft.Json.Linq;

namespace SmartKioskBot.Dialogs
{
    /*TODO:
     * User is not passed, all information is kept in store memory
     * When logging in, the userid is stored and 
     */
    [Serializable]
    public class RootDialog : IDialog<Object>
    {
        private User user = null;
        private bool identified = false;

        public async Task StartAsync(IDialogContext context)
        {
            TryIdentification(context);
            context.Wait(InputHandler);
        }

        private async Task InputHandler(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            var i = 1;

            //message
            if (activity.Text != null)
                await MessageHandler(context, activity);
            //event
            else if (activity.Value != null)
            {
                JObject json = activity.Value as JObject;
                List<InputData> data = getReplyData(json);

                if (data[0].attribute == REPLY_ATR && data[1].attribute == DIALOG_ATR)
                    await EventHandler(context, getDialogType(data[1].value), activity);
                else
                    context.Wait(InputHandler);
            }
        }

        private async Task MessageHandler(IDialogContext context, Activity activity)
        {
            await context.Forward(new LuisProcessingDialog(user), ResumeAfterDialogCall, activity);
        }

        private async Task EventHandler(IDialogContext context, DialogType dialog, Activity message)
        {
            switch (dialog)
            {
                case DialogType.ACCOUNT:
                    //TODO: call dialog
                    break;
                case DialogType.COMPARE:
                    await context.Forward(
                        new CompareDialog(user, CompareDialog.State.INPUT_HANDLER),
                        ResumeAfterDialogCall, message);
                    break;
                case DialogType.FILTER:
                    await context.Forward(
                        new FilterDialog(user, new List<Context.Filter>(), FilterDialog.State.INPUT_HANDLER),
                        ResumeAfterDialogCall, message);
                    break;
                case DialogType.MENU:
                    await context.Forward(
                        new MenuDialog(user,MenuDialog.State.INPUT_HANDLE), 
                        ResumeAfterDialogCall, message);
                    break;
                case DialogType.RECOMMENDATION:
                    await context.Forward(
                        new RecommendationDialog(
                            user,RecommendationDialog.State.INPUT_HANDLE), 
                        ResumeAfterDialogCall, message);
                    break;
                case DialogType.STORE:
                    //TODO: call store dialog
                    break;
                case DialogType.TUTORIAL:
                    //TODO: calldialog
                    break;
                case DialogType.WISHLIST:
                    await context.Forward(
                        new WishListDialog(user,WishListDialog.State.INPUT_HANDLER), 
                        ResumeAfterDialogCall, message);
                    break;
            }
        }

        private async Task ResumeAfterDialogCall(IDialogContext context, IAwaitable<object> result)
        {
            if (result != null)
            {
                //code from child parent
                var c = await result as CODE;

                //dialog ended
                if (c.code == DIALOG_CODE.DONE)
                    context.Wait(InputHandler);
                //message to handle
                else if (c.dialog == DialogType.NONE)
                    await MessageHandler(context, c.activity);
                //event handle
                else
                    await EventHandler(context, c.dialog, c.activity);
            }
            else
                context.Wait(InputHandler);
        }

        private void TryIdentification(IDialogContext context)
        {
            if (identified == false && !context.Activity.From.Id.Equals("default-user"))
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
            context.Wait(InputHandler);
        }

    }
}