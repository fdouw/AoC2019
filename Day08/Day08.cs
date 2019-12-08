using System;
using System.Linq;
using System.IO;

namespace AoC2019
{
    class Day08
    {
        static void Main (string[] args)
        {
            string filename = "input";
            string[] data;
            int width = 25;         // specs of the image
            int len = width * 6;
            using (StreamReader sr = new StreamReader(filename))
            {
                string raw = sr.ReadLine();   // All data is on the first line
                data = new string[raw.Length / len];
                for (int i = 0; i < data.Length; i++) data[i] = raw.Substring(i * len, len);
            }

            System.Console.WriteLine("Day 08");

            // Part 1
            var hash = data.Select(x => new int[]{x.Count(c => c == '0'), x.Count(c => c == '1') * x.Count(c => c == '2')})
                           .OrderBy<int[],int>(x => x[0])
                           .First();
            System.Console.WriteLine($"1. {hash[1]}");

            // Part 2
            const int BLACK = 0;
            const int WHITE = 1;
            const int TRANS = 2;
            int[] image = new int[len];
            int[][] layer = data.Select(l => l.ToCharArray().Select(c => Int32.Parse(c.ToString())).ToArray()).ToArray();
            for (int i = 0; i < len; i++)
            {
                for (int l = 0; l < layer.Length; l++)
                {
                    if (layer[l][i] != TRANS)
                    {
                        image[i] = layer[l][i];
                        break;
                    }
                    image[i] = TRANS;
                }
            }

            // Print the image in ascii art
            System.Console.WriteLine("2. Image:");
            char[] pixel = image.Select(p => (p == 0) ? '#' : (p == 1) ? ' ' : '/').ToArray();
            for (int i = 0; i < len; i++)
            {
                System.Console.Write(pixel[i]);
                if ((i + 1) % width == 0) System.Console.WriteLine("");
            }
        }
    }
}