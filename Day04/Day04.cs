using System;
using System.Collections.Generic;
using System.Linq;

namespace AoC2019
{
    class Day04
    {
        static void Main (string[] args)
        {
            // Input for this problem
            // NB: these do not match the criteria, hence no issues with incl/excl boundaries
            int lo = 137683;
            int hi = 596253;

            System.Console.WriteLine("Day 04");

            int count1 = 0;
            int count2 = 0;
            int num;
            for (int a = 1; a < 6; a++)     // Small optimisation: must start at lo.
            {
                for (int b = a; b < 10; b++)
                {
                    for (int c = b; c < 10; c++)
                    {
                        for (int d = c; d < 10; d++)
                        {
                            for (int e = d; e < 10; e++)
                            {
                                for (int f = e; f < 10; f++)
                                {
                                    num = a * 100_000 + b * 10_000 + c * 1_000 + d * 100 + e * 10 + f;
                                    if (num >= lo && num <= hi)
                                    {
                                        // Count duplicate digits.  They are adjacent by construction.
                                        var digit_count = new int[] {a,b,c,d,e,f}.GroupBy(x => x).Select(x => x.Count());
                                        if (digit_count.Max() > 1)
                                        {
                                            count1++;
                                            if (digit_count.Contains(2))
                                            {
                                                count2++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            System.Console.WriteLine($"1. {count1}");
            System.Console.WriteLine($"1. {count2}");
        }
    }
}