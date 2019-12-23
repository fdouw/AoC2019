using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC2019
{
    class Day23
    {
        public const int N = 50;

        public static void Main (string[] args)
        {
            System.Console.WriteLine("Day 23");

            string filename = "input";
            IcmManager[] manager = new IcmManager[N];
            using (StreamReader sr = new StreamReader(filename))
            {
                long[] code = sr.ReadLine().Trim().Split(",").Select(s => Int64.Parse(s)).ToArray();
                for (int i = 0; i < N; i++)
                {
                    manager[i] = new IcmManager(code, i);
                }
            }

            // Part 1
            long answer = 0;
            bool running = true;
            while (running)
            {
                for (int i = 0; i < N; i++)
                {
                    if (manager[i].Active && manager[i].Tick(out answer))
                    {
                        running = false;
                        break;
                    }
                }
            }
            System.Console.WriteLine($"1. {answer}");
        }
    }

    class IcmManager
    {
        private static IcmManager[] allManagers = new IcmManager[Day23.N];

        private IntCodeMachine icm;

        private Queue<long> input = new Queue<long>();
        private Queue<long> output = new Queue<long>();

        private long? answer;

        internal bool Active { get => icm.Active; }

        internal IcmManager (long[] code, int Id)
        {
            input.Enqueue(Id);
            icm = new IntCodeMachine($"Icm[{Id}]", code);
            icm.Input += new IcmInputReader(this.InputToIcm);
            icm.Output += new IcmOutputWriter(this.OutputFromIcm);
            icm.Reset();
            allManagers[Id] = this;
        }
        
        internal bool Tick (out long value)
        {
            icm.Tick();
            if (answer != null)
            {
                value = (long)answer;
                return true;
            }
            else
            {
                value = Int64.MaxValue;
                return false;
            }
        }

        private bool sendY = true;  // offset to send the id first, then x,y in pairs
        public long InputToIcm ()
        {
            if (sendY || input.Count > 1)
            {
                sendY = !sendY;
                return input.Dequeue();
            }
            else
            {
                return -1;
            }
        }

        public void OutputFromIcm (long value)
        {
            output.Enqueue(value);
            if (output.Count >= 3)
            {
                long address = output.Dequeue();
                long x = output.Dequeue();
                long y = output.Dequeue();

                if (address == 255)
                {
                    answer = y;
                }
                else
                {
                    allManagers[(int)address].AddPacket(x, y);
                    System.Console.WriteLine($"{icm.name}: send packet ({x}, {y}) to #{address}");
                }
            }
        }

        public void AddPacket (long x, long y)
        {
            System.Console.WriteLine($"{icm.name}: received packet ({x}, {y})");
            input.Enqueue(x);
            input.Enqueue(y);
        }
    }
}