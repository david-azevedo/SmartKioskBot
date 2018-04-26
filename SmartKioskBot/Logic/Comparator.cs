using SmartKioskBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartKioskBot.Logic
{
    public class Comparator
    {
        public Product GetBestProduct (List<Product> products)
        {
            int bestCPUIndex = GetBestCPU(products);

            return products[bestCPUIndex];
        }

        public int GetBestCPU (List<Product> products)
        {
            List<Comparable.CPU> cpus = new List<Comparable.CPU>();

            for(int i = 0; i < products.Count; i++)
            {
                Product currentProduct = products[i];
                cpus.Add(new Comparable.CPU(int.Parse(currentProduct.CoreNr), float.Parse(currentProduct.CPUSpeed)));
            }

            return GetBestPart(cpus);
        }

        public int GetBestPart<T> (List<T> comparables) where T : Comparable
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
    }
}