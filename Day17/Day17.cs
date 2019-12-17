using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AoC2019
{
    class Day17
    {
        private static int[] inputData;
        private static int inputPointer = 0;
        private static List<int> outputData = new List<int>();

        public static int InputWriter ()
        {
            // System.Console.WriteLine("InputWriter called");
            if (inputPointer < inputData.Length)
            {
                // System.Console.WriteLine($"Writing {inputData[inputPointer]} to ICM.");
                return inputData[inputPointer++];
            }
            else
            {
                System.Console.WriteLine("Writing -1 to ICM");
                return -1;
            }
        }

        public static void OutputReader (int output)
        {
            // System.Console.WriteLine("Outputreader called");
            outputData.Add(output);
        }

        static void Main(string[] args)
        {
            System.Console.WriteLine("Day 17");

            string filename = "input";
            IntCodeMachine icm;
            using (StreamReader sr = new StreamReader(filename))
            {
                int[] data = sr.ReadLine().Trim().Split(",").Select(s => Int32.Parse(s)).ToArray();
                icm = new IntCodeMachine("ASCII", data);
            }

            // Read the grid (and print it)
            int output;
            StringBuilder sb = new StringBuilder();
            icm.Reset();
            while (icm.active)
            {
                if (!icm.Next(new int[]{}, out output))
                {
                    break;
                }
                sb.Append((char)output);
            }
            string view = sb.ToString().Trim();
            char[][] grid = view.Split().Select(s => s.ToCharArray()).ToArray();
            // System.Console.WriteLine(view);

            // Part 1
            int sum = 0;
            for (int y = 1; y < grid.Length - 1; y++)
            {
                for (int x = 1; x < grid[y].Length - 1; x++)
                {
                    if (grid[y][x] == '#' && grid[y-1][x] == '#' && grid[y+1][x] == '#' && grid[y][x-1] == '#' && grid[y][x+1] == '#')
                    {
                        sum += x * y;
                    }
                }
            }
            System.Console.WriteLine($"1. {sum}");

            // Part 2
            // I mostly did this by eye & hand, from the grid drawn by the code above
            // A B A B C B A C B C 
            //
            // A L12L08R10R10
            // B L06L04L12
            // C R10L08L04R10
            // =>
            // 76,44,49,50,44,76,44,56,44,82,44,49,48,44,82,44,49,48
            // 76,44,54,44,76,44,52,44,76,44,49,50
            // 82,44,49,48,44,76,44,56,44,76,44,52,44,82,44,49,48
            //
            // A 65
            // B 66
            // C 67
            // L 76
            // R 82
            // , 44
            // \n 10
            // n 110
            // y 121

            int[] main = new int[]{65,44,66,44,65,44,66,44,67,44,66,44,65,44,67,44,66,44,67,10};
            int[] A = new int[]{76,44,49,50,44,76,44,56,44,82,44,49,48,44,82,44,49,48,10};
            int[] B = new int[]{76,44,54,44,76,44,52,44,76,44,49,50,10};
            int[] C = new int[]{82,44,49,48,44,76,44,56,44,76,44,52,44,82,44,49,48,10};
            int[] video = new int[]{110,10};
            inputData = new List<int>()
                .Concat(main)
                .Concat(A)
                .Concat(B)
                .Concat(C)
                .Concat(video)
                .ToArray();

            icm.Reset();
            icm.SetRegister(0,2);
            icm.InputReader += new IcmInputReader(InputWriter);
            icm.OutputWriter += new IcmOutputWriter(OutputReader);
            icm.Run();

            // Read output
            sb.Clear();
            foreach (int item in outputData)
            {
                sb.Append((char)item);
            }
            System.Console.WriteLine(sb.ToString());
            System.Console.WriteLine(outputData.Last());
        }
    }
}