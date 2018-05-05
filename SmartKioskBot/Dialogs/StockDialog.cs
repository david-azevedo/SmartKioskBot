using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using MongoDB.Bson;
using SmartKioskBot.Controllers;
using SmartKioskBot.Models;
using SmartKioskBot.Helpers;
using SmartKioskBot.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MongoDB.Driver;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class StockDialog
    {

        public static SortedDictionary<string, string> CheckAvailability(string productId)
        {
            var storeCollection = DbSingleton.GetDatabase().GetCollection<Store>(AppSettings.StoreCollection);
            var filter = Builders<Store>.Filter.Empty;

            List<Store> stores = storeCollection.Find(filter).ToList();
            SortedDictionary<string, string> storeNames = new SortedDictionary<string, string>();

            for (int i = 0; i < stores.Count(); i++)
            {
                for (int j = 0; j < stores[i].ProductsInStock.Count(); j++)
                {
                    if (stores[i].ProductsInStock[j].ProductId.ToString().Equals(productId))
                    {
                        if (stores[i].ProductsInStock[j].Stock > 0)
                        {
                            storeNames.Add(stores[i].Id.ToString(), stores[i].Name);
                            break;
                        }
                    }
                }
            }

            return storeNames;
        }
    }
}