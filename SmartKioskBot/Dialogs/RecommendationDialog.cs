using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using SmartKioskBot.Controllers;
using SmartKioskBot.Models;
using SmartKioskBot.UI;
using static SmartKioskBot.Models.Context;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class RecommendationDialog
    {
        public static Filter DEFAULT_RECOMMENDATION_FILTER = new Filter()
        {
            FilterName = "marca",
            Operator = "=",
            Value = "asus"
        };

        public static IMessageActivity ShowRecommendations(IDialogContext context, User user)
        {
            //fetch context
            Context myContext = ContextController.GetContext(user.Id);

            Filter[] userMostPopularFiltersArray = CRMController.GetMostPopularFilters(user.Id, 10);
            List<Filter> userMostPopularFilters;

            if(userMostPopularFiltersArray == null || userMostPopularFiltersArray.Length == 0)
            {
                userMostPopularFilters = new List<Filter>
                {
                    DEFAULT_RECOMMENDATION_FILTER
                };
            }
            else
            {
                userMostPopularFilters = new List<Filter>(userMostPopularFiltersArray);
            }
                
            List<Product> productsToRecommend = FilterDialog.GetProductsForUser(userMostPopularFilters);

            return ShowRecommendedProducts(context, productsToRecommend);
        }

        private static IMessageActivity ShowRecommendedProducts(IDialogContext context,List<Product> products)
        {
            var reply = context.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            List<Attachment> cards = new List<Attachment>();
  
            for (int i = 0; i < products.Count; i++)
            {
                cards.Add(ProductCard.GetProductCard(products[i], ProductCard.CardType.SEARCH).ToAttachment());
            }

            reply.Attachments = cards;
            return reply;
        }
    }
}