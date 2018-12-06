using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using SmartKioskBot.Controllers;
using SmartKioskBot.Dialogs;
namespace SmartKioskBot.UI
{
    public abstract class IdentificationCards
    { 
        public static async Task DisplayHeroCard(IDialogContext context, String option, String identifier, String value)
        {
            var replyMessage = context.MakeMessage();
            Attachment attachment = GetProfileHeroCard(option, identifier, value); ;
            replyMessage.Attachments = new List<Attachment> { attachment };
            await context.PostAsync(replyMessage);
        }

        private static Attachment GetProfileHeroCard(String option, String identifier, String value)
        {
            var heroCard = new HeroCard
            {
                Title = "Confirme os dados introduzidos",
                Text = identifier + " : " + value,
                Buttons = getButtons(option, value)
            };

            return heroCard.ToAttachment();
        }

        private static List<CardAction> getButtons(String option, String value)
        {
            var buttons = new List<CardAction>();
            if (option.Equals("set-customer-email"))
            {
                buttons.Add(new CardAction(ActionTypes.PostBack, "Confirmar", value: Interactions.set_customer_email + " yes " + value));
                buttons.Add(new CardAction(ActionTypes.PostBack, "Cancelar", value: Interactions.set_customer_email + " no "));
            }
            else if (option.Equals("set-customer-card"))
            {
                buttons.Add(new CardAction(ActionTypes.PostBack, "Confirmar", value: Interactions.set_customer_card + " yes " + value));
                buttons.Add(new CardAction(ActionTypes.PostBack, "Cancelar", value: Interactions.set_customer_card + " no "));
            }
            else if (option.Equals("set-customer-name"))
            {
                buttons.Add(new CardAction(ActionTypes.PostBack, "Confirmar", value: Interactions.set_customer_name + " yes " + value));
                buttons.Add(new CardAction(ActionTypes.PostBack, "Cancelar", value: Interactions.set_customer_name + " no "));
            }
            else
            {
                buttons.Add(new CardAction(ActionTypes.PostBack, "Confirmar", value: Interactions.add_channel + " yes " + value));
                buttons.Add(new CardAction(ActionTypes.PostBack, "Cancelar", value: Interactions.add_channel + " no "));
            }

                return buttons;

        }
    }
}