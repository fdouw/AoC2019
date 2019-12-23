using System;
using System.Collections.Generic;
using System.Linq;

public delegate long IcmInputReader ();
public delegate void IcmOutputWriter (long output);

namespace AoC2019
{
    class IntCodeMachine
    {
        public const long OP_ADD = 1;    // Add
        public const long OP_MUL = 2;    // Multiply
        public const long OP_IN = 3;     // Input value
        public const long OP_OUT = 4;    // Output value
        public const long OP_JMP_T = 5;  // Jump if true
        public const long OP_JMP_F = 6;  // Jump if false
        public const long OP_LT = 7;     // Less than
        public const long OP_EQ = 8;     // Equals
        public const long OP_SRB = 9;    // Set Relative Base
        public const long OP_END = 99;

        public string name { get; }     // Identifier for the machine instance
        private IntCodeTape code;       // Current state
        private long idx;                // Execution pointer
        
        public bool active { get; private set; }   // Is the machine currently running

        public event IcmInputReader Input;
        public event IcmOutputWriter Output;

        // For backward compatability (to make Next() work)
        private long[] inputData = new long[0];
        private long inputPointer = 0;
        private long internalInput () => (inputPointer < inputData.Length) ? inputData[inputPointer++] : -1;
        private long outputData;
        private void internalOutput (long data)
        {
            outputData = data;
        }

        public IntCodeMachine (string name, long[] data)
        {
            this.name = name;
            this.code = new IntCodeTape(data);
        }

        public void Reset()
        {
            idx = 0;
            active = true;
        }

        public void SetRegister(long address, long value)
        {
            // Machine must have been initialised
            if (!active) throw new Exception("Machine not active");
            code[address] = value;
        }

        public void Stop() =>  active = false;

        public bool Start (long[] input, out long output)
        {
            Reset();
            return Next(input, out output);
        }

        private void WriteAction (long line, string msg)
        {
            System.Console.WriteLine($"[{line,2}] {msg}");
        }

        public bool Run(bool returnOnOutput = false)
        {
            // Machine must have been initialised
            if (!active) throw new Exception("Machine not active");

            bool running = true;
            long a, b, output;
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

        public bool Next(long input, out long output) => Next(new long[]{input}, out output);

        public bool Next(long[] inputs, out long output)
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
        public const long MOD_POS = 0;
        public const long MOD_VAL = 1;
        public const long MOD_REL = 2;   // Relative Base for addressing

        private long[] initialCode;
        private Dictionary<long,long> code;

        private long relBase;

        internal IntCodeTape(long[] data)
        {
            initialCode = new long[data.Length];
            code = new Dictionary<long, long>(data.Length);
            Array.Copy(data, initialCode, data.Length);
            Reset();
        }

        internal long this[long index] {
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
            for (long i = 0; i < initialCode.Length; i++) code.Add(i, initialCode[i]);
            relBase = 0;
        }

        internal void AddRelativeBase (long adjust)
        {
            relBase += adjust;
        }

        internal long Read(long index, long mode)
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

        internal void Write(long index, long mode, long value)
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

    class AsciiIcm
    {
        private IntCodeMachine icm;
        private string ascii;

        private long[] input;
        private long inPtr;

        private List<long> output = new List<long>();

        public AsciiIcm (long[] code, string ascii = "")
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
            input = ascii.ToCharArray().Select(c => (long)c).ToArray();
            if (insertCommas)
            {
                List<long> tmp = new List<long>();
                foreach (long character in input)
                {
                    tmp.Add(character);
                    tmp.Add((long)',');
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

        public IEnumerator<long> GetOutput () => output.GetEnumerator();
        public IEnumerable<char> GetAsciiOutput () => output.Select(n => (char)n);
        public string GetOutputString () => String.Concat(output.Select(n => (n < 256) ? $"{(char)n}" : $"{n}"));

        public long PeekOutput() => output.First();

        public long SendInput()
        {
            if (inPtr >= input.Length)
                throw new Exception("No input available.");
            return input[inPtr++];
        }

        public void ReceiveOutput(long data)
        {
            output.Add(data);
        }
    }
}