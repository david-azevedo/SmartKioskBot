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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static SmartKioskBot.Models.Context;
using AdaptiveCards;
using SmartKioskBot.Logic;
using MongoDB.Bson;
using Microsoft.Bot.Builder.Luis;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class FilterDialog : IDialog<object>
    {
        private User user;
        private List<Filter> filters;
        private ObjectId last_fetch_id;

        public FilterDialog(User user, List<Filter> filters_luis)
        {
            this.user = user;
            this.filters = filters_luis;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await FilterAsync(context, null);
        }

        public async Task FilterAsync(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            
            var filtersRetrieved = ContextController.getFilters(user);

            // join the retrieved filters with the added ones
            foreach (Filter f1 in filtersRetrieved)
            {
                bool exists = false;
                foreach (Filter f2 in filters)
                    if (f1.FilterName == f2.FilterName && f1.Value == f2.Value)
                        exists = true;
                if (!exists)
                {
                    filters.Add(f1);
                    ContextController.AddFilter(user, f1.FilterName, f1.Operator, f1.Value);
                    CRMController.AddFilterUsage(user.Id, user.Country, f1);
                }
            }

            // search result
            List<Product> products = ProductController.getProductsFilter(
                FilterLogic.GetJoinedFilter(filters),
                Constants.N_ITEMS_CARROUSSEL,
                last_fetch_id);

            last_fetch_id = products[products.Count - 1].Id;

            var reply = context.MakeMessage();
            var text = "";

            if (products.Count > 0)
                text = BotDefaultAnswers.getFilter(BotDefaultAnswers.State.SUCCESS) + "  \n";
            else
                text = BotDefaultAnswers.getFilter(BotDefaultAnswers.State.FAIL) + "  \n";

            //display current filters
            foreach (Filter f in filters)
                text += f.FilterName + f.Operator + f.Value + ", ";

            await context.PostAsync(text);

            //display products 
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            List<Attachment> cards = new List<Attachment>();

            for (var i = 0; i < products.Count() && i < 7; i++)
                cards.Add(ProductCard.GetProductCard(products[i], ProductCard.CardType.SEARCH).ToAttachment());

            reply.Attachments = cards;
            await context.PostAsync(reply);

            //Check if pagination is needed
            if(products.Count < Constants.N_ITEMS_CARROUSSEL)
                context.Done<object>(null);
            else
            {
                reply = context.MakeMessage();
                reply.Attachments.Add(Common.PaginationCardAttachment());
                await context.PostAsync(reply);

                context.Wait(this.PaginationHandler);
            }
        }

        public async Task PaginationHandler(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result as Activity;

            if (activity.Text != null)
            {
                if (activity.Text.Equals(BotDefaultAnswers.next_pagination))
                    await FilterAsync(context,null);
                else
                    context.Done<object>(null);
            }
            else
                context.Done<object>(null);

            context.Done<object>(null);
        }

        public static IMessageActivity CleanAllFilters(IDialogContext context, User user)
        {
            ContextController.CleanFilters(user);
            var reply = context.MakeMessage();
            reply.Text = BotDefaultAnswers.getCleanAllFilters();
            return reply;
        }

        public static IMessageActivity CleanFilter(IDialogContext _context, User user, Context context, IList<EntityRecommendation> entities)
        {
            var reply = _context.MakeMessage();

            foreach (EntityRecommendation e in entities)
            {
                string filtername = "";

                if (e.Type.Contains("filter-type"))
                    filtername = e.Type.Remove(0, e.Type.LastIndexOf(":") + 1);
                else if (e.Type == "memory-type")
                {
                    if (e.Entity == "ram") filtername = "ram";
                    else filtername = "tipo_armazenamento";
                }
                else if (e.Type == "brand")
                    filtername = "marca";
                else if (e.Type == "cpu")
                    filtername = "familia_cpu";
                else if (e.Type == "gpu")
                    filtername = "placa_grafica";

                if (filtername != "")
                {
                    var removedIdx = -1;
                    for(var i = 0; i < context.Filters.Count(); i++)
                    {
                        if (context.Filters[i].FilterName == filtername)
                        {
                            removedIdx = i;
                            reply.Text = BotDefaultAnswers.getRemovedFilter(BotDefaultAnswers.State.SUCCESS, filtername);
                            ContextController.RemFilter(user, filtername);
                        }
                    }

                    if (removedIdx == -1)
                        reply.Text = BotDefaultAnswers.getRemovedFilter(BotDefaultAnswers.State.FAIL, filtername);
                }
            }

            context = ContextController.GetContext(user.Id);
            //display current filters
            if (context.Filters.Count() == 0)
                reply.Text += "  \n  \n" + BotDefaultAnswers.getViewFilters(BotDefaultAnswers.State.FAIL);
            else
            {
                reply.Text += "  \n  \n" + BotDefaultAnswers.getViewFilters(BotDefaultAnswers.State.SUCCESS) + "  \n";
                foreach (Filter f in context.Filters)
                    reply.Text += f.FilterName + f.Operator + f.Value + ", ";
            }

            return reply;
        }
    }
}
