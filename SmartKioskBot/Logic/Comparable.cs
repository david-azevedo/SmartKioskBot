using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartKioskBot.Logic
{
    public abstract class Comparable
    {
        private string compareExceptionMessage = "Product to compare is not of the same class.";

        /// <summary>
        /// This method is used to compare the current computer part with another part. It compares attributes of the parts directly, so it isn't as truthful as it should be.
        /// </summary>
        /// <param name="c">The part to compare to.</param>
        /// <returns>Returns an integer with the value of the comparison. Less than 0 means the current part is worst. More than 0 means it's better. Equal to 0 means it's a draw.</returns>
        public abstract int CompareTo(Comparable c);

        public class CPU : Comparable
        {
            private int numberOfCores;
            private float clock; // in GHz

            public CPU (int numberOfCores, float clock)
            {
                this.numberOfCores = numberOfCores;
                this.clock = clock;
            }

            public override int CompareTo(Comparable c)
            {
                if (!(c is CPU))
                {
                    throw new Exception(this.compareExceptionMessage);
                }

                CPU cpu = (CPU)c;
                int result = 0;

                if(this.numberOfCores > cpu.numberOfCores)
                {
                    result++;
                }
                else
                {
                    result--;
                }

                if(this.clock > cpu.clock)
                {
                    result++;
                }
                else
                {
                    result--;
                }

                return result;
                
            }
        }
    }
}
