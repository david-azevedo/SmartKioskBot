using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartKioskBot.Logic
{
    public abstract class Part : IComparable<Part>
    {
        private string compareExceptionMessage = "Product to compare is not of the same type.";

        /// <summary>
        /// This method is used to compare the current computer part with another part. It compares attributes of the parts directly, so it isn't as truthful as it should be.
        /// </summary>
        /// <param name="c">The part to compare to.</param>
        /// <returns>Returns an integer with the value of the comparison. Less than 0 means the current part is worst. More than 0 means it's better. Equal to 0 means it's a draw.</returns>
        public abstract int CompareTo(Part c);

        public static List<string> GetRanking(string file)
        {
            List<string> ranking = new List<string>();

            string rankingsDir = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory.ToString(), "Logic\\rankings");

            using (var reader = new StreamReader(rankingsDir + Path.DirectorySeparatorChar + file))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    // values[0] is the ranking position (not needed because List is ordered)
                    // values[1] is the name of the CPU 
                    ranking.Add(values[1]);
                }
            }

            return ranking;
        }

        public class CPU : Part
        {
            public static List<string> cpuRanking;
            public static string cpuRankingFile = "cpu.csv";

            public static void GetCPURanking()
            {
                cpuRanking = GetRanking(cpuRankingFile);
            }

            public static string GetChipName(string cpuName)
            {
                string[] words = cpuName.Split(' ');

                return words[words.Length - 1]; // assumes the chip name is the last substring, because it generally is, but not always
            }

            public static int FindCPURanking(string chipName)
            {
                int ranking = -1;

                for(int i = 0; i < cpuRanking.Count; i++)
                {
                    if (cpuRanking[i].Contains(chipName))
                    {
                        ranking = i;
                        break;
                    }
                }

                return ranking;
            }

            private string name = null;
            private int numberOfCores = 0;
            private float clock = 0; // in GHz
            

            public CPU(string name, int numberOfCores, float clock)
            {
                this.name = name;
                this.numberOfCores = numberOfCores;
                this.clock = clock;
            }

            public CPU (int numberOfCores, float clock) : this(null, numberOfCores, clock) { }
           
            public override int CompareTo(Part c)
            {
                if (!(c is CPU))
                {
                    throw new Exception(this.compareExceptionMessage);
                }

                CPU cpu = (CPU)c;
                int result = 0;

                // if both CPUs have names and they are different, search in ranking
                if ((this.name != null && cpu.name != null) && (!this.name.Equals(cpu.name)))
                {
                    // if ranking is null, generate it
                    if(cpuRanking == null)
                    {
                        GetCPURanking();
                    }

                    int currrentCPURanking = FindCPURanking(GetChipName(this.name));
                    int otherCPURanking = FindCPURanking(GetChipName(cpu.name));
                  
                    // only compare if we have both rankings
                    if(!(currrentCPURanking < 0 || otherCPURanking < 0))
                    {
                        // current is better
                        if(currrentCPURanking < otherCPURanking)
                        {
                            return 1;
                        }
                        // other is better
                        else if (currrentCPURanking > otherCPURanking)
                        {
                            return -1;
                        }
                        // current == other
                        else
                        {
                            return 0;
                        }
                    }
                }

                if(this.numberOfCores > cpu.numberOfCores)
                {
                    result++;
                }
                else if (this.numberOfCores < cpu.numberOfCores)
                {
                    result--;
                }

                if(this.clock > cpu.clock)
                {
                    result++;
                }
                else if (this.clock < cpu.clock)
                {
                    result--;
                }

                return result;
                
            }
        }

        public class GPU : Part
        {
            public static List<string> gpuRanking;
            public static string gpuRankingFile = "gpu.csv";

            public static void GetGPURanking()
            {
                gpuRanking = GetRanking(gpuRankingFile);
            }

            public static int FindGPURanking(string gpuName)
            {
                int ranking = -1;

                for (int i = 0; i < gpuRanking.Count; i++)
                {
                    string[] keywords = gpuName.Split(' ');

                    var invariantText = gpuName.ToUpperInvariant();
                    bool matches = keywords.All(kw => invariantText.Contains(kw.ToUpperInvariant())); // true if all keywords are present in ranking

                    if (matches)
                    {
                        ranking = i;
                        break;
                    }
                }

                return ranking;
            }

            private bool exists = true; // if there is a DEDICATED GPU
            private string name = null;
            private float vRAM = 0; // in GB

            public GPU (bool exists, string name, float vRAM)
            {
                this.exists = exists;
                this.name = name;
                this.vRAM = vRAM;
            }

            public override int CompareTo(Part c)
            {
                if (!(c is GPU))
                {
                    throw new Exception(this.compareExceptionMessage);
                }

                GPU gpu = (GPU)c;
                int result = 0;

                // if both GPUs have names, search in ranking
                if (this.name != null && gpu.name != null)
                {
                    // if ranking is null, generate it
                    if (gpuRanking == null)
                    {
                        GetGPURanking();
                    }

                    int currrentGPURanking = FindGPURanking(this.name);
                    int otherGPURanking = FindGPURanking(gpu.name);

                    // only compare if we have both rankings
                    if (!(currrentGPURanking < 0 || otherGPURanking < 0))
                    {
                        // current is better
                        if (currrentGPURanking < otherGPURanking)
                        {
                            return 1;
                        }
                        // other is better
                        else if (currrentGPURanking > otherGPURanking)
                        {
                            return -1;
                        }
                        // current == other
                        else
                        {
                            return 0;
                        }
                    }
                }

                if (this.exists && !(gpu.exists))
                {
                    result++;
                    return result;
                }
                else if(!(this.exists) && gpu.exists)
                {
                    result--;
                    return result;
                }
                else if(!(this.exists || gpu.exists))
                {
                    return result;
                }

                if (this.vRAM > gpu.vRAM)
                {
                    result++;
                }
                else if (this.vRAM < gpu.vRAM)
                {
                    result--;
                }

                return result;
            }
        }

        public class RAM : Part
        {
            private int memory = 0; // in GB

            public RAM(int memory)
            {
                this.memory = memory;
            }

            public override int CompareTo(Part c)
            {
                if (!(c is RAM))
                {
                    throw new Exception(this.compareExceptionMessage);
                }

                RAM ram = (RAM)c;
                int result = 0;

                if (this.memory > ram.memory)
                {
                    result++;
                }
                else if (this.memory < ram.memory)
                {
                    result--;
                }

                return result;

            }
        }

        public class Screen : Part
        {
            private float diagonal = 0;
            private int resolution_x = 0;
            private int resolution_y = 0;
            private bool touch = false;

            public Screen(float diagonal, int resolution_x, int resolution_y, bool touch)
            {
                this.diagonal = diagonal;
                this.resolution_x = resolution_x;
                this.resolution_y = resolution_y;
                this.touch = touch;
            }

            public override int CompareTo(Part c)
            {
                if (!(c is Screen))
                {
                    throw new Exception(this.compareExceptionMessage);
                }

                Screen screen = (Screen)c;
                int result = 0;

                if (this.diagonal > screen.diagonal)
                {
                    result++;
                }
                else if (this.diagonal < screen.diagonal)
                {
                    result--;
                }

                if (this.resolution_x > screen.resolution_x)
                {
                    result++;
                }
                else if (this.resolution_x < screen.resolution_x)
                {
                    result--;
                }
                if (this.resolution_y > screen.resolution_y)
                {
                    result++;
                }
                else if (this.resolution_y < screen.resolution_y)
                {
                    result--;
                }
                if (this.touch)
                {
                    result++;
                }

                return result;

            }
        }
    }
}
