using AdaptiveCards;
using Microsoft.Bot.Connector;
using SmartKioskBot.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartKioskBot.UI
{
    public abstract class Common
    {
        public static Attachment PaginationCardAttachment()
        {
            AdaptiveCard PaginationCard = new AdaptiveCard()
            {
                Actions = { new AdaptiveSubmitAction()
                    {
                        Title = BotDefaultAnswers.next_pagination,
                        Data = BotDefaultAnswers.next_pagination
                    }
                }
            };

            Attachment att = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = PaginationCard
            };

            return att;
        }
    }
}