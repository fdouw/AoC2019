using System;
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

            //int count = Enumerable.Range(lo,hi-lo).Where(n => isMatch(n)).Count();

            int count = 0;
            int num = 0;
            for (int a = 0; a < 10; a++)
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
                                    if (a == b || b == c || c == d || d == e || e == f)
                                    {
                                        num = a * 100_000 + b * 10_000 + c * 1000 + d * 100 + e * 10 + f;
                                        System.Console.WriteLine($"num: {num}");
                                        if (num >= lo && num <= hi)
                                        {
                                            count++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            System.Console.WriteLine($"1. {count}");
        }

        static bool isMatch (int num)
        {
            int nxt, prv = 0;
            int same = 0;
            while (num > 0)
            {
                nxt = num % 10;
                if (nxt > prv) return false;    // we're testing right-to-left
                else if (nxt == prv) same++;
            }
            return same == 1;
        }
    }
}