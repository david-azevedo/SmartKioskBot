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
using System.Threading;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class FilterDialog : IDialog<object>
    {
        private User user;
        private List<Filter> filters;
        private List<Filter> filters_received;  //from luis
        private ObjectId last_fetch_id;
        private int page = 1;
        private State state;

        public enum State { INIT, FILTER_PREVIOUS, FILTER, FILTER_AGAIN, CLEAN, CLEAN_ALL };

        public FilterDialog(User user, List<Filter> filters_luis, State state)
        {
            this.user = user;
            this.filters_received = filters_luis;
            this.filters = new List<Filter>();
            this.state = state;

            if (filters_received.Count == 0)
                this.state = State.INIT;
        }

        public async Task StartAsync(IDialogContext context)
        {
            //for guided dialog
            switch (this.state)
            {
                case State.INIT:
                case State.FILTER_AGAIN:
                    await InitDialog(context, null);
                    break; ;
                case State.FILTER:
                case State.FILTER_PREVIOUS:
                    await FilterAsync(context, null);
                    break;
                case State.CLEAN:
                    break;
                case State.CLEAN_ALL:
                    break;
                default:
                    await FilterAsync(context, null);
                    break;
            }
        }

        public async Task InitDialog(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var reply = context.MakeMessage();
            Attachment att = await getCardAttachment(CardType.FILTER);
            
            //Fills the form with the previous choosen filters
            if (state.Equals(State.FILTER_AGAIN))
            {
                JObject json = att.Content as JObject;
                SetFilterCardValue(json, filters);
            }
            //reset filters (they will be added again in the filtering process)
            filters = new List<Filter>();

            //send form
            reply.Attachments.Add(att);
            await context.PostAsync(reply);

            context.Wait(InputHandler);
        }

        public async Task FilterAsync(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            // join the retrieved filters with the added ones
            // in case the user entered the filters manually
            if (this.state.Equals(State.FILTER_PREVIOUS))
            {
                filters = ContextController.getFilters(user);

                foreach (Filter f1 in filters_received)
                {
                    bool exists = false;
                    foreach (Filter f2 in filters)
                        if (f1.Equals(f2))
                            exists = true;
                    if (!exists)
                    {
                        filters.Add(f1);
                        ContextController.AddFilter(user, f1.FilterName, f1.Operator, f1.Value);
                        CRMController.AddFilterUsage(user.Id, user.Country, f1);
                    }
                }
            }
            
            // search products based on the last fetch id (inclusive)
            // search will fetch +1 product to know if pagination is needed
            List<Product> products = ProductController.getProductsFilter(
                FilterLogic.GetJoinedFilter(filters),
                Constants.N_ITEMS_CARROUSSEL + 1,
                last_fetch_id);
            
            if(products.Count > 1)
                last_fetch_id = products[products.Count - 2].Id;

            var reply = context.MakeMessage();
            var text = "";

            if (products.Count > 0)
                text = BotDefaultAnswers.getFilter(BotDefaultAnswers.State.SUCCESS,page) + "  \n";
            else
                text = BotDefaultAnswers.getFilter(BotDefaultAnswers.State.FAIL,page) + "  \n";

            //display current filters
            for(int i = 0; i < filters.Count; i++)
            {
                text += filters[i].FilterName + filters[i].Operator + filters[i].Value;
                if (i != filters.Count - 1)
                    text += ", ";
            }

            await context.PostAsync(text);

            bool done = false;
            List<ButtonType> buttons = new List<ButtonType>();

            //show products
            if (products.Count > 0)
            {
                //display products 
                reply = context.MakeMessage();
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                List<Attachment> cards = new List<Attachment>();

                //limit 
                for (var i = 0; i < products.Count && i < Constants.N_ITEMS_CARROUSSEL; i++)
                    cards.Add(ProductCard.GetProductCard(products[i], ProductCard.CardType.SEARCH).ToAttachment());

                reply.Attachments = cards;
                await context.PostAsync(reply);

                //Check if pagination is needed
                if (products.Count > Constants.N_ITEMS_CARROUSSEL)
                    buttons.Add(ButtonType.PAGINATION);
            }

            buttons.Add(ButtonType.FILTER_AGAIN);

            //show options
            reply = context.MakeMessage();
            reply.Attachments.Add(getCardButtonsAttachment(buttons, DialogType.FILTER));
            await context.PostAsync(reply);

            context.Wait(this.InputHandler);
        }

        public async Task InputHandler(IDialogContext context, IAwaitable<object> argument)
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

                //has the mandatory info
                if (data.Count >= 2)
                {
                    //json structure is correct
                    if (data[0].attribute == REPLY_ATR && data[1].attribute == DIALOG_ATR)
                    {
                        ClickType click = getClickType(data[0].value);

                        if (data[1].value.Equals(getDialogName(DialogType.FILTER)) &&
                            click != ClickType.NONE)
                        {
                            switch (click)
                            {
                                case ClickType.PAGINATION:
                                    page++;
                                    done_ok = false;
                                    await StartAsync(context);
                                    break;
                                case ClickType.FILTER:
                                    done_ok = false;
                                    data.RemoveAt(0);   //remove reply_type
                                    data.RemoveAt(0);   //remove dialog
                                    this.filters = FilterLogic.GetFilterFromForm(data);
                                    this.state = State.FILTER;
                                    await StartAsync(context);
                                    break;
                                case ClickType.FILTER_AGAIN:
                                    done_ok = false;
                                    this.state = State.FILTER_AGAIN;
                                    await StartAsync(context);
                                    break;
                            }
                        }
                    }
                }
            }

            if(done_ok)
                context.Done(new CODE(DIALOG_CODE.DONE));
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
                    if (e.Entity == "ram") filtername = Constants.ram_filter;
                    else filtername = Constants.storage_type_filter;
                }
                else if (e.Type == "brand")
                    filtername = Constants.brand_filter;
                else if (e.Type == "cpu")
                    filtername = Constants.cpu_family_filter;
                else if (e.Type == "gpu")
                    filtername = Constants.gpu_filter;

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
