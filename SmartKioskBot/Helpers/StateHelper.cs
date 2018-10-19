using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static SmartKioskBot.Models.Context;

namespace SmartKioskBot.Helpers
{
    public class StateHelper
    {
        public static string WISHLIST_ATR = "wishlist";
        public static string FILTERS_ATR = "filters";
        public static string COMPARATOR_ATR = "comparator";
        public static string FILTER_COUNT_ATR = "filter-count";
        public static string PRODUCT_CLICKS_ATR = "product-clicks";
        public static string USER_ID_ATR = "user-id";

        public static void AddFilter(Filter f, IDialogContext context)
        {
            var user_data = context.UserData;
            List<Filter> filters_state = user_data.GetValue<List<Filter>>(FILTERS_ATR);
            filters_state.Add(f);
            user_data.SetValue<List<Filter>>(FILTERS_ATR, filters_state);
        }

        public static void SetFilters(List<Filter> new_filters, IDialogContext context)
        {
            var user_data = context.UserData;
            user_data.SetValue<List<Filter>>(FILTERS_ATR, new_filters);
        }
    }
}