using System;
using System.IO;
using System.Linq;

namespace AoC2019
{
    class Day02
    {
        class OpCode
        {
            public static int ADD = 1;
            public static int MUL = 2;
            public static int END = 99;
        }

        static void Main (string[] args)
        {            
            string inputPath = @"input";
            if (!File.Exists(inputPath))
            {
                Console.WriteLine($"Error: input file {inputPath} not found");
                return;
            }
            Console.WriteLine("Day 02");

            using (StreamReader sr = new StreamReader(inputPath))
            {
                // All input is on the first line
                string line = sr.ReadLine();
                int[] data = line.Split(',').Select(x => Int32.Parse(x)).ToArray();
                int[] codes = new int[data.Length];

                // Part 1
                Console.WriteLine($"1. code: {execute(12, 02)}");

                // Part 2
                for (int noun = 0; noun < 100; noun++)
                {
                    for (int verb = 0; verb < 100; verb++)
                    {
                        // Check if we have found the output we're asked for
                        if (execute(noun, verb) == 19690720)
                        {
                            Console.WriteLine($"2. input: {100 * noun + verb}");
                            return;
                        }
                    }
                }

                // Function to compute the intcode
                int execute (int noun, int verb)
                {
                    // Copy orignal array data
                    for (int i = 0; i < data.Length; i++) codes[i] = data[i];

                    codes[1] = noun;
                    codes[2] = verb;

                    // Execute intcode
                    int pos = 0;
                    while (codes[pos] != OpCode.END)
                    {
                        if (codes[pos] == OpCode.ADD)
                        {
                            codes[codes[pos + 3]] = codes[codes[pos + 1]] + codes[codes[pos + 2]];
                        }
                        else if (codes[pos] == OpCode.MUL)
                        {
                            codes[codes[pos + 3]] = codes[codes[pos + 1]] * codes[codes[pos + 2]];
                        }
                        pos += 4;
                    }

                    // Result is in address 0
                    return codes[0];
                }
            }
        }
    }
}