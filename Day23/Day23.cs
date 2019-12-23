using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AoC2019
{
    class Day23
    {
        internal static List<long> Result = new List<long>();

        public static void Main (string[] args)
        {
            System.Console.WriteLine("Day 23");

            string filename = "input";
            IcmManager[] manager = new IcmManager[50];
            Task<long>[] tasks = new Task<long>[50];
            using (StreamReader sr = new StreamReader(filename))
            {
                long[] code = sr.ReadLine().Trim().Split(",").Select(s => Int64.Parse(s)).ToArray();
                for (long i = 0; i < 50; i++)
                {
                    manager[i] = new IcmManager(code);
                }
                for (long i = 0; i < 50; i++)
                {
                    long j = i;
                    tasks[i] = Task<long>.Run(() => manager[j].Start());
                }
            }

            // Part 1
            long result = Task<long>.WaitAny(tasks);
            System.Console.WriteLine($"1. {result}");
            foreach(long res in Day23.Result)
                System.Console.WriteLine($"1x {res}");
            // for (int i = 0; i < 50; i++)
            //     System.Console.WriteLine($" {i,2}: {manager[i].Result,4}");
        }
    }

    class IcmManager
    {
        private static List<IcmManager> allManagers = new List<IcmManager>();

        private IntCodeMachine icm;

        private Queue<long> input = new Queue<long>();
        private Queue<long> output = new Queue<long>();

        internal long Result { get; private set; }

        internal IcmManager (long[] code)
        {
            icm = new IntCodeMachine($"Icm[{allManagers.Count}]", code);
            icm.Input += new IcmInputReader(InputToIcm);
            icm.Output += new IcmOutputWriter(OutputFromIcm);
            icm.Reset();
            allManagers.Add(this);
        }

        internal long Start ()
        {
            System.Console.WriteLine($"Started");
            icm.Run();
            return Result;
        }

        private bool sendY = false;
        public long InputToIcm ()
        {
            lock (input)
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
        }

        public void OutputFromIcm (long value)
        {
            lock (output)
            {
                lock (Day23.Result)
                {
                    output.Enqueue(value);
                    if (output.Count >= 3)
                    {
                        long address = output.Dequeue();
                        long x = output.Dequeue();
                        long y = output.Dequeue();
                        allManagers[(int)address].AddPacket(x, y);

                        if (address == 255)
                        {
                            Day23.Result.Add(y);
                            System.Console.WriteLine($"{y}");
                            Result = y;
                            icm.Stop();
                        }
                    }
                }
            }
        }

        public void AddPacket (long x, long y)
        {
            input.Enqueue(x);
            input.Enqueue(y);
        }
    }
}