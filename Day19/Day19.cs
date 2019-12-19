using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC2019
{
    class Day19
    {
        public static void Main (string[] args)
        {
            System.Console.WriteLine("Day 19");

            string filename = "input";
            IntCodeMachine icm;
            using (StreamReader sr = new StreamReader(filename))
            {
                int[] intcode = sr.ReadLine().Trim().Split(',').Select(s => Int32.Parse(s)).ToArray();
                icm = new IntCodeMachine("robot", intcode);
            }

            // Part 1
            // char c;
            int output;
            int beamCover = 0;
            // System.Console.Write("   ");
            // for (int x = 0; x < 50; x += 5) System.Console.Write($"{x,-5}");
            // System.Console.WriteLine();
            for (int y = 0; y < 50; y++)
            {
                // System.Console.Write($"{y,2} ");
                for (int x = 0; x < 50; x++)
                {
                    icm.Start(new int[] {x, y}, out output);
                    beamCover += output;
                    // c = (output == 1) ? '#' : '.';
                    // System.Console.Write(c);
                }
                // System.Console.WriteLine();
            }
            System.Console.WriteLine($"1. {beamCover}");

            // Part 2
            // Some assumptions:
            // the beam is connected, ie:
            // - if (x,y) is beam and (x + 99, y - 99) is beam, then everything in between is beam
            // - no spurious traces of beam below / left of the actual beam
            int prevStart = 0;      // Improve performance by ignoring the void below the beam
            bool searching = true;
            int L = 99;             // 100x100 block means x and x+99 inclusive
            for (int y = L; searching && y < 10_000; y++)
            {
                for (int x = prevStart; x < 10_000; x++)
                {
                    if (icm.Start(new int[]{x, y}, out output) && output == 1)
                    {
                        // prevStart = (x < y) ? x : y;
                        prevStart = x;
                        if (icm.Start(new int[]{x + L, y - L}, out output) && output == 1)
                        {
                            System.Console.WriteLine($"2. {x * 10_000 + y - L}");
                            searching = false;
                        }
                        break;
                    }
                }
            }
        }
    }
}