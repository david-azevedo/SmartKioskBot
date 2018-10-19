using Microsoft.Bot.Builder.Dialogs;
using System.Threading.Tasks;
using static SmartKioskBot.Helpers.AdaptiveCardHelper;
using System;
using Microsoft.Bot.Connector;
using static SmartKioskBot.Helpers.Constants;
using Newtonsoft.Json.Linq;
using static SmartKioskBot.Models.Context;
using System.Collections.Generic;
using SmartKioskBot.Models;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class MenuDialog : IDialog<object>
    {
        public User user = null;
        public State state;

        public enum State { INIT, INPUT_HANDLE}

        public MenuDialog(User user, State state)
        {
            this.user = user;
            this.state = state;
        }

        public async Task StartAsync(IDialogContext context)
        {
            switch (state)
            {
                case State.INIT:
                    var reply = context.MakeMessage();
                    reply.Attachments.Add(await getCardAttachment(CardType.MENU));
                    await context.PostAsync(reply);
                    break;
            }

            context.Wait(InputHandler);
        }

        private async Task InputHandler(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var activity = await argument as Activity;

            //Received a Message
            if (activity.Text != null)
                context.Done(new CODE(DIALOG_CODE.PROCESS_LUIS, activity));
            //Received an Event
            else if (activity.Value != null)
                await EventHandler(context, activity);
            else
                context.Done(new CODE(DIALOG_CODE.DONE));
        }

        private async Task EventHandler(IDialogContext context, Activity activity)
        {
            JObject json = activity.Value as JObject;
            List<InputData> data = getReplyData(json);

            //have mandatory info
            if (data.Count >= 2)
            {
                //json structure is correct
                if (data[0].attribute == REPLY_ATR && data[1].attribute == DIALOG_ATR)
                {
                    ClickType click = getClickType(data[0].value);

                    if (data[1].value.Equals(getDialogName(DialogType.MENU)) &&
                        click != ClickType.NONE &&
                        click.Equals(ClickType.MENU))
                    {
                        switch (data[0].value)
                        {
                            case "menu_session":
                                context.Call(new AccountDialog(user, AccountDialog.State.INIT), 
                                    ResumeAfterDialogCall);
                                break;
                            case "menu_filter":
                                context.Call(new FilterDialog(user, new List<Filter>(), FilterDialog.State.INIT), 
                                    ResumeAfterDialogCall);
                                break;
                            case "menu_comparator":
                                context.Call(new CompareDialog(user, CompareDialog.State.INIT),
                                    ResumeAfterDialogCall);
                                break;
                            case "menu_recommendations":
                                context.Call(
                                    new RecommendationDialog(user,RecommendationDialog.State.INIT),
                                    ResumeAfterDialogCall);
                                break;
                            case "menu_wishlist":
                                context.Call(
                                    new WishListDialog(user,WishListDialog.State.INIT), 
                                    ResumeAfterDialogCall);
                                break;
                            case "menu_stores":
                                //Add call to stores
                                context.Wait(InputHandler);
                                break;
                            case "menu_help":
                                //Add call to help
                                context.Wait(InputHandler);
                                break;
                            case "menu_info":
                                var reply = context.MakeMessage();
                                reply.Attachments.Add(await getCardAttachment(CardType.INFO_MENU));
                                await context.PostAsync(reply);
                                context.Wait(InputHandler);
                                break;
                        }
                    }
                    else
                        context.Done(new CODE(DIALOG_CODE.PROCESS_EVENT, activity));
                }
                else
                    context.Done(new CODE(DIALOG_CODE.DONE));
            }
            else
                context.Done(new CODE(DIALOG_CODE.DONE));
        }

        private async Task ResumeAfterDialogCall(IDialogContext context, IAwaitable<object> result)
        {
            CODE code = await result as CODE;
            if (code.dialog == DialogType.MENU)
                await EventHandler(context, code.activity);
            context.Done(code);
        }
    }
}