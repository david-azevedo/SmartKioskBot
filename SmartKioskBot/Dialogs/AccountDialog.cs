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

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class AccountDialog : IDialog<object>
    {
        private User user;
  
        public AccountDialog(User user)
        {
            this.user = user;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await ViewAccountDialog(context, null);
        }

        public async Task ViewAccountDialog(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var reply = context.MakeMessage();
            Attachment att = await getCardAttachment(CardType.VIEW_ACCOUNT);

            JObject json = att.Content as JObject;
            AccountLogic.SetAccountCardFields(json, user,false);

            reply.Attachments.Add(att);
            await context.PostAsync(reply);

            context.Wait(InputHandler);
        }
        public async Task InputHandler(IDialogContext context, IAwaitable<object> argument)
        {
            var activity = await argument as Activity;

            //Received a Message
            if (activity.Text != null)
            {
                context.Done(new CODE(DIALOG_CODE.PROCESS_LUIS, activity as IMessageActivity));
            }

            //Received an Event
            else if (activity.Value != null)
            {
                JObject json = activity.Value as JObject;
                CardType type = getReplyType(json);

                if (type.Equals(CardType.VIEW_ACCOUNT)) {

                    //edit card
                    var reply = context.MakeMessage();

                    Attachment att = await getCardAttachment(CardType.EDIT_ACCOUNT);
                    JObject content = att.Content as JObject;
                    AccountLogic.SetAccountCardFields(content, user, true);
                    reply.Attachments.Add(att);
                    await context.PostAsync(reply);

                    context.Wait(InputHandler);
                    
                }
                else if(type.Equals(CardType.EDIT_ACCOUNT)){ 
                    var data = getReplyData(json);
                    var i = 1;
                }

            }
            else
                context.Done(new CODE(DIALOG_CODE.DONE));
        }
       
    }
}
