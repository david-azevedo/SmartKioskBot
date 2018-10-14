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
        private User user = null;

        public MenuDialog(User user)
        {
            this.user = user;
        }

        public async Task StartAsync(IDialogContext context)
        {
            var reply = context.MakeMessage();
            reply.Attachments.Add(await getCardAttachment(CardType.MENU));
            await context.PostAsync(reply);

            context.Wait(InputHandler);
        }

        private async Task InputHandler(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var activity = await argument as Activity;

            //close dialog at the end without more processing
            bool done_ok = true;

            //Received a Message
            if (activity.Text != null)
            {
                done_ok = false;
                context.Done(new CODE(DIALOG_CODE.PROCESS_LUIS, activity as IMessageActivity));
            }
            //Received an Event
            else if (activity.Value != null)
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
                            done_ok = false;

                            switch (data[0].value)
                            {
                                case "menu_session":
                                    //Add call to account
                                    context.Wait(InputHandler);
                                    break;
                                case "menu_filter":
                                    var next_dialog = new FilterDialog(user, new List<Filter>(), FilterDialog.State.INIT);
                                    context.Call(next_dialog, ResumeAfterDialogCall);
                                    break;
                                case "menu_comparator":
                                    context.Call(new CompareDialog(user), ResumeAfterDialogCall);
                                    break;
                                case "menu_recommendations":
                                    context.Call(new RecommendationDialog(user), ResumeAfterDialogCall);
                                    break;
                                case "menu_wishlist":
                                    context.Call(new WishListDialog(user), ResumeAfterDialogCall);
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
                    }
                }
            }

            if(done_ok)
                context.Done(new CODE(DIALOG_CODE.DONE));
        }

        private async Task ResumeAfterDialogCall(IDialogContext context, IAwaitable<object> result)
        {
            CODE code = await result as CODE;
            context.Done(code);
        }
    }
}