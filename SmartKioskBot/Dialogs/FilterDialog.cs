using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using SmartKioskBot.Controllers;
using SmartKioskBot.Helpers;
using SmartKioskBot.Models;
using SmartKioskBot.UI;
using System;
using System.Collections.Generic;
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
    public class FilterDialog : IDialog<object>
    {
        private ObjectId last_fetch_id;
        private int page = 1;
        private State state;

        public enum State {
            INIT,               //Form
            FILTER,             //Perform filtering
            FILTER_AGAIN,       //Form with previous filters
            CLEAN_ALL,      
            INPUT_HANDLER };    //Handle input

        public FilterDialog(State state)
        {
            this.state = state;
        }

        public async Task StartAsync(IDialogContext context)
        {
            //for guided dialog
            switch (this.state)
            {
                case State.INIT:
                case State.FILTER_AGAIN:
                    await InitDialog(context, null);
                    break;
                case State.FILTER:
                    await FilterAsync(context, null);
                    break;
                case State.CLEAN_ALL:
                    break;
                case State.INPUT_HANDLER:
                    context.Wait(InputHandler);
                    break;
                default:
                    context.Wait(FilterAsync);
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
                await Interactions.SendMessage(context, "Modifique os seus requisitos para efetuarmos uma nova pesquisa no nosso catálogo.", 0, 2000);
                JObject json = JObject.Parse(att.Content.ToString());
                FilterLogic.SetFilterCardValue(json, StateHelper.GetFilters(context));
            }
            else
                await Interactions.SendMessage(context, "Preencha o formulário abaixo com as suas preferências para que ue possa reunir os melhores produtos para si.", 0, 2000);

            //reset filters (they will be added again in the filtering process)
            StateHelper.SetFilters(new List<Filter>(), context);

            //send form
            reply.Attachments.Add(att);
            await context.PostAsync(reply);

            context.Wait(InputHandler);
        }

        public async Task FilterAsync(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            List<Filter> filters = StateHelper.GetFilters(context);
            
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
                text = Interactions.getFilter(Interactions.State.SUCCESS,page) + "  \n";
            else
                text = Interactions.getFilter(Interactions.State.FAIL,page) + "  \n";

            //display current filters
            for(int i = 0; i < filters.Count; i++)
            {
                text += filters[i].FilterName + filters[i].Operator + filters[i].Value;
                if (i != filters.Count - 1)
                    text += ", ";
            }

            await Interactions.SendMessage(context, text, 0, 3000);

            bool done = false;
            List<ButtonType> buttons = new List<ButtonType>();

            string button_text = "";

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
                {
                    button_text = "Não consegui trazer todos os produtos do catálogo com esses requisitos. Se quiser ver mais produtos clique no botão abaixo.";
                    buttons.Add(ButtonType.PAGINATION);
                }
            }

            button_text += "\nPara alterar os parâmetros da pesquisa carregue no respetivo botão.";
            await Interactions.SendMessage(context, button_text, 0, 2000);

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

            //Received a Message
            if (activity.Text != null)
                context.Done(new CODE(DIALOG_CODE.PROCESS_LUIS, activity));
            //Received an Event
            else if (activity.Value != null)
                await EventHandler(context, activity);
            //Other
            else
                context.Done(new CODE(DIALOG_CODE.DONE));
        }

        public async Task EventHandler(IDialogContext context, Activity activity)
        {
            JObject json = activity.Value as JObject;
            List<InputData> data = getReplyData(json);

            //has the mandatory info
            if (data.Count >= 2)
            {
                //json structure is correct
                if (data[0].attribute == REPLY_ATR && data[1].attribute == DIALOG_ATR)
                {
                    ClickType event_click = getClickType(data[0].value);
                    DialogType event_dialog = getDialogType(data[1].value);

                    //event of this dialog
                    if (event_dialog == DialogType.FILTER &&
                        event_click != ClickType.NONE)
                    {
                        switch (event_click)
                        {
                            case ClickType.PAGINATION:
                                page++;
                                await StartAsync(context);
                                break;
                            case ClickType.FILTER:
                                data.RemoveAt(0);   //remove reply_type
                                data.RemoveAt(0);   //remove dialog

                                List<Filter> filters= FilterLogic.GetFilterFromForm(data);
                                StateHelper.SetFilters(filters, context);

                                foreach (Filter f in filters)
                                    StateHelper.AddFilterCount(context, f);

                                this.state = State.FILTER;
                                await StartAsync(context);
                                break;
                            case ClickType.FILTER_AGAIN:
                                this.state = State.FILTER_AGAIN;
                                await StartAsync(context);
                                break;
                        }
                    }
                    // event of other dialog
                    else
                    {
                        context.Done(new CODE(DIALOG_CODE.PROCESS_EVENT, activity, event_dialog));
                    }
                }
                else
                    context.Done(new CODE(DIALOG_CODE.DONE));
            }
            else
                context.Done(new CODE(DIALOG_CODE.DONE));
        }

        public static IMessageActivity CleanAllFilters(IDialogContext context)
        {
            StateHelper.CleanFilters(context);
            var reply = context.MakeMessage();
            reply.Text = Interactions.getCleanAllFilters();
            return reply;
        }
    }
}
