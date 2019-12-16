using System;
using System.IO;
using System.Linq;

namespace AoC2019
{
    class Day16
    {
        public static void Main(string[] args)
        {
            System.Console.WriteLine("Day 16");

            // Read the data
            string filename = "input";
            int[] data;
            using (StreamReader sr = new StreamReader(filename))
            {
                data = sr.ReadLine().Trim().ToCharArray().Select(c => Int32.Parse(c.ToString())).ToArray();
            }

            // Cache the computation for individual digits
            int[,] transform = new int[10,3];
            for (int d = 0; d < 10; d++)
                for (int m = 0; m < 3; m++)
                    transform[d,m] = (d * (m - 1)) % 10;
            
            //int[] modifier = new int[]{ 0, 1, 0, -1 };
            int[] modifier = new int[]{ 1, 2, 1, 0 };   // Shift by 1 to use these as indices for the transform matrix

            int[] newData = new int[data.Length];
            for (int phase = 0; phase < 100; phase++)
            {
                for (int d = 0; d < data.Length; d++)   // Loop for the digit we are computing
                {
                    int m = 0;
                    for (int i = 0; i < data.Length; i++)   // Loop for the digit used in the computation
                    {
                        // Update the modifier according to the digit we're computing
                        if ((i + 1) % (d + 1) == 0)
                        {
                            m = (m + 1) % 4;                    
                        }
                        newData[d] += transform[data[i], modifier[m]];
                    }
                    newData[d] = Math.Abs(newData[d] % 10);
                }
                System.Console.WriteLine($"{phase,3}: {String.Concat(newData).Substring(0,8)}");
                data = newData;
                newData = new int[data.Length];
            }
        }
    }
}