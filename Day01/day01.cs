using System;
using System.IO;

namespace AoC2019
{
    class Program
    {
        static void Main (string[] args)
        {
            string inputPath = @"input";
            if (!File.Exists(inputPath))
            {
                Console.WriteLine($"Error: input file {inputPath} not found");
                return;
            }

            Console.WriteLine("Day 01");
            using (StreamReader reader = new StreamReader(inputPath))
            {
                long sum = 0;
                string line;
                // Read lines 'by hand', because ReadToEnd().Split("\n") adds an entry that breaks
                // Linq
                while (null != (line = reader.ReadLine()))
                {
                    sum += Int32.Parse(line) / 3 - 2;
                }
                Console.WriteLine($"1. Total Fuel: {sum}");
            }
        }
    }
}