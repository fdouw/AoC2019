using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace AoC2019
{
    public enum Tile
    {
        Wall = 0,
        Empty = 1,
        System = 2
    }

    class Day15
    {        
        static void Main(string[] args)
        {
            System.Console.WriteLine("Day 15");

            string filename = "input";
            IntCodeMachine icm;
            using (StreamReader sr = new StreamReader(filename))
            {
                int[] data = sr.ReadLine().Trim().Split(",").Select(s => Int32.Parse(s)).ToArray();
                icm = new IntCodeMachine("arcade", data);
            }
            
            // Part 1
            Room currentRoom = Room.CreateOrigin();
            icm.Reset();
            while (icm.active)
            {
                System.Console.WriteLine($"{currentRoom.ToString()}");
                Room nextRoom = currentRoom.Explore(icm);
                if (nextRoom == null)
                {
                    // We hit a dead end
                    System.Console.WriteLine($"No more rooms to explore!");
                    break;
                }
                if (nextRoom.IsSystem)
                {
                    System.Console.WriteLine($"1. {nextRoom.DistanceToStart}");
                    return;
                }
                currentRoom = nextRoom;
            }
        }
    }

    class Room
    {
        private static int[] DX = new int[] {0,0,1,-1};  // North, South, West, East
        private static int[] DY = new int[] {1,-1,0,0};  // North, South, West, East
        private static int[] opposite = new int[] {1,0,3,2};

        private static Dictionary<(int x, int y), Room> pool = new Dictionary<(int x, int y), Room>();
        private static Dictionary<(int x, int y), int> area = new Dictionary<(int x, int y), int>();

        Room[] neighbour = new Room[4];   // North, South, West, East
        public Room Previous { get; private set; }
        public int PreviousDir { get; private set; }
        public int X { get; }
        public int Y { get; }
        public int DistanceToStart { get => Previous?.DistanceToStart + 1 ?? 0; }
        private int exploreDir = 0;

        public bool IsSystem { get; }

        public static Room CreateRoom (Room prev, int dir, bool system = false)
        {
            if (prev == null)
                throw new ArgumentNullException("prev", "Could not create room from empty source.");
            if (dir < 0 || dir > 3)
                throw new ArgumentOutOfRangeException("dir", "Direction must be within [0,3].");
            
            int x = prev.X + DX[dir];
            int y = prev.Y + DY[dir];
            int d = prev.DistanceToStart + 1;
            int fromDir = opposite[dir];
            
            if (pool.ContainsKey((x,y)))
            {
                System.Console.WriteLine("Returning existing room!");
                // Return existing
                if (pool[(x,y)].DistanceToStart > d)
                {
                    pool[(x,y)].SetPrevious(prev, fromDir);
                }
                return pool[(x,y)];
            }
            else
            {
                return new Room(x, y, prev, system);
            }
        }

        public static Room CreateOrigin ()
        {
            return new Room(0, 0);
        }

        private Room (int x, int y, Room prev = null, bool system = false)
        {
            this.X = x;
            this.Y = y;
            this.IsSystem = system;
            Previous = prev;
            if (Previous != null)
            {
                if (prev.X > this.X) PreviousDir = 3;
                else if (prev.X < this.X) PreviousDir = 2;
                else if (prev.Y > this.Y) PreviousDir = 1;
                else if (prev.Y < this.Y) PreviousDir = 0;
            }
            pool.Add((x, y), this);
            area.Add((x, y), system ? 2 : 1);
        }

        void SetPrevious(Room prev, int fromDir)
        {
            this.neighbour[fromDir] = prev;
            this.Previous = prev;
            this.PreviousDir = fromDir;
        }

        public Room Explore (IntCodeMachine icm)
        {
            for (int d = exploreDir; d < 4; d++)
            {
                if (neighbour[d] == null)
                {
                    int response;
                    icm.Next(d+1, out response);
                    if (response == 0)
                    {
                        // Hit a wall, keep looking
                        //area[(X + DX[d], Y + DY[d])] = 0;
                        continue;
                    }
                    if (response == 1 || response == 2)
                    {
                        if (PoolContains(this, d))
                        {
                            // Do not venture into this room: make sure the icm returns too
                            icm.Next(opposite[d] + 1, out _);
                            continue;
                        }
                        else
                        {
                            // Move to new room
                            neighbour[d] = CreateRoom(this, d, (response == 2));
                            exploreDir = d + 1;
                            return neighbour[d];
                        }
                    }
                    // Should not get here...
                    throw new Exception($"Unknown reply from robot: {response}, in room: ({X},{Y}).");
                }
            }

            // Did not find a next room: backtrack
            exploreDir = 4;
            icm.Next(PreviousDir + 1, out _);   // Make sure the icm also returns
            return Previous;
        }

        public Room NextRoom (IntCodeMachine icm, bool backtrack = false)
        {
            for (int d = 0; d < 4; d++)
            {
                if (neighbour[d] == null)
                {
                    int response;
                    icm.Next(d+1, out response);
                    if (response == 0)
                    {
                        // Hit a wall, keep looking
                        continue;
                    }
                    if (response == 1 || response == 2)
                    {
                        // Move to new room
                        neighbour[d] = CreateRoom(this, d, (response == 2));
                        return neighbour[d];
                    }
                    // Should not get here...
                    throw new Exception($"Unknown reply from robot: {response}, in room: ({X},{Y}).");
                }
                else if (backtrack)
                {
                    int response;
                    icm.Next(d+1, out response);
                    if (response == 0)
                    {
                        // Hit a wall, keep looking
                        continue;
                    }
                    if (response == 1 || response == 2)
                    {
                        // Move to empty room
                        return neighbour[d];
                    }
                    // Should not get here...
                    throw new Exception($"Unknown reply from robot: {response}, in room: ({X},{Y}).");
                }
            }
            // Did not move
            return this;
        }

        public override string ToString () => $"({X}, {Y}) (dist: {DistanceToStart})";

        public static bool PoolContains(Room room) => pool.ContainsValue(room);
        public static bool PoolContains(Room room, int dir) => pool.ContainsKey((room.X + DX[dir], room.Y + DY[dir]));
        public static bool PoolContains((int, int) coord) => pool.ContainsKey(coord);
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
        private int idx;                // Execution pointer
        private int relBase;            // Relative Base, used for relative addressing
        
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

        public bool Next(int input, out int output) => Next(new int[]{input}, out output);

        public bool Next(int[] inputs, out int output)
        {
            // Machine must have been initialised
            if (!active) throw new Exception("Machine not active");
            
            int in_ptr = 0;     // current position in the input array

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
                        a = Read(idx++, mode1);
                        b = Read(idx++, mode2);
                        Write(idx++, mode3, a + b);
                        break;
                    case OP_MUL:
                        a = Read(idx++, mode1);
                        b = Read(idx++, mode2);
                        Write(idx++, mode3, a * b);
                        break;
                    case OP_IN:
                        a = inputs[in_ptr++];   // Assume enough inputs
                        Write(idx++, mode1, a);
                        break;
                    case OP_OUT:
                        output = Read(idx++, mode1);
                        return true;
                    case OP_JMP_T:
                        a = Read(idx++, mode1);
                        b = Read(idx++, mode2);
                        if (a != 0)
                        {
                            idx = b;
                        }
                        break;
                    case OP_JMP_F:
                        a = Read(idx++, mode1);
                        b = Read(idx++, mode2);
                        if (a == 0)
                        {
                            idx = b;
                        }
                        break;
                    case OP_LT:
                        a = Read(idx++, mode1);
                        b = Read(idx++, mode2);
                        Write(idx++, mode3, (a < b) ? 1 : 0);
                        break;
                    case OP_EQ:
                        a = Read(idx++, mode1);
                        b = Read(idx++, mode2);
                        Write(idx++, mode3,(a == b) ? 1 : 0);
                        break;
                    case OP_SRB:
                        a = Read(idx++, mode1);
                        relBase += a;
                        break;
                    case OP_END:
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