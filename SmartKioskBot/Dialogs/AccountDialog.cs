using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using SmartKioskBot.Models;
using System;
using System.Threading.Tasks;
using static SmartKioskBot.Helpers.Constants;
using static SmartKioskBot.Helpers.AdaptiveCardHelper;
using Newtonsoft.Json.Linq;
using SmartKioskBot.Logic;
using System.Collections.Generic;
using SmartKioskBot.Controllers;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class AccountDialog : IDialog<object>
    {
        private User user;
        private State state;

        public enum State { INIT, INPUT_HANDLER };

        public AccountDialog(User user, State state)
        {
            this.user = user;
            this.state = state;
        }

        public async Task StartAsync(IDialogContext context)
        {
            switch (state)
            {
                case State.INIT:
                    await ViewAccountDialog(context, null);
                    break;
                case State.INPUT_HANDLER:
                    context.Wait(InputHandler);
                    break;
            }
        }

        public async Task ViewAccountDialog(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var reply = context.MakeMessage();
            Attachment att = await getCardAttachment(CardType.VIEW_ACCOUNT);

            this.user = UserController.getUser(user.Id);

            JObject json = att.Content as JObject;
            AccountLogic.SetAccountCardFields(json,user ,false);

            reply.Attachments.Add(att);
            await context.PostAsync(reply);

            context.Wait(InputHandler);
        }

        public async Task InputHandler(IDialogContext context, IAwaitable<object> argument)
        {
            var activity = await argument as Activity;

            //received message
            if (activity.Text != null)
                context.Done(new CODE(DIALOG_CODE.PROCESS_LUIS, activity));
            //received event
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

                    //process in this dialog
                    if (data[1].value.Equals(getDialogName(DialogType.ACCOUNT)) &&
                        click != ClickType.NONE)
                    {
                        switch (click)
                        {
                            case ClickType.ACCOUNT_EDIT:
                                var reply = context.MakeMessage();
                                Attachment att = await getCardAttachment(CardType.EDIT_ACCOUNT);
                                JObject content = att.Content as JObject;
                                AccountLogic.SetAccountCardFields(content, user, true);
                                reply.Attachments.Add(att);
                                await context.PostAsync(reply);
                                context.Wait(InputHandler);
                                break;
                            case ClickType.ACCOUNT_SAVE:
                                var fail_text = AccountLogic.SaveAccountInfo(data, user);
                                if (fail_text != "")
                                    await context.PostAsync(fail_text);

                                state = State.INIT;
                                await StartAsync(context);
                                break;
                            case ClickType.LOGOUT:
                                //TODO
                                break;
                        }
                    }
                    //process in parent dialog
                    else
                        context.Done(new CODE(DIALOG_CODE.PROCESS_EVENT, activity));
                }
                else
                    context.Done(new CODE(DIALOG_CODE.DONE));
            }
            else
                context.Done(new CODE(DIALOG_CODE.DONE));
        }
    }
}
