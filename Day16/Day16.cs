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
            // filename = "input-ex-2-84462026";
            int[] data;
            int offset;
            using (StreamReader sr = new StreamReader(filename))
            {
                string rawString = sr.ReadLine().Trim();
                offset = Int32.Parse(rawString.Substring(0,7)); // % rawString.Length;
                // int targetLength = (10_000 * rawString.Length - offset) / rawString.Length + 1;
                // offset %= rawString.Length;
                int[] raw = rawString.ToCharArray().Select(c => Int32.Parse(c.ToString())).ToArray();
                data = new int[raw.Length * 10_000];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = raw[i % raw.Length];
                }
            }

            // System.Console.WriteLine(offset);
            // System.Console.WriteLine(data.Length);

            // Cache the computation for individual digits
            int[,] transform = new int[10,3];
            for (int d = 0; d < 10; d++)
                for (int m = 0; m < 3; m++)
                    transform[d,m] = (d * (m - 1)) % 10;
            
            //int[] modifier = new int[]{ 0, 1, 0, -1 };
            int[] modifier = new int[]{ 1, 2, 1, 0 };   // Shift by 1 to use these as indices for the transform matrix

            int[] newData = new int[data.Length];
            int[] tmp;
            for (int phase = 0; phase < 100; phase++)
            {
                int d;
                for (d = offset; d < data.Length / 2; d++)   // Loop for the digit we are computing
                {
                    int m = 0;
                    for (int i = d; i < data.Length; i++)   // Loop for the digit used in the computation
                    {
                        // Update the modifier according to the digit we're computing
                        if ((i + 1) % (d + 1) == 0)
                        {
                            m = (m == 3) ? 0 : m + 1;
                        }
                        newData[d] += transform[data[i], modifier[m]];
                    }
                    newData[d] = (newData[d] < 0) ? -newData[d] % 10 : newData[d] % 10;
                }
                int cur = 0;
                for (int e = data.Length - 1; e >= d; e--)
                {
                    cur += data[e];
                    newData[e] = (cur < 0) ? (-cur % 10) : cur % 10;   // Second half is all multiplied by 1
                }
                tmp = data;
                data = newData;
                newData = tmp;
                Array.Clear(newData, offset, newData.Length - offset);
            }
            // System.Console.WriteLine($"1. {String.Concat(data).Substring(data.Length-8)}");
            // System.Console.WriteLine($"1. {String.Concat(data).Substring(0,8)}");
            System.Console.WriteLine($"1. {String.Concat(data).Substring(offset,8)}");
        }
    }
}