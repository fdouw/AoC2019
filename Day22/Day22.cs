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

            string[] shuffle;
            string filename = "input";
            using (StreamReader sr = new StreamReader(filename))
            {
                shuffle = sr.ReadToEnd().Trim().Split("\n").ToArray();
            }

            // Part 1
            // Deck cards = new Deck(10_007);
            // ShuffleDeck(cards, shuffle);
            // System.Console.WriteLine($"1. {cards.IndexOf(2019)}");

            LightweightDeck lwCards = new LightweightDeck(10_007, 2019);
            ShuffleDeck(lwCards, shuffle);
            System.Console.WriteLine($"1b {lwCards.Position}");

            // Part 2
            // cards = new Deck(119315717514047);
            // ShuffleDeck(cards, shuffle, 101741582076661);
            // System.Console.WriteLine($"2. {cards.ItemAt(2020)}");
        }

        private static void ShuffleDeck (IDeck cards, string[] shuffle, long times = 1)
        {
            for (int i = 0; i < times; i++)
            {
                foreach(string line in shuffle)
                {
                    if (line.StartsWith("deal with increment"))
                    {
                        cards.DealIncrement(Int64.Parse(line.Substring(20)));
                    }
                    else if (line == "deal into new stack")
                    {
                        cards.Reverse();
                    }
                    else if (line.StartsWith("cut"))
                    {
                        cards.Cut(Int64.Parse(line.Substring(4)));
                    }
                }
            }
        }
    }

    class Deck : IDeck
    {
        internal long Size { get; }
        internal long Last { get => Size - 1; }

        private long[] cards;

        internal Deck (long s)
        {
            Size = s;
            cards = new long[Size];
            for (long i = 0; i < Size; i++)
                cards[i] = i;
        }

        public void Reverse ()
        {
            for (long i = 0; i < Size / 2; i++)
            {
                long tmp = cards[i];
                cards[i] = cards[Last - i];
                cards[Last - i] = tmp;
            }
        }

        public void Cut (long n)
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
                long[] tmp = new long[Math.Abs(n)];
                for (long i = 0; i < n; i++)
                    tmp[i] = cards[i];
                for (long i = n; i < Size; i++)
                    cards[i-n] = cards[i];
                for (long i = 0; i < n; i++)
                    cards[Size - n + i] = tmp[i];
            }
        }

        public void DealIncrement (long n)
        {
            long[] tmp = new long[Size];
            for (long i = 0; i < Size; i++)
                tmp[i * n % Size] = cards[i];   // Probably more efficient to have an extra j += n
            cards = tmp;
        }

        internal long IndexOf (long item)
        {
            for (long i = 0; i < Size; i++)
                if (cards[i] == item)
                    return i;
            return -1;
        }

        internal long ItemAt (long index) => cards[index];
    }

    // Instead of simulating an entire deck, follow the position of a single card
    class LightweightDeck : IDeck
    {
        internal long Size { get; }
        internal long Last { get => Size - 1; }
        internal long Card { get; }
        internal long Position { get; private set; }

        internal LightweightDeck (long size, long target)
        {
            this.Size = size;
            this.Card = target;
            this.Position = Card;
        }

        public void Reverse ()
        {
            Position = Last - Position;
        }

        public void Cut (long n)
        {
            Position = (Position - n + Size) % Size;
        }

        public void DealIncrement (long n)
        {
            Position = Position * n % Size;
        }
    }

    interface IDeck
    {
        void Reverse();
        void Cut(long n);
        void DealIncrement(long n);
    }
}