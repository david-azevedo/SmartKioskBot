using MongoDB.Bson;
using MongoDB.Driver;
using SmartKioskBot.Helpers;
using SmartKioskBot.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace SmartKioskBot.Logic
{
    public static class Comparator
    {
        public static Product GetBestProduct (List<Product> products)
        {
            int bestCPUIndex = GetBestCPU(products);

            return products[bestCPUIndex];
        }

        public static int GetBestCPU (List<Product> products)
        {
            List<Comparable.CPU> cpus = new List<Comparable.CPU>();

            Regex dualRegex = new Regex(@"dual", RegexOptions.IgnoreCase);
            Regex quadRegex = new Regex(@"quad", RegexOptions.IgnoreCase);

            for (int i = 0; i < products.Count; i++)
            {
                Product currentProduct = products[i];
                int numOfCores = 0;

                if (dualRegex.Match(currentProduct.CoreNr).Length != 0) // match dual core (2 cores)
                {
                    numOfCores = 2;
                }

                if (quadRegex.Match(currentProduct.CoreNr).Length != 0) // match quad core (4 cores)
                {
                    numOfCores = 4;
                }

                cpus.Add(new Comparable.CPU(numOfCores, float.Parse(currentProduct.CPUSpeed)));
            }

            return GetBestPart(cpus);
        }

        public static int GetBestPart<T> (List<T> comparables) where T : Comparable
        {
            int indexOfBestComparable = -1;

            for(int i = 0; i < comparables.Count - 1; i++)
            {
                int comparisonResult = comparables[i].CompareTo(comparables[i + 1]);

                if (comparisonResult > 0)
                {
                    indexOfBestComparable = i;
                }
                else if (comparisonResult < 0)
                {
                    indexOfBestComparable = i + 1;
                }
            }

            return indexOfBestComparable;
        }

        public static void Test()
        {
            var collection = DbSingleton.GetDatabase().GetCollection<Product>(AppSettings.CollectionName);
            var builder = Builders<Product>.Filter;

            var filter1 = builder.Eq("_id", ObjectId.Parse("5ad6628086e5482fb04ea97b"));
            var filter2 = builder.Eq("_id", ObjectId.Parse("5ad6628086e5482fb04ea97b"));

            var product1 = collection.Find(filter1).FirstOrDefault();
            var product2 = collection.Find(filter2).FirstOrDefault();

            var winner = GetBestProduct(new List<Product>() { product1, product2 });
        }
    }
}