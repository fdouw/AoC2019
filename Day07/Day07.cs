using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC2019
{
    class Day07
    {
        static void Main (string[] args)
        {
            const int N_AMP = 5;    // Number of amplifiers

            // Read input and create IntCode instances
            string filename = @"input";
            IntCode amplifier;
            using (StreamReader sr = new StreamReader(filename))
            {
                // All data is on the first line
                int[] data = sr.ReadLine().Trim().Split(",").Select(s => Int32.Parse(s)).ToArray();
                // Only 1 IntCode is needed, as they all use the same instructions, and they reset
                // their state between runs anyway.  Therefor we can reuse the amplifier.
                amplifier = new IntCode(data);
            }

            int[] output = new int[N_AMP];
            int thrusterOutput = 0;
            for (int a = 0; a < N_AMP; a++)
            {
                output[0] = amplifier.Run(new int[] {a, 0}).Last();
                for (int b = 0; b < N_AMP; b++)
                {
                    if (b == a) continue;
                    output[1] = amplifier.Run(new int[] {b, output[0]}).Last();
                    for (int c = 0; c < N_AMP; c++)
                    {
                        if (c == a || c == b) continue;
                        output[2] = amplifier.Run(new int[] {c, output[1]}).Last();
                        for (int d = 0; d < N_AMP; d++)
                        {
                            if (d == a || d == b || d == c) continue;
                            output[3] = amplifier.Run(new int[] {d, output[2]}).Last();
                            int e = 10 - a - b - c - d; // No loop needed!
                            output[4] = amplifier.Run(new int[] {e, output[3]}).Last();
                            if (output[4] > thrusterOutput)
                            {
                                thrusterOutput = output[4];
                            }
                        }
                    }
                }
            }

            System.Console.WriteLine("Day 07");
            System.Console.WriteLine($"1. {thrusterOutput}");
        }
    }

    class IntCode
    {
        public const int OP_ADD = 1;   // Add
        public const int OP_MUL = 2;   // Multiply
        public const int OP_IN = 3;    // Input value
        public const int OP_OUT = 4;   // Output value
        public const int OP_JMP_T = 5; // Jump if true
        public const int OP_JMP_F = 6; // Jump if false
        public const int OP_LT = 7;    // Less than
        public const int OP_EQ = 8;    // Equals
        public const int OP_END = 99;

        public const int MOD_POS = 0;
        public const int MOD_VAL = 1;

        private int[] data;

        public IntCode (int[] data)
        {
            this.data = new int[data.Length];
            for (int i = 0; i < data.Length; i++) this.data[i] = data[i];
        }

        public IEnumerable<int> Run(int[] inputs, bool verbose = false)
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
}