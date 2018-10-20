using Microsoft.Bot.Builder.Dialogs;
using MongoDB.Bson;
using SmartKioskBot.Controllers;
using SmartKioskBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static SmartKioskBot.Models.Context;
using static SmartKioskBot.Models.Customer;

namespace SmartKioskBot.Helpers
{
    public abstract class StateHelper
    {
        //state properties name
        public static string WISHLIST_ATR = "wishlist";
        public static string FILTERS_ATR = "filters";
        public static string COMPARATOR_ATR = "comparator";
        public static string FILTER_COUNT_ATR = "filter-count";
        public static string PRODUCT_CLICKS_ATR = "product-clicks";
        public static string USER_ATR = "user";
        public static string LOGIN_ATR = "login";

        public static void ResetUserData(IDialogContext context)
        {
            context.PrivateConversationData.SetValue<List<string>>(WISHLIST_ATR, new List<string>());
            context.PrivateConversationData.SetValue<List<Filter>>(FILTERS_ATR, new List<Filter>());
            context.PrivateConversationData.SetValue<List<string>>(COMPARATOR_ATR, new List<string>());
            context.PrivateConversationData.SetValue<List<FilterCount>>(FILTER_COUNT_ATR, new List<FilterCount>());
            context.PrivateConversationData.SetValue<List<ProductClicks>>(PRODUCT_CLICKS_ATR, new List<ProductClicks>());
            context.PrivateConversationData.SetValue<bool>(LOGIN_ATR, false);
        }

        public static void Login(IDialogContext context, User user)
        {
            Context user_context = ContextController.GetContext(user.Id);
            Customer user_crm = CRMController.GetCustomer(user.Id);

            var userdata = context.PrivateConversationData;

            //User login
            userdata.SetValue<bool>(LOGIN_ATR, true);

            //User info
            userdata.SetValue<User>(USER_ATR, user);

            //User context
            List<string> compare = new List<string>();
            List<string> wishes = new List<string>();

            foreach (ObjectId w in user_context.WishList.ToList())
                wishes.Add(w.ToString());

            foreach (ObjectId c in user_context.Comparator.ToList())
                compare.Add(c.ToString());

            userdata.SetValue<List<string>>(WISHLIST_ATR, wishes);
            userdata.SetValue<List<string>>(COMPARATOR_ATR, compare);

            //User crm
            userdata.SetValue<List<FilterCount>>(FILTER_COUNT_ATR, user_crm.FiltersCount.ToList());
            userdata.SetValue<List<ProductClicks>>(PRODUCT_CLICKS_ATR, user_crm.ProductsClicks.ToList());
        }

        /*
         * GET
         */

        public static bool IsLoggedIn(IDialogContext context)
        {
            return context.PrivateConversationData.GetValue<bool>(LOGIN_ATR);
        }
        
        public static User GetUser(IDialogContext context)
        {
            return context.PrivateConversationData.GetValue<User>(USER_ATR);
        }

        public static List<string> GetComparatorItems(IDialogContext context)
        {
            return context.PrivateConversationData.GetValue<List<string>>(COMPARATOR_ATR);
        }

        public static List<Filter> GetFilters(IDialogContext context)
        {
            return context.PrivateConversationData.GetValue<List<Filter>>(FILTERS_ATR);
        }
        
        public static List<FilterCount> GetFiltersCount(IDialogContext context)
        {
            return context.PrivateConversationData.GetValue<List<FilterCount>>(FILTER_COUNT_ATR);
        }
        
        public static List<string> GetWishlistItems(IDialogContext context)
        {
            return context.PrivateConversationData.GetValue<List<string>>(WISHLIST_ATR);
        }
        
        public static List<ProductClicks> GetProductClicks(IDialogContext context)
        {
            return context.PrivateConversationData.GetValue<List<ProductClicks>>(PRODUCT_CLICKS_ATR);
        }

        /*
         * SET
         */

        public static void SetUser(IDialogContext context, User user)
        {
            context.PrivateConversationData.SetValue<User>(USER_ATR,user);
        }

        public static bool AddFilter(Filter f, IDialogContext context)
        {
            foreach(Filter fl in GetFilters(context))
            {
                if (fl.FilterName.Equals(f.FilterName) && fl.Value.Equals(f.Value))
                    return false;
            }

            var user_data = context.PrivateConversationData;
            List<Filter> filters_state = user_data.GetValue<List<Filter>>(FILTERS_ATR);
            filters_state.Add(f);
            user_data.SetValue<List<Filter>>(FILTERS_ATR, filters_state);
            return true;
        }

        public static void SetFilters(List<Filter> new_filters, IDialogContext context)
        {
            var user_data = context.PrivateConversationData;
            user_data.SetValue<List<Filter>>(FILTERS_ATR, new_filters);
        }

        public static void AddFilterCount(IDialogContext context, Filter f)
        {
            List<FilterCount> counts = GetFiltersCount(context);

            foreach (FilterCount c in counts)
                if (c.Filter.FilterName.Equals(f.FilterName) && c.Filter.Value.Equals(f.Value))
                {
                    c.NSearches += 1;
                    return;
                }

            FilterCount cnt = new FilterCount();
            cnt.Filter = f;
            cnt.NSearches = 1;

            counts.Add(cnt);
            context.PrivateConversationData.SetValue<List<FilterCount>>(FILTER_COUNT_ATR,counts);
        }

        public static void CleanFilters(IDialogContext context)
        {
            context.PrivateConversationData.SetValue<List<Filter>>(FILTERS_ATR, new List<Filter>());
        }

        public static void AddItemComparator(IDialogContext context, string item)
        {
            List<string> items = GetComparatorItems(context);

            foreach (string i in items)
                if (i.Equals(item))
                    return;

            items.Add(item.ToString());
            context.PrivateConversationData.SetValue<List<string>>(COMPARATOR_ATR, items);
        }

        public static void RemItemComparator(IDialogContext context, string item)
        {
            List<string> items = GetComparatorItems(context);

            foreach(string i in items)
            {
                if (i.Equals(item))
                {
                    items.Remove(i);
                    context.PrivateConversationData.SetValue<List<string>>(COMPARATOR_ATR, items);
                    break;
                }
            }

        }

        public static void AddItemWishList(IDialogContext context, string item)
        {
            List<string> items = GetWishlistItems(context);

            foreach (string i in items)
                if (i.Equals(item))
                    return;

            items.Add(item);
            context.PrivateConversationData.SetValue<List<string>>(WISHLIST_ATR, items);
        }

        public static void RemItemWishlist(IDialogContext context, string item)
        {
            List<string> items = GetWishlistItems(context);

            foreach (string i in items)
            {
                if (i.Equals(item))
                {
                    items.Remove(i);
                    context.PrivateConversationData.SetValue<List<string>>(WISHLIST_ATR, items);
                    break;
                }
            }
        }

        public static void AddProductClick(IDialogContext context, string id)
        {
            List<ProductClicks> clicks = GetProductClicks(context);

            foreach(ProductClicks c in clicks)
            {
                if (c.ProductId.ToString().Equals(id))
                {
                    c.NClicks += 1;
                    return;
                }
            }

            ProductClicks pc = new ProductClicks();
            pc.NClicks = 1;
            pc.ProductId = new ObjectId(id);

            clicks.Add(pc);
            context.PrivateConversationData.SetValue<List<ProductClicks>>(PRODUCT_CLICKS_ATR, clicks);
        }
    }
}