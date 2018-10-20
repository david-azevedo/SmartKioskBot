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
using SmartKioskBot.Helpers;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class AccountDialog : IDialog<object>
    {
        private State state;

        public enum State { INIT, INPUT_HANDLER };

        public AccountDialog(State state)
        {
            this.state = state;
        }

        public async Task StartAsync(IDialogContext context)
        {
            switch (state)
            {
                case State.INIT:
                    if (StateHelper.IsLoggedIn(context))
                        await ViewAccountDialog(context, null);
                    else
                        await NotLoginDialog(context, null);
                    break;
                case State.INPUT_HANDLER:
                    context.Wait(InputHandler);
                    break;
            }
        }

        public async Task NotLoginDialog(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var reply = context.MakeMessage();
            Attachment att = await getCardAttachment(CardType.NOT_LOGIN);

            reply.Attachments.Add(att);
            await context.PostAsync(reply);

            context.Wait(InputHandler);
        }

        public async Task ViewAccountDialog(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var reply = context.MakeMessage();
            Attachment att = await getCardAttachment(CardType.VIEW_ACCOUNT);

            JObject json = att.Content as JObject;
            AccountLogic.SetAccountCardFields(json,context, false);

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
                        var reply = context.MakeMessage();
                        switch (click)
                        {
                            case ClickType.LOGIN:
                                Attachment att = await getCardAttachment(CardType.LOGIN);
                                reply.Attachments.Add(att);
                                await context.PostAsync(reply);
                                context.Wait(InputHandler);
                                break;
                            case ClickType.LOGIN_START:
                                var fail_text = AccountLogic.Login(data, context);
                                if (fail_text != "")
                                    await context.PostAsync(fail_text);

                                state = State.INIT;
                                await StartAsync(context);
                                break;
                            case ClickType.REGISTER:
                                Attachment att1 = await getCardAttachment(CardType.REGISTER);
                                reply.Attachments.Add(att1);
                                await context.PostAsync(reply);
                                context.Wait(InputHandler);
                                break;
                            case ClickType.REGISTER_SAVE:
                                fail_text = AccountLogic.Register(data, context);
                                if (fail_text != "")
                                    await context.PostAsync(fail_text);

                                state = State.INIT;
                                await StartAsync(context);
                                break;
                            case ClickType.ACCOUNT_EDIT:
                                Attachment att3 = await getCardAttachment(CardType.EDIT_ACCOUNT);
                                JObject content = att3.Content as JObject;
                                AccountLogic.SetAccountCardFields(content, context, true);
                                reply.Attachments.Add(att3);
                                await context.PostAsync(reply);
                                context.Wait(InputHandler);
                                break;
                            case ClickType.ACCOUNT_SAVE:
                                fail_text = AccountLogic.SaveAccountInfo(data, context);
                                if (fail_text != "")
                                    await context.PostAsync(fail_text);

                                state = State.INIT;
                                await StartAsync(context);
                                break;
                            case ClickType.LOGOUT:
                                await context.PostAsync("A sua sessão foi terminada. Não se preocupe, ainda me lembro da nossa conversa.");
                                context.Done<CODE>(new CODE(DIALOG_CODE.RESET));
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
