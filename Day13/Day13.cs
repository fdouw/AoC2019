using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace AoC2019
{
    public enum Tile
    {
        Empty = 0,
        Wall = 1,
        Block = 2,
        Paddle = 3,
        Ball = 4
    }

    class Day13
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Day 13");

            string filename = "input";
            int[] data;
            using (StreamReader sr = new StreamReader(filename))
            {
                data = sr.ReadLine().Trim().Split(",").Select(s => Int32.Parse(s)).ToArray();
            }

            IntCodeMachine icm = new IntCodeMachine("arcade", data);
            Dictionary<(int x,int y),Tile> canvas = new Dictionary<(int, int), Tile>();
            int[] EMPTY = new int[]{0};
            
            // Part 1
            icm.Reset();
            while (icm.active)
            {
                int x, y, id;
                if (icm.Next(EMPTY, out x) && icm.Next(EMPTY, out y) && icm.Next(EMPTY, out id))
                {
                    canvas[(x,y)] = (Tile)id;
                }
            }
            System.Console.WriteLine($"1. {canvas.Where(x => x.Value == Tile.Block).Count()}");

            // Part 2
            data[0] = 2;    // Insert quarters
            icm = new IntCodeMachine("arcade", data);
            icm.Reset();
            canvas.Clear();
            int joystick = 0; // neutral by default
            while (icm.active)
            {
                int x, y, id;
                if (icm.Next(new int[]{joystick}, out x) && icm.Next(new int[]{0}, out y) && icm.Next(new int[]{0}, out id))
                {
                    if (x == -1 && y == 0)
                    {
                        DrawCanvas(true, id);
                    }
                    else {
                        canvas[(x,y)] = (Tile)id;
                        DrawCanvas(false);
                    }
                }
            }

            int DrawCanvas (bool draw, int id = 0)
            {
                // Note the x-position of ball and paddle to guide the paddle
                int ballX = -1, paddleX = -1;
                
                // Determine the edges of the board
                int hix = Int32.MinValue;
                int hiy = Int32.MinValue;
                int lox = Int32.MaxValue;
                int loy = Int32.MaxValue;
                foreach(var item in canvas)
                {
                    if (item.Key.x > hix) hix = item.Key.x;
                    if (item.Key.x < lox) lox = item.Key.x;
                    if (item.Key.y > hiy) hiy = item.Key.y;
                    if (item.Key.y < loy) loy = item.Key.y;
                }
                // Draw the board, with the score on bottom
                if (draw) System.Console.Clear();
                for (int a = loy; a <= hiy; a++) {
                    string line = "";
                    for (int b = lox; b <= hix; b++) {
                        switch(canvas.ContainsKey((b,a)) ? canvas[(b,a)] : Tile.Empty)
                        { 
                            case Tile.Empty:
                                if (draw) line += " ";
                                break;
                            case Tile.Wall:
                                if (draw) line += "\u2588";
                                break;
                            case Tile.Block:
                                if (draw) line += "#";
                                break;
                            case Tile.Paddle:
                                if (draw) line += "-";
                                paddleX = b;
                                break;
                            case Tile.Ball:
                                if (draw) line += "O";
                                ballX = b;
                                break;
                        }                               
                    }
                    if (draw) System.Console.WriteLine(line);
                }
                // Adjust the joystick
                if (paddleX < ballX) joystick = 1;
                if (paddleX > ballX) joystick = -1;
                if (paddleX == ballX) joystick = 0;
                if (draw)
                {
                    System.Console.WriteLine($"{id} || {joystick}");
                    Thread.Sleep(100);
                }
                return joystick;
            }
        }
    }

    class IntCodeTape
    {
        private int[] initialCode;
        Dictionary<int,int> code;

        public IntCodeTape(int[] data)
        {
            initialCode = new int[data.Length];
            code = new Dictionary<int, int>(data.Length);
            Array.Copy(data, initialCode, data.Length);
            Reset();
        }

        public int this[int index] {
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
        private int idx;               // Execution pointer
        private int relBase;           // Relative Base, used for relative addressing
        
        public bool active { get; private set; }   // Is the machine currently running

        public IntCodeMachine (string name, int[] data)
        {
            this.name = name;
            this.code = new IntCodeTape(data);
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
            code.Reset();
            idx = 0;
            relBase = 0;
            active = true;
        }

        public void Stop() =>  active = false;

        public bool Start (int[] input, out int output)
        {
            Reset();
            return Next(input, out output);
        }

        private int Read(int index, int mode)
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

        private void Write(int index, int mode, int value)
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

        private void WriteAction (int line, string msg)
        {
            System.Console.WriteLine($"[{line,2}] {msg}");
        }

        public bool Next(int[] inputs, out int output)
        {
            // Machine must have been initialised
            if (!active) throw new Exception("Machine not active");
            
            int in_ptr = 0;     // current position in the input array

            // System.Console.WriteLine($"[{name}][IN] idx = {idx}");

            // Guarantee that we have output
            output = Int32.MaxValue;

            bool running = true;
            int a, b;
            while (running)
            {
                int cmd = code[idx++];
                int opcode = cmd % 100;
                int mode1 = (cmd / 100) % 10;
                int mode2 = (cmd / 1000) % 10;
                int mode3 = (cmd / 10000) % 10;

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