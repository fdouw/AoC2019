using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC2019
{
    class Day05
    {
        class IntCode
        {
            private const int OP_ADD = 1;
            private const int OP_MUL = 2;
            private const int OP_IN = 3;
            private const int OP_OUT = 4;
            private const int OP_END = 99;

            private const int MOD_POS = 0;
            private const int MOD_VAL = 1;

            public static IEnumerable<int> Run(int[] data, int[] inputs)
            {
                // First copy the data (code) so as not to mess up the original
                int[] code = new int[data.Length];
                for (int i = 0; i < data.Length; i++) code[i] = data[i];

                int ptr = 0;    // current position in the program
                int in_ptr = 0; // current position in the input array

                bool running = true;
                int a, b, c;
                while (running)
                {
                    int cmd = code[ptr++];
                    int opcode = cmd % 100;
                    int mode1 = (cmd / 100) % 10;
                    int mode2 = (cmd / 1000) % 10;
                    int mode3 = (cmd / 10000);

                    switch (opcode)
                    {
                        case OP_ADD:
                            System.Console.WriteLine($"> ADD {ptr} {mode1} {mode2}");
                            a = (mode1 == MOD_POS) ? code[code[ptr++]] : code[ptr++];
                            b = (mode2 == MOD_POS) ? code[code[ptr++]] : code[ptr++];
                            c = code[ptr++];    // Write address: never in immediate mode
                            code[c] = a + b;
                            break;
                        case OP_MUL:
                            System.Console.WriteLine($"> MUL {ptr} {mode1} {mode2}");
                            a = (mode1 == MOD_POS) ? code[code[ptr++]] : code[ptr++];
                            b = (mode2 == MOD_POS) ? code[code[ptr++]] : code[ptr++];
                            c = code[ptr++];    // Write address: never in immediate mode
                            code[c] = a * b;
                            break;
                        case OP_IN:
                            System.Console.WriteLine($"> IN {ptr} {mode1}");
                            a = inputs[in_ptr++];   // Assume enough inputs
                            b = code[ptr++];    // Write address: never in immediate mode
                            code[b] = a;
                            break;
                        case OP_OUT:
                            System.Console.WriteLine($"> OUT {ptr} {mode1}");
                            yield return (mode1 == MOD_POS) ? code[code[ptr++]] : code[ptr++];
                            break;
                        case OP_END:
                            running = false;
                            break;
                    }
                }
            }
        }

        static void Main (string[] args)
        {            
            string inputPath = @"input-edit";
            if (!File.Exists(inputPath))
            {
                Console.WriteLine($"Error: input file {inputPath} not found");
                return;
            }
            Console.WriteLine("Day 05");

            int[] data;
            using (StreamReader sr = new StreamReader(inputPath))
            {
                List<int> raw = new List<int>();
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    int tmp;
                    if (Int32.TryParse(line, out tmp))
                    {
                        raw.Add(tmp);
                    }
                    else
                    {
                        System.Console.WriteLine("WARNING: Could not read int");
                    }
                }
                data = raw.ToArray();
            }

            // Part 1
            foreach (int output in IntCode.Run(data, new int[]{1}))
            {
                System.Console.WriteLine($"{output}");
            }
        }
    }
}