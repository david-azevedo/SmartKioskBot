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
    }
}