using MongoDB.Bson;
using MongoDB.Driver;
using SmartKioskBot.Helpers;
using SmartKioskBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartKioskBot.Controllers
{
    public abstract class CRMController
    {
        public static string CRM_COLLECTION = "CRM";

        public static Customer GetCustomer(ObjectId customerId)
        {

            var collection = DbSingleton.GetDatabase().GetCollection<Customer>(CRM_COLLECTION);
            var filter = Builders<Customer>.Filter.Eq(c => c.Id, customerId);

            List<Customer> customer = collection.Find(filter).ToList();

            if (customer.Count() == 0)
                return null;
            else
                return customer[0];
        }

        public static void AddCustomer(Customer c)
        {
            var collection = DbSingleton.GetDatabase().GetCollection<Customer>(CRM_COLLECTION);
            collection.InsertOne(c);
        }

        public static void DeleteCustomer(ObjectId customerId)
        {
            var collection = DbSingleton.GetDatabase().GetCollection<Customer>(CRM_COLLECTION);
            var filter = Builders<Customer>.Filter.Eq(c => c.Id, customerId);

            collection.DeleteOne(filter);
        }

        public static void AddPurchase(ObjectId customerId, ObjectId boughtProductId)
        {
            var collection = DbSingleton.GetDatabase().GetCollection<Customer>(CRM_COLLECTION);

            Customer.ProductBought productBought = new Customer.ProductBought
            {
                ProductId = boughtProductId,
                Date = DateTime.Now
            };

            var update = Builders<Customer>.Update.Push(c => c.ProductsBought, productBought);
            var filter = Builders<Customer>.Filter.Eq(c => c.Id, customerId);

            collection.UpdateOne(filter, update);
        }

        public static void AddProductClick(ObjectId customerId, ObjectId clickedProductId)
        {
            var collection = DbSingleton.GetDatabase().GetCollection<Customer>(CRM_COLLECTION);
            var filter = Builders<Customer>.Filter.Eq(c => c.Id, customerId);
            UpdateDefinition<Customer> update;

            Customer customer = GetCustomer(customerId);

            if(customer != null)
            {
                for(int i = 0; i < customer.ProductsClicks.Length; i++) {
                    if (customer.ProductsClicks[i].ProductId.Equals(clickedProductId))
                    {
                        var currentProductClicksElement = customer.ProductsClicks[i];
                        var newNumberOfClicks = currentProductClicksElement.NClicks + 1;

                        update = Builders<Customer>.Update.Set(c => c.ProductsClicks[i].NClicks, newNumberOfClicks);
                        collection.UpdateOne(filter, update);

                        return;
                    }
                }

                // if the product has not been clicked before

                Customer.ProductClicks productClick = new Customer.ProductClicks
                {
                    ProductId = clickedProductId,
                    NClicks = 1
                };

                update = Builders<Customer>.Update.Push(c => c.ProductsClicks, productClick);
                collection.UpdateOne(filter, update);

            }
        }

        public void AddFilterUsage(ObjectId customerId, string filterString)
        {
            var collection = DbSingleton.GetDatabase().GetCollection<Customer>(CRM_COLLECTION);
            var filter = Builders<Customer>.Filter.Eq(c => c.Id, customerId);
            UpdateDefinition<Customer> update;

            Customer customer = GetCustomer(customerId);

            if (customer != null)
            {
                foreach (KeyValuePair<string, int> item in customer.FiltersCount)
                {
                    if (item.Key.Equals(filterString))
                    {
                        int newFiltersCount = item.Value + 1;
                        update = Builders<Customer>.Update.Set(c => c.FiltersCount[item.Key], newFiltersCount);
                        collection.UpdateOne(filter, update);

                        return;
                    }
                }

                KeyValuePair<string, int> kvp = new KeyValuePair<string, int>(filterString, 1);

                update = Builders<Customer>.Update.Push(c => c.FiltersCount, kvp);
                collection.UpdateOne(filter, update);

            }
        }

        public string[] GetMostPopularFilters(ObjectId customerId, int maxNumOfFilters)
        {
            Customer customer = GetCustomer(customerId);

            List<KeyValuePair<string, int>> list = customer.FiltersCount.ToList();
            list.Sort(
                // 'delegate' passes a method as argument to another method. 
                // In this case, I believe it passes the function comparing to pairs to the Sort function.
                delegate (KeyValuePair<string, int> pair1, KeyValuePair<string, int> pair2)
                {
                    return pair1.Value.CompareTo(pair2.Value);
                }
            );

            List<string> filters = new List<string>();

            for(int i = 0; i < maxNumOfFilters; i++)
            {
                filters.Add(list[i].Key);
            }

            return filters.ToArray();
        }

        public ObjectId[] GetMostClickedProducts(ObjectId customerId, int maxNumOfProducts)
        {
            Customer customer = GetCustomer(customerId);

            List<Customer.ProductClicks> list = customer.ProductsClicks.ToList();

            list.Sort(
                // 'delegate' passes a method as argument to another method. 
                // In this case, I believe it passes the function comparing to pairs to the Sort function.
                delegate (Customer.ProductClicks l1, Customer.ProductClicks l2)
                {
                    return l1.NClicks.CompareTo(l2.NClicks);
                }
            );

            List<ObjectId> filters = new List<ObjectId>();

            for (int i = 0; i < maxNumOfProducts; i++)
            {
                filters.Add(list[i].ProductId);
            }

            return filters.ToArray();
        }
    }
}