using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC2019
{
    class Day11
    {
        static void Main (string[] args)
        {
            System.Console.WriteLine("Day 11");

            // Read data
            string filename = "input";
            long[] data;
            using (StreamReader sr = new StreamReader(filename))
            {
                data = sr.ReadLine().Trim().Split(",").Select(s => Int64.Parse(s)).ToArray();
            }

            // Part 1
            int X = 0;
            int Y = 0;
            int dir = 0;
            HashSet<(int,int)> painted = new HashSet<(int,int)>();
            HashSet<(int,int)> white = new HashSet<(int,int)>();
            IntCodeMachine icm = new IntCodeMachine("robot", data);
            long colour, deltaDir;
            long[] input;
            long[] WHITE = new long[]{1};
            long[] BLACK = new long[]{0};
            long[] EMPTY = new long[0];
            icm.Reset();
            while (icm.active)
            {
                input = (white.Contains((X,Y))) ? WHITE : BLACK;
                if (icm.Next(input, out colour) && icm.Next(EMPTY, out deltaDir))
                {
                    // Paint black or white
                    if (colour == 0) white.Remove((X,Y));
                    else white.Add((X,Y));
                    painted.Add((X,Y));

                    dir = (deltaDir == 0) ? dir - 1 : dir + 1;
                    dir = (dir + 4) % 4;    // To keep positive, and in range

                    // Y is up
                    if (dir == 0) Y++;
                    else if (dir == 1) X++;
                    else if (dir == 2) Y--;
                    else if (dir == 3) X--;
                }
            }
            System.Console.WriteLine($"1. {painted.Count}");

            // Part 2
            white.Clear();
            white.Add((0,0));
            X = Y = 0;
            dir = 0;
            icm.Reset();
            while (icm.active)
            {
                input = (white.Contains((X,Y))) ? WHITE : BLACK;
                if (icm.Next(input, out colour) && icm.Next(EMPTY, out deltaDir))
                {
                    // Paint black or white
                    if (colour == 0) white.Remove((X,Y));
                    else white.Add((X,Y));
                    painted.Add((X,Y));

                    dir = (deltaDir == 0) ? dir - 1 : dir + 1;
                    dir = (dir + 4) % 4;    // To keep positive, and in range

                    // Y is up
                    if (dir == 0) Y++;
                    else if (dir == 1) X++;
                    else if (dir == 2) Y--;
                    else if (dir == 3) X--;
                }
            }

            // Find the canvas size and boundaries
            int minX = Int32.MaxValue;
            int maxX = Int32.MinValue;
            int minY = Int32.MaxValue;
            int maxY = Int32.MinValue;
            foreach ((int x,int y) in white)
            {
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (Y > maxY) maxY = y;
            }
            System.Console.WriteLine("2. Registration Identifier:");
            for (int y = maxY; y >= minY; y--)  // y goes down for printing
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (white.Contains((x,y))) System.Console.Write("\u2588");
                    else System.Console.Write(" ");
                }
                System.Console.WriteLine("");
            }
        }
    }

    class IntCodeTape
    {
        private long[] initialCode;
        Dictionary<long,long> code;

        public IntCodeTape(long[] data)
        {
            initialCode = new long[data.Length];
            code = new Dictionary<long, long>(data.Length);
            Array.Copy(data, initialCode, data.Length);
            Reset();
        }

        public long this[long index] {
            get
            { 
                return (code.ContainsKey(index)) ? code[index] : 0;
            }
            set
            {
                code[index] = value;
            }
        }

        public void Reset()
        {
            code.Clear();
            for (int i = 0; i < initialCode.Length; i++) code.Add(i, initialCode[i]);
        }
    }
    
    class IntCodeMachine
    {
        public const int OP_ADD = 1;    // Add
        public const int OP_MUL = 2;    // Multiply
        public const int OP_IN = 3;     // Input value
        public const int OP_OUT = 4;    // Output value
        public const int OP_JMP_T = 5;  // Jump if true
        public const int OP_JMP_F = 6;  // Jump if false
        public const int OP_LT = 7;     // Less than
        public const int OP_EQ = 8;     // Equals
        public const int OP_SRB = 9;    // Set Relative Base
        public const int OP_END = 99;

        public const int MOD_POS = 0;
        public const int MOD_VAL = 1;
        public const int MOD_REL = 2;   // Relative Base for addressing

        public string name { get; }     // Identifier for the machine instance
        private IntCodeTape code;       // Current state
        private long idx;               // Execution pointer
        private long relBase;           // Relative Base, used for relative addressing
        
        public bool active { get; private set; }   // Is the machine currently running

        public IntCodeMachine (string name, long[] data)
        {
            this.name = name;
            this.code = new IntCodeTape(data);
        }

        public long Run (long[] input, bool force = false)
        {
            if (active && !force) throw new Exception($"Machine [{name}] already active");

            long output;
            Start(input, out output);   // Assume the machine has no intermediate output!
            return output;
        }

        public void Reset()
        {
            code.Reset();
            idx = 0;
            relBase = 0;
            active = true;
        }

        public void Stop() =>  active = false;

        public bool Start (long[] input, out long output)
        {
            Reset();
            return Next(input, out output);
        }

        private long Read(long index, long mode)
        {
            switch (mode)
            {
                case MOD_POS:
                    return code[code[index]];
                case MOD_VAL:
                    return code[index];
                case MOD_REL:
                    return code[relBase + code[index]];
                default:
                    throw new Exception($"Unknown mode: {mode}");
            }
        }

        private void Write(long index, long mode, long value)
        {
            switch (mode)
            {
                case MOD_POS:
                    code[code[index]] = value;
                    break;
                case MOD_VAL:
                    throw new Exception("MOD_VAL is invalid for writing");
                case MOD_REL:
                    code[relBase + code[index]] = value;
                    break;
                default:
                    throw new Exception($"Unknown mode: {mode}");
            }
        }

        private void WriteAction (long line, string msg)
        {
            System.Console.WriteLine($"[{line,2}] {msg}");
        }

        public bool Next(long[] inputs, out long output)
        {
            // Machine must have been initialised
            if (!active) throw new Exception("Machine not active");
            
            int in_ptr = 0;     // current position in the input array

            // System.Console.WriteLine($"[{name}][IN] idx = {idx}");

            // Guarantee that we have output
            output = Int64.MaxValue;

            bool running = true;
            long a, b;
            while (running)
            {
                long cmd = code[idx++];
                long opcode = cmd % 100;
                long mode1 = (cmd / 100) % 10;
                long mode2 = (cmd / 1000) % 10;
                long mode3 = (cmd / 10000) % 10;

                switch (opcode)
                {
                    case OP_ADD:
                        // Console.WriteLine("OP_ADD");
                        a = Read(idx++, mode1);
                        b = Read(idx++, mode2);
                        Write(idx++, mode3, a + b);
                        // WriteAction(idx - 4, $"[{cmd,4}] ADD {a} {b} to {c}");
                        break;
                    case OP_MUL:
                        // Console.WriteLine("OP_MUL");
                        a = Read(idx++, mode1);
                        b = Read(idx++, mode2);
                        Write(idx++, mode3, a * b);
                        // WriteAction(idx - 4, $"[{cmd,4}] MUL {a} {b} to {c}");
                        break;
                    case OP_IN:
                        // Console.WriteLine("OP_IN");
                        a = inputs[in_ptr++];   // Assume enough inputs
                        Write(idx++, mode1, a);
                        // WriteAction(idx - 3, $"[{cmd,4}] IN {a} to {b}");
                        break;
                    case OP_OUT:
                        // Console.WriteLine("OP_OUT");
                        output = Read(idx++, mode1);
                        // WriteAction(idx - 2, $"[{cmd,4}] OUT {output}");
                        return true;
                    case OP_JMP_T:
                        // Console.WriteLine("OP_JMP_T");
                        a = Read(idx++, mode1);
                        b = Read(idx++, mode2);
                        if (a != 0)
                        {
                            idx = b;
                        }
                        // WriteAction(idx - 3, $"[{cmd,4}] JMP TRUE {a} to {b}");
                        break;
                    case OP_JMP_F:
                        // Console.WriteLine("OP_JMP_F");
                        a = Read(idx++, mode1);
                        b = Read(idx++, mode2);
                        if (a == 0)
                        {
                            idx = b;
                        }
                        // WriteAction(idx - 3, $"[{cmd,4}] JMP FALSE {a} to {b}");
                        break;
                    case OP_LT:
                        // Console.WriteLine("OP_LT");
                        a = Read(idx++, mode1);
                        b = Read(idx++, mode2);
                        Write(idx++, mode3, (a < b) ? 1 : 0);
                        // WriteAction(idx - 4, $"[{cmd,4}] LT {a} {b} to {c}");
                        break;
                    case OP_EQ:
                        // Console.WriteLine("OP_EQ");
                        a = Read(idx++, mode1);
                        b = Read(idx++, mode2);
                        Write(idx++, mode3,(a == b) ? 1 : 0);
                        // WriteAction(idx - 4, $"[{cmd,4}] EQ {a} {b} to {c}");
                        break;
                    case OP_SRB:
                        // Console.WriteLine("OP_SRB");
                        a = Read(idx++, mode1);
                        relBase += a;
                        // WriteAction(idx - 2, $"[{cmd,4}] RELBASE {a} from {relBase - a} to {relBase}");
                        break;
                    case OP_END:
                        // Console.WriteLine("OP_END");
                        // WriteAction($"[{name}][END] idx = {idx}");
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