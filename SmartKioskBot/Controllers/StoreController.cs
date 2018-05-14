using MongoDB.Driver;
using SmartKioskBot.Helpers;
using SmartKioskBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartKioskBot.Controllers
{
    public abstract class StoreController
    {
        public static List<Store> getClosesStores(Double[] coords)
        {
            var storeCollection = DbSingleton.GetDatabase().GetCollection<Store>(AppSettings.StoreCollection);
            var filter = Builders<Store>.Filter.Empty;
            //var s = Builders<Store>.Sort.Ascending(o => (o.Coordinates[0] -1000));
            // var sort = Builders<Store>.Sort.Ascending(o => o.Coordinates);// calcDist(o.Coordinates,coords));

            var stores = storeCollection.Find(filter).ToList();

            foreach (Store s in stores)
                s.calculateProximity(coords);

            var sorted = stores.OrderBy(o => o.proximity).ToList();

           /* var s = stores.Sort(sort);
            var a = s.Count();
            var y =s.ToList();*/

            if (sorted.Count() == 0)
                return new List<Store>();
            else
                return sorted;
        }
    }
}