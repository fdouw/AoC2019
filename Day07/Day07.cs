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
            IntCodeMachine amp;
            IntCodeMachine[] amplifier;
            using (StreamReader sr = new StreamReader(filename))
            {
                // All data is on the first line
                int[] data = sr.ReadLine().Trim().Split(",").Select(s => Int32.Parse(s)).ToArray();

                // Only 1 IntCode is needed, as they all use the same instructions, and they reset
                // their state between runs anyway.  Therefor we can reuse the amplifier.
                amp = new IntCodeMachine("Part1-test", data);

                // For part 2, we do need distinct amplifiers
                amplifier = new IntCodeMachine[N_AMP];
                for (int i = 0; i < N_AMP; i++) amplifier[i] = new IntCodeMachine(i.ToString(), data);
            }

            System.Console.WriteLine("Day 07");

            int[] output = new int[N_AMP];
            int thrusterOutput = 0;
            for (int a = 0; a < N_AMP; a++)
            {
                output[0] = amp.Run(new int[] {a, 0}, true);
                for (int b = 0; b < N_AMP; b++)
                {
                    if (b == a) continue;
                    output[1] = amp.Run(new int[] {b, output[0]}, true);
                    for (int c = 0; c < N_AMP; c++)
                    {
                        if (c == a || c == b) continue;
                        output[2] = amp.Run(new int[] {c, output[1]}, true);
                        for (int d = 0; d < N_AMP; d++)
                        {
                            if (d == a || d == b || d == c) continue;
                            output[3] = amp.Run(new int[] {d, output[2]}, true);
                            int e = 10 - a - b - c - d; // No loop needed!
                            output[4] = amp.Run(new int[] {e, output[3]}, true);
                            if (output[4] > thrusterOutput)
                            {
                                thrusterOutput = output[4];
                            }
                        }
                    }
                }
            }
            System.Console.WriteLine($"1. {thrusterOutput}");

            // Part 2
            thrusterOutput = 0;
            int intermediateResult = 0;
            int[] phase, bestPhase = null;
            for (int a = 5; a < 10; a++)
            {
                for (int b = 5; b < 10; b++)
                {
                    if (b == a) continue;
                    for (int c = 5; c < 10; c++)
                    {
                        if (c == a || c == b) continue;
                        for (int d = 5; d < 10; d++)
                        {
                            if (d == a || d == b || d == c) continue;
                            int e = 35 - a - b - c - d; // No loop needed!

                            // Starting the machines also resets their state
                            phase = new int[] { a, b, c, d, e };
                            amplifier[0].Start(new int[] {phase[0], 0}, out output[0]);
                            amplifier[1].Start(new int[] {phase[1], output[0]}, out output[1]);
                            amplifier[2].Start(new int[] {phase[2], output[1]}, out output[2]);
                            amplifier[3].Start(new int[] {phase[3], output[2]}, out output[3]);
                            if (amplifier[4].Start(new int[] {phase[4], output[3]}, out output[4]))
                            {
                                intermediateResult = output[4];
                            }
                            while (amplifier[0].active)
                            {
                                amplifier[0].Next(new int[] {output[4]}, out output[0]);
                                amplifier[1].Next(new int[] {output[0]}, out output[1]);
                                amplifier[2].Next(new int[] {output[1]}, out output[2]);
                                amplifier[3].Next(new int[] {output[2]}, out output[3]);

                                // output is always overwritten, even if the machine has no output
                                // Next() return true iff the machine actually has output (and the
                                // value is meaningful)
                                if (amplifier[4].Next(new int[] {output[3]}, out output[4]))
                                {
                                    intermediateResult = output[4];
                                }
                            }

                            if (intermediateResult > thrusterOutput)
                            {
                                thrusterOutput = intermediateResult;
                                bestPhase = phase;
                            }
                        }
                    }
                }
            }
            System.Console.WriteLine($"2. {thrusterOutput} [{bestPhase[0]},{bestPhase[1]},{bestPhase[2]},{bestPhase[3]},{bestPhase[4]}]");
        }
    }

    class IntCodeMachine
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

        public string name { get; }     // Identifier for the machine instance
        private int[] data;             // Initial state, keep to reset
        private int[] code;             // Current state
        private int idx;                // Execution pointer
        
        public bool active { get; private set; }   // Is the machine currently running

        public IntCodeMachine (string name, int[] data)
        {
            this.name = name;
            this.data = new int[data.Length];
            this.code = new int[data.Length];
            for (int i = 0; i < data.Length; i++) this.data[i] = data[i];
        }

        public int Run (int[] input, bool force = false)
        {
            if (active && !force) throw new Exception($"Machine [{name}] already active");

            int output;
            Start(input, out output);   // Assume the machine has no intermediate output!
            return output;
        }

        public void Reset()
        {
            for (int i = 0; i < data.Length; i++) code[i] = data[i];
            idx = 0;
            active = true;
        }

        public void Stop() =>  active = false;

        public bool Start (int[] input, out int output)
        {
            Reset();
            return Next(input, out output);
        }

        public bool Next(int[] inputs, out int output)
        {
            // Machine must have been initialised
            if (!active) throw new Exception("Machine not active");
            
            int in_ptr = 0; // current position in the input array

            // System.Console.WriteLine($"[{name}][IN] idx = {idx}");

            // Guarantee that we have output
            output = Int32.MaxValue;

            bool running = true;
            int a, b, c;
            while (running)
            {
                int cmd = code[idx++];
                int opcode = cmd % 100;
                int mode1 = (cmd / 100) % 10;
                int mode2 = (cmd / 1000) % 10;

                switch (opcode)
                {
                    case OP_ADD:
                        // Console.WriteLine("OP_ADD");
                        a = (mode1 == MOD_POS) ? code[code[idx++]] : code[idx++];
                        b = (mode2 == MOD_POS) ? code[code[idx++]] : code[idx++];
                        c = code[idx++];    // Write address: never in immediate mode
                        code[c] = a + b;
                        break;
                    case OP_MUL:
                        // Console.WriteLine("OP_MUL");
                        a = (mode1 == MOD_POS) ? code[code[idx++]] : code[idx++];
                        b = (mode2 == MOD_POS) ? code[code[idx++]] : code[idx++];
                        c = code[idx++];    // Write address: never in immediate mode
                        code[c] = a * b;
                        break;
                    case OP_IN:
                        // Console.WriteLine("OP_IN");
                        a = inputs[in_ptr++];   // Assume enough inputs
                        b = code[idx++];    // Write address: never in immediate mode
                        code[b] = a;
                        break;
                    case OP_OUT:
                        // Console.WriteLine("OP_OUT");
                        output = (mode1 == MOD_POS) ? code[code[idx++]] : code[idx++];
                        // System.Console.WriteLine($"[{name}][OUT] idx = {idx}, val = {output}");
                        return true;
                    case OP_JMP_T:
                        // Console.WriteLine("OP_JMP_T");
                        a = (mode1 == MOD_POS) ? code[code[idx++]] : code[idx++];
                        b = (mode2 == MOD_POS) ? code[code[idx++]] : code[idx++];
                        if (a != 0)
                        {
                            idx = b;
                            // Console.WriteLine($"Jump on True to: {idx}");
                        }
                        break;
                    case OP_JMP_F:
                        // Console.WriteLine("OP_JMP_F");
                        a = (mode1 == MOD_POS) ? code[code[idx++]] : code[idx++];
                        b = (mode2 == MOD_POS) ? code[code[idx++]] : code[idx++];
                        if (a == 0)
                        {
                            idx = b;
                            // Console.WriteLine($"Jump on False to: {idx}");
                        }
                        break;
                    case OP_LT:
                        // Console.WriteLine("OP_LT");
                        a = (mode1 == MOD_POS) ? code[code[idx++]] : code[idx++];
                        b = (mode2 == MOD_POS) ? code[code[idx++]] : code[idx++];
                        c = code[idx++];    // Write address: never in immediate mode
                        code[c] = (a < b) ? 1 : 0;
                        break;
                    case OP_EQ:
                        // Console.WriteLine("OP_EQ");
                        a = (mode1 == MOD_POS) ? code[code[idx++]] : code[idx++];
                        b = (mode2 == MOD_POS) ? code[code[idx++]] : code[idx++];
                        c = code[idx++];    // Write address: never in immediate mode
                        code[c] = (a == b) ? 1 : 0;
                        break;
                    case OP_END:
                        // Console.WriteLine("OP_END");
                        // System.Console.WriteLine($"[{name}][END] idx = {idx}");
                        active = false;
                        return false;
                    default:
                        active = false;
                        throw new Exception($"Unknown OpCode: {opcode}");
                }
            }

            // End of run, but no output set (apparently)
            active = false;
            return false;
        }
    }
}