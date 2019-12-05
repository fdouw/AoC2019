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
            private const int OP_ADD = 1;   // Add
            private const int OP_MUL = 2;   // Multiply
            private const int OP_IN = 3;    // Input value
            private const int OP_OUT = 4;   // Output value
            private const int OP_JMP_T = 5; // Jump if true
            private const int OP_JMP_F = 6; // Jump if false
            private const int OP_LT = 7;    // Less than
            private const int OP_EQ = 8;    // Equals
            private const int OP_END = 99;

            private const int MOD_POS = 0;
            private const int MOD_VAL = 1;

            public static IEnumerable<int> Run(int[] data, int[] inputs, bool verbose = false)
            {
                // First copy the data (code) so as not to mess up the original
                int[] code = new int[data.Length];
                for (int i = 0; i < data.Length; i++) code[i] = data[i];
                // System.Console.WriteLine($"Code length: {code.Length}");

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

                    switch (opcode)
                    {
                        case OP_ADD:
                            if (verbose) System.Console.WriteLine($"> ADD {ptr} {mode1} {mode2}");
                            a = (mode1 == MOD_POS) ? code[code[ptr++]] : code[ptr++];
                            b = (mode2 == MOD_POS) ? code[code[ptr++]] : code[ptr++];
                            c = code[ptr++];    // Write address: never in immediate mode
                            code[c] = a + b;
                            break;
                        case OP_MUL:
                            if (verbose) System.Console.WriteLine($"> MUL {ptr} {mode1} {mode2}");
                            a = (mode1 == MOD_POS) ? code[code[ptr++]] : code[ptr++];
                            b = (mode2 == MOD_POS) ? code[code[ptr++]] : code[ptr++];
                            c = code[ptr++];    // Write address: never in immediate mode
                            code[c] = a * b;
                            break;
                        case OP_IN:
                            if (verbose) System.Console.WriteLine($"> IN {ptr} {mode1}");
                            a = inputs[in_ptr++];   // Assume enough inputs
                            b = code[ptr++];    // Write address: never in immediate mode
                            code[b] = a;
                            break;
                        case OP_OUT:
                            if (verbose) System.Console.WriteLine($"> OUT {ptr} {mode1}");
                            yield return (mode1 == MOD_POS) ? code[code[ptr++]] : code[ptr++];
                            break;
                        case OP_JMP_T:
                            if (verbose) System.Console.WriteLine($"> JMP TRUE {ptr} {mode1}");
                            a = (mode1 == MOD_POS) ? code[code[ptr++]] : code[ptr++];
                            b = (mode2 == MOD_POS) ? code[code[ptr++]] : code[ptr++];
                            if (a != 0) ptr = b;
                            break;
                        case OP_JMP_F:
                            if (verbose) System.Console.WriteLine($"> JMP FALSE {ptr} {mode1}");
                            a = (mode1 == MOD_POS) ? code[code[ptr++]] : code[ptr++];
                            b = (mode2 == MOD_POS) ? code[code[ptr++]] : code[ptr++];
                            if (a == 0) ptr = b;
                            break;
                        case OP_LT:
                            if (verbose) System.Console.WriteLine($"> LESS THAN {ptr} {mode1}");
                            a = (mode1 == MOD_POS) ? code[code[ptr++]] : code[ptr++];
                            b = (mode2 == MOD_POS) ? code[code[ptr++]] : code[ptr++];
                            c = code[ptr++];    // Write address: never in immediate mode
                            code[c] = (a < b) ? 1 : 0;
                            break;
                        case OP_EQ:
                            if (verbose) System.Console.WriteLine($"> EQUAL {ptr} {mode1}");
                            a = (mode1 == MOD_POS) ? code[code[ptr++]] : code[ptr++];
                            b = (mode2 == MOD_POS) ? code[code[ptr++]] : code[ptr++];
                            c = code[ptr++];    // Write address: never in immediate mode
                            code[c] = (a == b) ? 1 : 0;
                            break;
                        case OP_END:
                            running = false;
                            break;
                        default:
                            System.Console.WriteLine($"Unknown OpCode: {opcode}");
                            running = false;
                            break;
                    }
                }
            }
        }

        static int[] ReadCode (string filename)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException($"Could not find \"{filename}\"");
            }
            using (StreamReader sr = new StreamReader(filename))
            {
                string line = sr.ReadLine();    // All data is on the first line
                return line.Split(',').Select(x => Int32.Parse(x)).ToArray();
            }
        }

        static void Main (string[] args)
        {
            // // Tests from Reddit
            // System.Console.WriteLine("Output zero");
            // int[] data = new int[] {1,0,3,3,1005,2,10,5,1,0,4,1,99};
            // foreach (int output in IntCode.Run(data, new int[]{8})) System.Console.WriteLine($">> {output}");
            // System.Console.WriteLine("\n");
            // System.Console.WriteLine("Countdown");
            // data = new int[] {101,-1,7,7,4,7,1105,11,0,99};
            // foreach (int output in IntCode.Run(data, new int[]{8})) System.Console.WriteLine($">> {output}");

            // Try examples to test
            // System.Console.WriteLine("input-ex-eq8");
            // int[] data = ReadCode(@"input-ex-eq8");
            // foreach (int output in IntCode.Run(data, new int[]{8})) System.Console.WriteLine($">> {output}");
            // foreach (int output in IntCode.Run(data, new int[]{12})) System.Console.WriteLine($">> {output}");
            // System.Console.WriteLine("input-ex-eq8-im");
            // data = ReadCode(@"input-ex-eq8-im");
            // foreach (int output in IntCode.Run(data, new int[]{8})) System.Console.WriteLine($">> {output}");
            // foreach (int output in IntCode.Run(data, new int[]{12})) System.Console.WriteLine($">> {output}");
            // System.Console.WriteLine();
            // System.Console.WriteLine("input-ex-lt8");
            // data = ReadCode(@"input-ex-lt8");
            // foreach (int output in IntCode.Run(data, new int[]{5})) System.Console.WriteLine($">> {output}");
            // foreach (int output in IntCode.Run(data, new int[]{12})) System.Console.WriteLine($">> {output}");
            // System.Console.WriteLine("input-ex-lt8-im");
            // data = ReadCode(@"input-ex-lt8-im");
            // foreach (int output in IntCode.Run(data, new int[]{5})) System.Console.WriteLine($">> {output}");
            // foreach (int output in IntCode.Run(data, new int[]{12})) System.Console.WriteLine($">> {output}");
            // System.Console.WriteLine();
            // System.Console.WriteLine("input-ex-jmp-pm");
            // data = ReadCode(@"input-ex-jmp-pm");
            // foreach (int output in IntCode.Run(data, new int[]{0})) System.Console.WriteLine($">> {output}");
            // foreach (int output in IntCode.Run(data, new int[]{12})) System.Console.WriteLine($">> {output}");
            // System.Console.WriteLine("input-ex-jmp-im");
            // data = ReadCode(@"input-ex-jmp-im");
            // foreach (int output in IntCode.Run(data, new int[]{0})) System.Console.WriteLine($">> {output}");
            // foreach (int output in IntCode.Run(data, new int[]{12})) System.Console.WriteLine($">> {output}");
            // System.Console.WriteLine();
            // System.Console.WriteLine("input-ex-cmp8 -- 0");
            // data = ReadCode(@"input-ex-cmp8");
            // foreach (int output in IntCode.Run(data, new int[]{0})) System.Console.WriteLine($">> {output}");
            // System.Console.WriteLine("input-ex-cmp8 -- 8");
            // foreach (int output in IntCode.Run(data, new int[]{8})) System.Console.WriteLine($">> {output}");
            // System.Console.WriteLine("input-ex-cmp8 -- 12");
            // foreach (int output in IntCode.Run(data, new int[]{12})) System.Console.WriteLine($">> {output}");

            System.Console.WriteLine("Day 05");
            int[] data = ReadCode(@"input");

            // Part 1
            System.Console.WriteLine("\nPart 1\n================================");
            foreach (int output in IntCode.Run(data, new int[]{1}))
            {
                System.Console.WriteLine($"{output}");
            }

            // Part 2
            System.Console.WriteLine("\nPart 2\n================================");
            foreach (int output in IntCode.Run(data, new int[]{5}))
            {
                System.Console.WriteLine($"{output}");
            }
        }
    }
}