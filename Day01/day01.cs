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

            // Part 01
            using (StreamReader reader = new StreamReader(inputPath))
            {
                long fuel = 0;
                string line;
                // Read lines 'by hand', because ReadToEnd().Split("\n") adds an entry that breaks
                // Linq
                while (null != (line = reader.ReadLine()))
                {
                    fuel += Int32.Parse(line) / 3 - 2;
                }
                Console.WriteLine($"1. Total Fuel: {fuel}");
            }

            // Part 2
            using (StreamReader reader = new StreamReader(inputPath))
            {
                long fuel, totalFuel = 0;
                string line;
                // Read lines 'by hand', because ReadToEnd().Split("\n") adds an entry that breaks
                // Linq
                while (null != (line = reader.ReadLine()))
                {
                    fuel = Int32.Parse(line) / 3 - 2;
                    totalFuel += fuel;
                    while (0 < (fuel = fuel / 3 - 2))
                    {
                        totalFuel += fuel;
                    }
                }
                Console.WriteLine($"2. Total Fuel: {totalFuel}");
            }
        }
    }
}