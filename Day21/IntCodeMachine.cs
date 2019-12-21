using System;
using System.Collections.Generic;

public delegate int IcmInputReader ();
public delegate void IcmOutputWriter (int output);

namespace AoC2019
{
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

        public string name { get; }     // Identifier for the machine instance
        private IntCodeTape code;       // Current state
        private int idx;                // Execution pointer
        
        public bool active { get; private set; }   // Is the machine currently running

        public event IcmInputReader Input;
        public event IcmOutputWriter Output;

        // For backward compatability (to make Next() work)
        private int[] inputData = new int[0];
        private int inputPointer = 0;
        private int internalInput () => (inputPointer < inputData.Length) ? inputData[inputPointer++] : -1;
        private int outputData;
        private void internalOutput (int data)
        {
            outputData = data;
        }

        public IntCodeMachine (string name, int[] data)
        {
            this.name = name;
            this.code = new IntCodeTape(data);
        }

        public void Reset()
        {
            idx = 0;
            active = true;
        }

        public void SetRegister(int address, int value)
        {
            // Machine must have been initialised
            if (!active) throw new Exception("Machine not active");
            code[address] = value;
        }

        public void Stop() =>  active = false;

        public bool Start (int[] input, out int output)
        {
            Reset();
            return Next(input, out output);
        }

        private void WriteAction (int line, string msg)
        {
            System.Console.WriteLine($"[{line,2}] {msg}");
        }

        public bool Run(bool returnOnOutput = false)
        {
            // Machine must have been initialised
            if (!active) throw new Exception("Machine not active");

            bool running = true;
            int a, b, output;
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
                        a = code.Read(idx++, mode1);
                        b = code.Read(idx++, mode2);
                        code.Write(idx++, mode3, a + b);
                        break;
                    case OP_MUL:
                        a = code.Read(idx++, mode1);
                        b = code.Read(idx++, mode2);
                        code.Write(idx++, mode3, a * b);
                        break;
                    case OP_IN:
                        a = Input();
                        code.Write(idx++, mode1, a);
                        break;
                    case OP_OUT:
                        output = code.Read(idx++, mode1);
                        Output(output);
                        running = !returnOnOutput;
                        break;
                    case OP_JMP_T:
                        a = code.Read(idx++, mode1);
                        b = code.Read(idx++, mode2);
                        if (a != 0)
                        {
                            idx = b;
                        }
                        break;
                    case OP_JMP_F:
                        a = code.Read(idx++, mode1);
                        b = code.Read(idx++, mode2);
                        if (a == 0)
                        {
                            idx = b;
                        }
                        break;
                    case OP_LT:
                        a = code.Read(idx++, mode1);
                        b = code.Read(idx++, mode2);
                        code.Write(idx++, mode3, (a < b) ? 1 : 0);
                        break;
                    case OP_EQ:
                        a = code.Read(idx++, mode1);
                        b = code.Read(idx++, mode2);
                        code.Write(idx++, mode3,(a == b) ? 1 : 0);
                        break;
                    case OP_SRB:
                        a = code.Read(idx++, mode1);
                        code.AddRelativeBase(a);
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

        public bool Next(int input, out int output) => Next(new int[]{input}, out output);

        public bool Next(int[] inputs, out int output)
        {
            // Set input/output
            inputData = inputs;         // Not copying: not safe!
            inputPointer = 0;
            Input = internalInput;      // Overwrite existing listeners
            Output = internalOutput;

            // Run the machine (but return on output)
            bool response = Run(true);
            output = outputData;

            return response;
        }
    }

    class IntCodeTape
    {
        public const int MOD_POS = 0;
        public const int MOD_VAL = 1;
        public const int MOD_REL = 2;   // Relative Base for addressing

        private int[] initialCode;
        private Dictionary<int,int> code;

        private int relBase;

        internal IntCodeTape(int[] data)
        {
            initialCode = new int[data.Length];
            code = new Dictionary<int, int>(data.Length);
            Array.Copy(data, initialCode, data.Length);
            Reset();
        }

        internal int this[int index] {
            get
            { 
                return (code.ContainsKey(index)) ? code[index] : 0;
            }
            set
            {
                code[index] = value;
            }
        }

        internal void Reset()
        {
            code.Clear();
            for (int i = 0; i < initialCode.Length; i++) code.Add(i, initialCode[i]);
            relBase = 0;
        }

        internal void AddRelativeBase (int adjust)
        {
            relBase += adjust;
        }

        internal int Read(int index, int mode)
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

        internal void Write(int index, int mode, int value)
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
    } 
}