using MongoDB.Bson;
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
using Microsoft.Bot.Connector;
using SmartKioskBot.UI;
using SmartKioskBot.Dialogs;

namespace SmartKioskBot.Logic
{
    public static class Comparator
    {
        public enum Parts { CPU, GPU, RAM, Screen, Price };

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

            List<int> bestSortedPriceIndex = GetSortedBestPrices(products);
            partToSortedBestProducts.Add(Parts.Price, bestSortedPriceIndex);

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


                cpuSpeed = (float)currentProduct.CPUSpeed;


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

                memory = (int)currentProduct.RAM;

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

                diagonal = (float)currentProduct.ScreenDiagonal;

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

        private static List<int> GetSortedBestPrices(List<Product> products)
        {
            List<Part.Price> prices = new List<Part.Price>();

            for (int i = 0; i < products.Count; i++)
            {
                Product currentProduct = products[i];
                int price = 0;

                price = (int)currentProduct.Price;              
                prices.Add(new Part.Price(price));
            }

            return GetSortedParts(prices);
        }
        public static List<int> GetSortedParts<T>(List<T> comparables) where T : Part
        {
            Dictionary<T, int> partToIndexOfProduct = new Dictionary<T, int>();

            for (int i = 0; i < comparables.Count; i++)
            {
                partToIndexOfProduct.Add(comparables[i], i);
            }

            List<T> sortedComparables = new List<T>(comparables);

            sortedComparables.Sort();

            List<int> orderedIndexes = new List<int>();

            for (int i = 0; i < sortedComparables.Count; i++)
            {
                // add index of the product to which the part belongs
                orderedIndexes.Add(partToIndexOfProduct[sortedComparables[i]]);
            }

            return orderedIndexes;
        }

        public static void ShowProductComparison(IDialogContext context, List<Product> productsToCompare)
        {
            Dictionary<Comparator.Parts, List<int>> comparisonResults = GetBestProduct(productsToCompare); //TODO VERIFICAR RESULTS


            //size of products to show on result(top 3 if >3)
           /* var resultSize = 0;
            if (productsToCompare.Count > 3)
                resultSize = 3;
            else */var resultSize = productsToCompare.Count;

            var reply = context.MakeMessage();

            //Sends a reply for each specification compared and shows the products(best ones first)
            foreach (KeyValuePair<Comparator.Parts, List<int>> entry in comparisonResults)
            {
                reply = context.MakeMessage();
                reply.Text = "### " + BotDefaultAnswers.getComparator(entry.Key.ToString()) + "  \n";

                for (int i = 0; i < resultSize && i <7; i++)
                {
                    reply.Text += i+1 + ". "  + productsToCompare[entry.Value[i]].Brand + " "  + productsToCompare[entry.Value[i]].Model + ";  \n";
                }
                //Send individual classification
                context.PostAsync(reply);
            }
            
            

            List<double> overallBest = new List<double>(new double[productsToCompare.Count]);
            var bestInPart = new Product();
            var product = new Product();
            //calculate score for each product
            foreach (KeyValuePair<Comparator.Parts, List<int>> entry in comparisonResults)
            {
                for (int i = 0; i < productsToCompare.Count; i++)
                {
                    //RAM
                    if (entry.Key == Comparator.Parts.RAM)
                    {
                        product = productsToCompare[entry.Value[i]];
                        if (i == 0)
                        {
                            bestInPart = productsToCompare[entry.Value[i]];
                        }

                        overallBest[entry.Value[i]] += (double)(product.RAM * 0.15 / bestInPart.RAM);
                    }
                    //PRICE
                    else if (entry.Key == Comparator.Parts.Price)
                    {
                        product = productsToCompare[entry.Value[i]];
                        if (i == 0)
                        {
                            bestInPart = productsToCompare[entry.Value[i]];
                        }
                            overallBest[entry.Value[i]] += (double)(1-(Math.Abs(bestInPart.Price - product.Price) * 0.3 / bestInPart.Price));
                    }
                    //GPU
                    else if (entry.Key == Comparator.Parts.GPU)
                    {
                        overallBest[entry.Value[i]] += (double)((productsToCompare.Count - i) * 0.2);
                    }
                    //CPU
                    else if (entry.Key == Comparator.Parts.CPU)
                    {
                        overallBest[entry.Value[i]] += (double)((productsToCompare.Count - i) * 0.25);
                    }
                    //SCREEN
                    else if (entry.Key == Comparator.Parts.Screen)
                    {
                        overallBest[entry.Value[i]] += (double)((productsToCompare.Count - i) * 0.1);
                    }
                }
            }

            List<Attachment> cards = new List<Attachment>();
            Dictionary<int, double> result = new Dictionary<int, double>();

            for(int i = 0; i < overallBest.Count; i++)
            {
                result.Add(i, overallBest[i]);
            }

            //sort by descending score
            IEnumerable<KeyValuePair< int, double>> sortedResult = result.OrderByDescending(i => i.Key);

            //add products to cards
            foreach (KeyValuePair<int, double> kvp in sortedResult)
            {
                cards.Add(ProductCard.GetProductCard(productsToCompare[kvp.Key], ProductCard.CardType.SEARCH).ToAttachment());
            }

            context.PostAsync("## Resultados finais:");

            reply = context.MakeMessage();
            reply.Attachments = cards;
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            context.PostAsync(reply);
            cards.Clear();
        }
    }
}