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



        public static Customer GetCustomer(ObjectId userId)
        {

            var collection = DbSingleton.GetDatabase().GetCollection<Customer>(CRM_COLLECTION);
            var filter = Builders<Customer>.Filter.Eq(c => c.UserId, userId);

            List<Customer> customer = collection.Find(filter).ToList();

            if (customer.Count() == 0)
                return null;
            else
                return customer[0];
        }

        public static ObjectId AddCustomer(User user)
        {
            Customer c = new Customer
            {
                Country = user.Country,
                UserId = user.Id,
                FiltersCount = new Customer.FilterCount[] { },
                ProductsBought = new Customer.ProductBought[] { },
                ProductsClicks = new Customer.ProductClicks[] { }
            };

            if(c.Country == null)
            {
                throw new Exception("A customer must contain a non-null country field, because 'country' is a shard key in Mongo.");
            }

            var collection = DbSingleton.GetDatabase().GetCollection<Customer>(CRM_COLLECTION);
            collection.InsertOne(c);

            return c.Id;
        }

        public static void DeleteCustomer(ObjectId userId)
        {
            var collection = DbSingleton.GetDatabase().GetCollection<Customer>(CRM_COLLECTION);
            var filter = Builders<Customer>.Filter.Eq(c => c.UserId, userId);

            collection.DeleteOne(filter);
        }

        public static void AddPurchase(ObjectId userId, string country, ObjectId boughtProductId)
        {
            var collection = DbSingleton.GetDatabase().GetCollection<Customer>(CRM_COLLECTION);

            Customer.ProductBought productBought = new Customer.ProductBought
            {
                ProductId = boughtProductId,
                Date = DateTime.Now
            };

            var update = Builders<Customer>.Update.Push(c => c.ProductsBought, productBought);
            var filter = Builders<Customer>.Filter.And(
               Builders<Customer>.Filter.Eq(c => c.UserId, userId),
               Builders<Customer>.Filter.Eq(c => c.Country, country));
            
            collection.UpdateOne(filter, update);
        }

        public static void AddProductClick(ObjectId userId, string country, ObjectId clickedProductId)
        {
            var collection = DbSingleton.GetDatabase().GetCollection<Customer>(CRM_COLLECTION);
            var filter = Builders<Customer>.Filter.And(
               Builders<Customer>.Filter.Eq(c => c.UserId, userId),
               Builders<Customer>.Filter.Eq(c => c.Country, country));
            UpdateDefinition<Customer> update;

            Customer customer = GetCustomer(userId);

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
            else
            {
                throw new Exception("Customer not found.");
            }
        }

        public static void AddFilterUsage(ObjectId customerId, string country, string filterString)
        {
            var collection = DbSingleton.GetDatabase().GetCollection<Customer>(CRM_COLLECTION);
            var filter = Builders<Customer>.Filter.And(
               Builders<Customer>.Filter.Eq(c => c.UserId, customerId),
               Builders<Customer>.Filter.Eq(c => c.Country, country));
            UpdateDefinition<Customer> update;

            Customer customer = GetCustomer(customerId);

            if (customer != null)
            {
                for (int i = 0; i < customer.FiltersCount.Length; i++)
                {
                    if (customer.FiltersCount[i].Filter.Equals(filterString))
                    {
                        var currentFilterSearches = customer.FiltersCount[i];
                        var newNumberOfSearches = currentFilterSearches.NSearches + 1;

                        update = Builders<Customer>.Update.Set(c => c.FiltersCount[i].NSearches, newNumberOfSearches);
                        collection.UpdateOne(filter, update);

                        return;
                    }
                }

                // if the product has not been clicked before

                Customer.FilterCount filterCount = new Customer.FilterCount
                {
                    Filter = filterString,
                    NSearches = 1
                };

                update = Builders<Customer>.Update.Push(c => c.FiltersCount, filterCount);
                collection.UpdateOne(filter, update);

            }
            else
            {
                throw new Exception("Customer not found.");
            }
        }

        public static string[] GetMostPopularFilters(ObjectId customerId, int maxNumOfFilters)
        {
            Customer customer = GetCustomer(customerId);

            List<Customer.FilterCount> list = customer.FiltersCount.ToList();
            list.Sort(
                // 'delegate' passes a method as argument to another method. 
                // In this case, I believe it passes the function comparing to pairs to the Sort function.
                delegate (Customer.FilterCount l1, Customer.FilterCount l2)
                {
                    return l1.NSearches.CompareTo(l2.NSearches);
                }
            );

            List<string> filters = new List<string>();

            for (int i = 0; i < maxNumOfFilters; i++)
            {
                filters.Add(list[i].Filter);
            }

            return filters.ToArray();
        }

        public static ObjectId[] GetMostClickedProducts(ObjectId customerId, int maxNumOfProducts)
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

        public static void AddRandomCustomer()
        {

        }

        public static void Test()
        {
            ObjectId id = new ObjectId("111111111111111111111111");

            User u = new User()
            {
                Id = id,
                Country = "Portugal",
                Name = "José"
            };

            ObjectId insertedCustomer = AddCustomer(u);

            Customer c1 = GetCustomer(id);

            AddPurchase(id, c1.Country, new ObjectId("111111111111111111111111"));
            AddPurchase(id, c1.Country, new ObjectId("222222222222222222222222"));

            AddFilterUsage(id, c1.Country, "asus");
            AddFilterUsage(id, c1.Country, "lenovo");
            AddFilterUsage(id, c1.Country, "asus");

            AddProductClick(id, c1.Country, new ObjectId("111111111111111111111111"));

            c1 = GetCustomer(id);

            DeleteCustomer(id);

        }
    }
}