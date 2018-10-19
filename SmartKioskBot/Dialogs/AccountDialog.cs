using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using MongoDB.Driver;
using SmartKioskBot.Controllers;
using SmartKioskBot.Helpers;
using SmartKioskBot.Models;
using SmartKioskBot.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SmartKioskBot.Models.Context;
using SmartKioskBot.Logic;
using MongoDB.Bson;
using static SmartKioskBot.Helpers.Constants;
using static SmartKioskBot.Helpers.AdaptiveCardHelper;
using Newtonsoft.Json.Linq;

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

                var reply = context.MakeMessage();
                Attachment att = await getCardAttachment(CardType.EDIT_ACCOUNT);

                reply.Attachments.Add(att);
                await context.PostAsync(reply);

                context.Wait(InputHandler);

            }
            else
                context.Done(new CODE(DIALOG_CODE.DONE));
        }
       
    }
}
