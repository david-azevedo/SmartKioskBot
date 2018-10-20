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
    public class StateHelper
    {
        //state properties name
        public static string WISHLIST_ATR = "wishlist";
        public static string FILTERS_ATR = "filters";
        public static string COMPARATOR_ATR = "comparator";
        public static string FILTER_COUNT_ATR = "filter-count";
        public static string PRODUCT_CLICKS_ATR = "product-clicks";
        public static string USER_ID_ATR = "user-id";
        public static string USER_COUNTRY_ATR = "user-country";
        public static string USER_CARD_ID_ATR = "user-card";
        public static string USER_GENDER_ATR = "user-gender";
        public static string USER_EMAIL_ATR = "user-email";
        public static string USER_NAME_ATR = "user-name";
        public static string LOGIN_ATR = "login";

        public static void ResetUserData(IDialogContext context)
        {
            context.PrivateConversationData.SetValue<List<ObjectId>>(WISHLIST_ATR, new List<ObjectId>());
            context.PrivateConversationData.SetValue<List<Filter>>(FILTERS_ATR, new List<Filter>());
            context.PrivateConversationData.SetValue<List<ObjectId>>(COMPARATOR_ATR, new List<ObjectId>());
            context.PrivateConversationData.SetValue<List<FilterCount>>(FILTER_COUNT_ATR, new List<FilterCount>());
            context.PrivateConversationData.SetValue<List<ProductClicks>>(PRODUCT_CLICKS_ATR, new List<ProductClicks>());
            context.PrivateConversationData.SetValue<bool>(LOGIN_ATR, false);
        }

        /*
         * GET
         */

        public static bool IsLoggedIn(IDialogContext context)
        {
            return context.PrivateConversationData.GetValue<bool>(LOGIN_ATR);
        }

        /// <summary>
        /// Gets a state property related to the user. The string value 'property' must be either:
        /// StateHelper.USER_ID_ATR, StateHelper.USER_COUNTRY_ATR, StateHelper.USER_CARD_ID_ATR, 
        /// StateHelper.USER_GENDER_ATR, StateHelper.USER_EMAIL_ATR or StateHelper.USER_NAME_ATR.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static string GetUserProperty(IDialogContext context, string property)
        {
            return context.PrivateConversationData.GetValue<string>(property);
        }

        public static User GetUser(IDialogContext context)
        {
            if (IsLoggedIn(context))
            {
                ObjectId id = context.PrivateConversationData.GetValue<ObjectId>(USER_ID_ATR);
                return UserController.getUser(id);
            }
            else
                return null;
        }

        public static ObjectId GetUserId(IDialogContext context)
        {
            return context.PrivateConversationData.GetValue<ObjectId>(USER_ID_ATR);
        }

        public static void Login(IDialogContext context, User user)
        {
            Context user_context = ContextController.GetContext(user.Id);
            Customer user_crm = CRMController.GetCustomer(user.Id);

            var userdata = context.PrivateConversationData;
            //User login
            userdata.SetValue<bool>(LOGIN_ATR, true);
            //User info
            userdata.SetValue<ObjectId>(USER_ID_ATR, user.Id);
            userdata.SetValue<string>(USER_COUNTRY_ATR, user.Country);
            SetUserState(context,user.Name, user.Email, user.CustomerCard, user.Gender);
            //User context
            userdata.SetValue<List<ObjectId>>(WISHLIST_ATR,user_context.WishList.ToList());
            userdata.SetValue<List<ObjectId>>(COMPARATOR_ATR, user_context.Comparator.ToList());
            //User crm
            userdata.SetValue<List<FilterCount>>(FILTER_COUNT_ATR, user_crm.FiltersCount.ToList());
            userdata.SetValue<List<ProductClicks>>(PRODUCT_CLICKS_ATR, user_crm.ProductsClicks.ToList());
        }

        public static List<ObjectId> GetComparatorItems(IDialogContext context)
        {
            return context.PrivateConversationData.GetValue<List<ObjectId>>(COMPARATOR_ATR);
        }

        public static List<Filter> GetFilters(IDialogContext context)
        {
            return context.PrivateConversationData.GetValue<List<Filter>>(FILTERS_ATR);
        }
        
        public static List<FilterCount> GetFiltersCount(IDialogContext context)
        {
            return context.PrivateConversationData.GetValue<List<FilterCount>>(FILTER_COUNT_ATR);
        }
        
        public static List<ObjectId> GetWishlistItems(IDialogContext context)
        {
            return context.PrivateConversationData.GetValue<List<ObjectId>>(WISHLIST_ATR);
        }
        
        public static List<ProductClicks> GetProductClicks(IDialogContext context)
        {
            return context.PrivateConversationData.GetValue<List<ProductClicks>>(PRODUCT_CLICKS_ATR);
        }

        /*
         * SET
         */

        /// <summary>
        /// Sets a user property. The string value 'property' must be either:
        /// StateHelper.USER_ID_ATR, StateHelper.USER_COUNTRY_ATR, StateHelper.USER_CARD_ID_ATR, 
        /// StateHelper.USER_GENDER_ATR, StateHelper.USER_EMAIL_ATR or StateHelper.USER_NAME_ATR.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public static void SetUserProperty(IDialogContext context, string property, string value)
        {
            context.PrivateConversationData.SetValue<string>(property, value);
        }

        public static void SetUserState(IDialogContext context, string name, string email, string card, string gender)
        {
            SetUserProperty(context, USER_NAME_ATR, name);
            SetUserProperty(context, USER_EMAIL_ATR, email);
            SetUserProperty(context, USER_CARD_ID_ATR, card);
            SetUserProperty(context, USER_GENDER_ATR, gender);
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

        public static void AddItemComparator(IDialogContext context, ObjectId item)
        {
            List<ObjectId> items = GetComparatorItems(context);

            foreach (ObjectId i in items)
                if (i.ToString().Equals(item.ToString()))
                    return;

            items.Add(item);
            context.PrivateConversationData.SetValue<List<ObjectId>>(COMPARATOR_ATR, items);
        }

        public static void RemItemComparator(IDialogContext context, string item)
        {
            List<ObjectId> items = GetComparatorItems(context);

            foreach(ObjectId i in items)
            {
                if (i.ToString().Equals(item))
                {
                    items.Remove(i);
                    break;
                }
            }

            context.PrivateConversationData.SetValue<List<ObjectId>>(COMPARATOR_ATR, items);
        }

        public static void AddItemWishList(IDialogContext context, ObjectId item)
        {
            List<ObjectId> items = GetWishlistItems(context);

            foreach (ObjectId i in items)
                if (i.ToString().Equals(item.ToString()))
                    return;

            items.Add(item);
            context.PrivateConversationData.SetValue<List<ObjectId>>(WISHLIST_ATR, items);
        }

        public static void RemItemWishlist(IDialogContext context, string item)
        {
            List<ObjectId> items = GetWishlistItems(context);

            foreach (ObjectId i in items)
            {
                if (i.ToString().Equals(item))
                {
                    items.Remove(i);
                    break;
                }
            }

            context.PrivateConversationData.SetValue<List<ObjectId>>(WISHLIST_ATR, items);
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