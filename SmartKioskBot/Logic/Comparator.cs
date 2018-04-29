﻿using MongoDB.Bson;
using MongoDB.Driver;
using SmartKioskBot.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Security.Authentication;
using SmartKioskBot.Helpers;
using Microsoft.Bot.Builder.Dialogs;

namespace SmartKioskBot.Logic
{
    public static class Comparator
    {
        public enum Parts { CPU, GPU, RAM, Screen };

        public static Dictionary<Parts, List<int>> GetBestProduct(List<Product> products)
        {
            // The key is the index of the product in the list argument. The value is the number of times a component of the product is 
            // better than the others.
            Dictionary<Parts, List<int>> partToSortedBestProducts = new Dictionary<Parts, List<int>>();

            List<int> bestSortedCPUIndex = GetSortedBestCPUs(products);
            partToSortedBestProducts.Add(Parts.CPU, bestSortedCPUIndex);

            List<int> bestSortedGPUIndex = GetSortedBestGPUs(products);
            partToSortedBestProducts.Add(Parts.GPU, bestSortedGPUIndex);

            List<int> bestSortedRAMIndex = GetSortedBestRAMs(products);
            partToSortedBestProducts.Add(Parts.RAM, bestSortedRAMIndex);

            List<int> bestSortedScreenIndex = GetSortedBestScreens(products);
            partToSortedBestProducts.Add(Parts.Screen, bestSortedScreenIndex);

            return partToSortedBestProducts;
        }

        public static List<int> GetSortedBestCPUs(List<Product> products)
        {
            List<Part.CPU> cpus = new List<Part.CPU>();

            Regex dualRegex = new Regex(@"dual", RegexOptions.IgnoreCase);
            Regex quadRegex = new Regex(@"quad", RegexOptions.IgnoreCase);

            for (int i = 0; i < products.Count; i++)
            {
                Product currentProduct = products[i];
                int numOfCores = 0;

                if (currentProduct.CoreNr != null)
                {
                    if (dualRegex.Match(currentProduct.CoreNr).Length != 0) // match dual core (2 cores)
                    {
                        numOfCores = 2;
                    }

                    if (quadRegex.Match(currentProduct.CoreNr).Length != 0) // match quad core (4 cores)
                    {
                        numOfCores = 4;
                    }
                }

                float cpuSpeed = 0;

               
                cpuSpeed = (float) currentProduct.CPUSpeed;
                

                string name = null;

                if (currentProduct.CPU != null)
                {
                    name = currentProduct.CPU;
                }

                cpus.Add(new Part.CPU(name, numOfCores, cpuSpeed));
            }

            return GetSortedParts(cpus);
        }

        public static List<int> GetSortedBestGPUs(List<Product> products)
        {
            List<Part.GPU> gpus = new List<Part.GPU>();

            for (int i = 0; i < products.Count; i++)
            {
                Product currentProduct = products[i];

                bool exists = false;
                string name = null;
                int vRAM = 0;

                if (currentProduct.GraphicsCard != null)
                {
                    name = currentProduct.GraphicsCard;
                    exists = true;
                }

                gpus.Add(new Part.GPU(exists, name, vRAM));
            }

            return GetSortedParts(gpus);
        }

        public static List<int> GetSortedBestRAMs(List<Product> products)
        {
            List<Part.RAM> rams = new List<Part.RAM>();

            for (int i = 0; i < products.Count; i++)
            {
                Product currentProduct = products[i];
                int memory = 0;

                memory = (int) currentProduct.RAM;

                rams.Add(new Part.RAM(memory));
            }

            return GetSortedParts(rams);
        }

        public static List<int> GetSortedBestScreens(List<Product> products)
        {
            List<Part.Screen> screens = new List<Part.Screen>();

            Regex numberRegex = new Regex(@"\d+");
            Regex negRegex = new Regex(@"não", RegexOptions.IgnoreCase);
            Regex posRegex = new Regex(@"sim", RegexOptions.IgnoreCase);

            for (int i = 0; i < products.Count; i++)
            {
                Product currentProduct = products[i];
                int resolution_x = 0, resolution_y = 0;
                float diagonal = 0;
                bool touch = false;

                diagonal = (float) currentProduct.ScreenDiagonal;

                if (currentProduct.ScreenResolution != null)
                {
                    Match resMatch = numberRegex.Match(currentProduct.ScreenResolution);
                    resolution_x = int.Parse(resMatch.Value);
                    resolution_y = int.Parse(resMatch.NextMatch().Value);
                }

                if (currentProduct.TouchScreen != null)
                {
                    if (negRegex.Match(currentProduct.TouchScreen).Length != 0)
                    {
                        touch = false;
                    }
                    else if (posRegex.Match(currentProduct.TouchScreen).Length != 0)
                    {
                        touch = true;
                    }
                }

                screens.Add(new Part.Screen(diagonal, resolution_x, resolution_y, touch));
            }

            return GetSortedParts(screens);
        }

        public static List<int> GetSortedParts<T>(List<T> comparables) where T : Part
        {
            Dictionary<T, int> partToIndexOfProduct = new Dictionary<T, int>();

            for(int i = 0; i < comparables.Count; i++)
            {
                partToIndexOfProduct.Add(comparables[i], i);
            }

            List<T> sortedComparables = new List<T>(comparables);

            sortedComparables.Sort();

            List<int> orderedIndexes = new List<int>();

            for(int i = 0; i < sortedComparables.Count; i++)
            {
                // add index of the product to which the part belongs
                orderedIndexes.Add(partToIndexOfProduct[sortedComparables[i]]); 
            }

            return orderedIndexes;
        }

        public static void Test(IDialogContext context)
        {
            try
            {
                var productCollection = DbSingleton.GetDatabase().GetCollection<Product>(AppSettings.ProductsCollection);

                List<Product> products = productCollection.Find(new BsonDocument()).ToList();

                var collection = DbSingleton.GetDatabase().GetCollection<Product>(AppSettings.CollectionName);
                var builder = Builders<Product>.Filter;

                var filter1 = builder.Eq("_id", ObjectId.Parse("5ad6628086e5482fb04ea97b"));
                var filter2 = builder.Eq("_id", ObjectId.Parse("5ad6628186e5482fb04ea97e"));

                var product1 = collection.Find(filter1).FirstOrDefault();
                var product2 = collection.Find(filter2).FirstOrDefault();

                Dictionary<Comparator.Parts, List<int>> comparisonResults = GetBestProduct(new List<Product>() { product1, product2 });

                var reply = context.MakeMessage();
                reply.Text = "Comparison results: \n";

                foreach (KeyValuePair<Comparator.Parts, List<int>> entry in comparisonResults)
                {
                    reply.Text += String.Format("Best {0}:\n\n", entry.Key);

                    entry.Value.ForEach(item => reply.Text += String.Format("\t-{0}\n\n", item));
                }

                context.PostAsync(reply);
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Debug.WriteLine(string.Format("Caught exception! Error ocurred! Message: {0}", e.Message));
                Debug.WriteLine(string.Format("Stack trace: {0}", e.ToString()));
            }

           
            /*Product product1 = new Product();
            product1.CPU = "Intel Core i7-6500U";
            product1.CoreNr = "Dual Core";
            product1.CPUSpeed = 2.7;
            product1.RAM = 8;

            Product product2 = new Product();
            product2.CPU = "Intel Core i7-7820HQ";
            product2.CoreNr = "Quad Core";
            product2.CPUSpeed = 2.7;
            product2.RAM = 4;*/

           
        }
    }
}