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
using AdaptiveCards;

namespace SmartKioskBot.Logic
{
    public static class ComparatorLogic
    {
        public enum Parts { CPU, GPU, RAM, Screen, Price };
        public static int MAX_PRODUCTS_ON_COMPARATOR = 6;

        private static Dictionary<Parts, List<int>> GetBestProduct(List<Product> products)
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

        private static List<int> GetSortedBestCPUs(List<Product> products)
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

        private static List<int> GetSortedBestGPUs(List<Product> products)
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

        private static List<int> GetSortedBestRAMs(List<Product> products)
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

        private static List<int> GetSortedBestScreens(List<Product> products)
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

        private static List<int> GetSortedParts<T>(List<T> comparables) where T : Part
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
            /* This is a dictionary that for each part points to a ordered List of indexes. The first element of the list is the index of the
             * BEST product for that part.
             */
            Dictionary<ComparatorLogic.Parts, List<int>> comparisonResults = GetBestProduct(productsToCompare); //TODO VERIFICAR RESULTS


            //size of products to show on result(top 3 if >3)
            /* var resultSize = 0;
             if (productsToCompare.Count > 3)
                 resultSize = 3;
             else */

            var resultSize = productsToCompare.Count;

            var reply = context.MakeMessage();

            AdaptiveCard scoreTableCard = new AdaptiveCard()
            {
                Version = "1.0",
                Body = { },
                Actions = { }
            };

            List<AdaptiveColumn> scoreColumns = new List<AdaptiveColumn>();
            scoreColumns.Add(new AdaptiveColumn()
            {
                Items = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock()
                    {
                        Text = "Product",
                        Weight = AdaptiveTextWeight.Bolder
                    }
                }
            }
            );

            List<double> overallBest = new List<double>(new double[productsToCompare.Count]);
            var bestInPart = new Product();
            var product = new Product();

            //calculate score for each product
            foreach (KeyValuePair<ComparatorLogic.Parts, List<int>> entry in comparisonResults)
            {
                for (int i = 0; i < productsToCompare.Count; i++)
                {
                    //RAM
                    if (entry.Key == ComparatorLogic.Parts.RAM)
                    {
                        product = productsToCompare[entry.Value[i]];
                        if (i == 0)
                        {
                            bestInPart = productsToCompare[entry.Value[i]];
                        }

                        overallBest[entry.Value[i]] += (double)(product.RAM * 0.15 / bestInPart.RAM);
                    }
                    //PRICE
                    else if (entry.Key == ComparatorLogic.Parts.Price)
                    {
                        product = productsToCompare[entry.Value[i]];
                        if (i == 0)
                        {
                            bestInPart = productsToCompare[entry.Value[i]];
                        }
                        overallBest[entry.Value[i]] += (double)(1 - (Math.Abs(bestInPart.Price - product.Price) * 0.3 / bestInPart.Price));
                    }
                    //GPU
                    else if (entry.Key == ComparatorLogic.Parts.GPU)
                    {
                        overallBest[entry.Value[i]] += (double)((productsToCompare.Count - i) * 0.2);
                    }
                    //CPU
                    else if (entry.Key == ComparatorLogic.Parts.CPU)
                    {
                        overallBest[entry.Value[i]] += (double)((productsToCompare.Count - i) * 0.25);
                    }
                    //SCREEN
                    else if (entry.Key == ComparatorLogic.Parts.Screen)
                    {
                        overallBest[entry.Value[i]] += (double)((productsToCompare.Count - i) * 0.1);
                    }
                }
            }

            Dictionary<int, double> result = new Dictionary<int, double>();

            for (int i = 0; i < overallBest.Count; i++)
            {
                result.Add(i, overallBest[i]);
            }

            //sort by descending score
            IEnumerable<KeyValuePair<int, double>> sortedResult = result.OrderByDescending(i => i.Value);

            /* This for loop creates the table to present the results.
             * First, it adds a row to the first column for each product that is being examined.
             * Then, it creates "n" new columns, where "n" is the number of parts being compared.
             * The first column is ordered by best overall products. That is, the first row should have the best overall product and its 
             * scores for each of the parts. The last row should have the worst overall.
             */

            int position = 1;

            // Creates a row for each product
            foreach (KeyValuePair<int, double> entry in sortedResult)
            {
                int currentProductIndex = entry.Key;
                Product currentProduct = productsToCompare[currentProductIndex];

                scoreColumns[0].Items.Add(
                    new AdaptiveTextBlock()
                    {
                        Text = position + ". " + currentProduct.Brand + " " + currentProduct.Model,
                        Separator = true
                    }
                    );

                position++;
            }

            // Creates a column for each part
            foreach (KeyValuePair<ComparatorLogic.Parts, List<int>> entry in comparisonResults)
            {
                Parts part = entry.Key;

                List<AdaptiveElement> partColumnCells = new List<AdaptiveElement>()
                {
                    new AdaptiveTextBlock()
                    {
                        Text = part.ToString(),
                        Weight = AdaptiveTextWeight.Bolder
                    }
                };

                for (int i = 0; i < sortedResult.Count(); i++)
                {
                    KeyValuePair<int, double> currentIndexToScore = sortedResult.ElementAt(i);
                    int currentProductIndex = currentIndexToScore.Key;

                    int currentProductRankingInPart = comparisonResults[part][currentProductIndex] + 1;

                    partColumnCells.Add(new AdaptiveTextBlock()
                    {
                        Text = currentProductRankingInPart.ToString(),
                        HorizontalAlignment = AdaptiveHorizontalAlignment.Center
                    });
                }

                scoreColumns.Add(new AdaptiveColumn()
                {
                    Width = AdaptiveColumnWidth.Stretch,
                    Items = partColumnCells
                });
            }

            scoreTableCard.Body.Add(new AdaptiveColumnSet()
            {
                Columns = scoreColumns
            });

            // Create the attachment.
            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = scoreTableCard
            };

            reply = context.MakeMessage();
            reply.Attachments.Add(attachment);

            //Send table
            context.PostAsync(reply);

            //add products to cards
            List<Attachment> cards = new List<Attachment>();

            foreach (KeyValuePair<int, double> kvp in sortedResult)
            {
                cards.Add(ProductCard.GetProductCard(productsToCompare[kvp.Key], ProductCard.CardType.COMPARATOR).ToAttachment());
            }

            reply = context.MakeMessage();
            reply.Attachments = cards;
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            context.PostAsync(reply);
            cards.Clear();
        }
    }
}