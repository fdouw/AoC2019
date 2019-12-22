using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC2019
{
    class Day22
    {
        public static void Main (string[] args)
        {
            System.Console.WriteLine("Day 22");

            Deck cards = new Deck();
            string filename = "input";
            using (StreamReader sr = new StreamReader(filename))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("deal with increment"))
                    {
                        System.Console.WriteLine($"{Int32.Parse(line.Substring(20))}");
                        cards.DealIncrement(Int32.Parse(line.Substring(20)));
                    }
                    else if (line == "deal into new stack")
                    {
                        System.Console.WriteLine($"new stack");
                        cards.Reverse();
                    }
                    else if (line.StartsWith("cut"))
                    {
                        System.Console.WriteLine($"{Int32.Parse(line.Substring(4))}");
                        cards.Cut(Int32.Parse(line.Substring(4)));
                    }
                }
                System.Console.WriteLine($"1. {cards.IndexOf(2019)}");
                // System.Console.WriteLine($"{cards.ToString()}");
            }
        }
    }

    class Deck
    {
        internal int Size { get; }
        internal int Last { get => Size - 1; }

        private int[] cards;

        internal Deck (int s = 10_007)
        {
            Size = s;
            cards = new int[Size];
            for (int i = 0; i < Size; i++)
                cards[i] = i;
        }

        internal void Reverse ()
        {
            for (int i = 0; i < Size / 2; i++)
            {
                int tmp = cards[i];
                cards[i] = cards[Last - i];
                cards[Last - i] = tmp;
            }
        }

        internal void Cut (int n)
        {
            if (n < 0)
            {
                // Maybe not the most efficient, but more readable
                Reverse();
                Cut(-n);
                Reverse();
            }
            else
            {
                int[] tmp = new int[Math.Abs(n)];
                for (int i = 0; i < n; i++)
                    tmp[i] = cards[i];
                for (int i = n; i < Size; i++)
                    cards[i-n] = cards[i];
                for (int i = 0; i < n; i++)
                    cards[Size - n + i] = tmp[i];
            }
        }

        internal void DealIncrement (int n)
        {
            int[] tmp = new int[Size];
            for (int i = 0; i < Size; i++)
                tmp[i * n % Size] = cards[i];   // Probably more efficient to have an extra j += n
            cards = tmp;
        }

        internal int IndexOf (int item)
        {
            for (int i = 0; i < Size; i++)
                if (cards[i] == item)
                    return i;
            return -1;
        }

        internal string ToString ()
        {
            return String.Join(' ', cards);
        }
    }
}