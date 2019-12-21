using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC2019
{
    class Day21
    {
        public static void Main (string[] args)
        {
            System.Console.WriteLine("Day 21");

            string filename = "input";
            AsciiIcm icm;
            using (StreamReader sr = new StreamReader(filename))
            {
                int[] code = sr.ReadLine().Trim().Split(",").Select(s => Int64.Parse(s)).ToArray();
                icm = new AsciiIcm(code);
            }

            // Part 1
            string instructions = "NOT A J\nNOT C T\nAND D T\nOR T J\nWALK\n";
            icm.Reset(instructions);
            icm.Run();
            System.Console.WriteLine($"Part 1:\n{icm.GetOutputString()}");

            // Part 2
            instructions = "NOT A J\nOR B T\nAND C T\nNOT T T\nAND D T\nAND H T\nOR T J\nRUN\n";
            icm.Reset(instructions);
            icm.Run();
            System.Console.WriteLine($"\nPart 2:\n{icm.GetOutputString()}");
        }
    }

    class AsciiIcm
    {
        private IntCodeMachine icm;
        private string ascii;

        private int[] input;
        private int inPtr;

        private List<int> output = new List<int>();

        public AsciiIcm (int[] code, string ascii = "")
        {
            icm = new IntCodeMachine("AsciiIcm", code);
            icm.Input += new IcmInputReader(SendInput);
            icm.Output += new IcmOutputWriter(ReceiveOutput);
            Reset(ascii);
        }

        public void Reset(string ascii, bool insertCommas = false)
        {
            this.ascii = ascii;
            Reset(insertCommas);
        }

        public void Reset(bool insertCommas = false)
        {
            input = ascii.ToCharArray().Select(c => (int)c).ToArray();
            if (insertCommas)
            {
                List<int> tmp = new List<int>();
                foreach (int character in input)
                {
                    tmp.Add(character);
                    tmp.Add((int)',');
                }
                if (tmp.Count() > 0)
                    tmp.RemoveAt(tmp.Count() - 1);
                input = tmp.ToArray();
            }
            inPtr = 0;
            output.Clear();
            icm.Reset();
        }

        public void Run ()
        {
            icm.Run();
        }

        public IEnumerator<int> GetOutput () => output.GetEnumerator();
        public IEnumerable<char> GetAsciiOutput () => output.Select(n => (char)n);
        public string GetOutputString () => String.Concat(output.Select(n => (n < 256) ? $"{(char)n}" : $"{n}"));

        public int PeekOutput() => output.First();

        public int SendInput()
        {
            if (inPtr >= input.Length)
                throw new Exception("No input available.");
            return input[inPtr++];
        }

        public void ReceiveOutput(int data)
        {
            output.Add(data);
        }
    }
}