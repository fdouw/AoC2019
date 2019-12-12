using System;
using System.Collections.Generic;
using System.Linq;

namespace AoC2019
{
    class Day12
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Day 12");

            // Part 1
            Moon[] moon = new Moon[] { new Moon(15, -2, -6),
                                        new Moon(-5, -4, -11),
                                        new Moon(0, -6, 0),
                                        new Moon(5, 9, 6) };
            
            for (int t = 0; t < 1000; t++)
            {
                for (int m = 0; m < moon.Length; m++)
                {
                    for (int n = 0; n < moon.Length; n++)
                    {
                        moon[m].Gravitate(moon[n]);
                    }
                }
                foreach(Moon m in moon) m.Move();
            }
            int E = 0;
            foreach (Moon m in moon) E += m.GetTotalEnergy();

            System.Console.WriteLine($"1. {E}");

            // Part 2            
            // int[] X = new int[] { -1, 2, 4, 3 };
            // int[] Y = new int[] { 0, -10, -8, 5 };
            // int[] Z = new int[] { 2, -7, 8, -1 };
            int[] X = new int[] { 15, -5, 0, 5 };
            int[] Y = new int[] { -2, -4, -6, 9 };
            int[] Z = new int[] {  -6, -11, 0, 6 };

            long[] period = new long[3];
            period[0] = GetPeriod(X);
            System.Console.WriteLine($"{period[0]}");
            period[1] = GetPeriod(Y);
            System.Console.WriteLine($"{period[1]}");
            period[2] = GetPeriod(Z);
            System.Console.WriteLine($"{period[2]}");

            long lcm1 = lcm(period[0], period[1]);
            long lcm2 = lcm(lcm1, period[2]);
            System.Console.WriteLine($"2. {lcm2}");
            
            long GetPeriod(int[] pos)
            {
                int[] u = new int[4];
                for (int i = 0; i < 4; i++) u[i] = pos[i];

                int[] v = new int[] { 0, 0, 0, 0 };
                for (long t = 0; t < Int64.MaxValue; t++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            if (u[i] < u[j]) v[i]++;
                            if (u[i] > u[j]) v[i]--;
                        }
                    }
                    for (int i = 0; i < 4; i++) u[i] += v[i];
                    bool match = true;
                    for (int i = 0; i < 4; i++)
                    {
                        if (u[i] != pos[i] || v[i] != 0) match = false;
                    }
                    if (match)
                    {
                        return t + 1;
                    }
                }
                return -1;
            }
        }

        static long gcf(long a, long b)
        {
            while (b != 0)
            {
                long temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        static long lcm(long a, long b)
        {
            return (a / gcf(a, b)) * b;
        }
    }

    class Moon
    {
        public int[] pos { get; private set; }
        public int[] v { get; private set; }

        public Moon(int x, int y, int z, int vx = 0, int vy = 0, int vz = 0)
        {
            pos = new int[] {x, y, z};
            v = new int[] {vx, vy, vz};
        }

        public void Gravitate(Moon other)
        {
            // If other == this, we automatically skip, because positions are equal
            for (int i = 0; i < 3; i++)
            {
                if (this.pos[i] < other.pos[i]) v[i]++;
                if (this.pos[i] > other.pos[i]) v[i]--;
            }
        }

        public void Move()
        {
            pos[0] += v[0];
            pos[1] += v[1];
            pos[2] += v[2];
        }

        public int GetTotalEnergy()
        {
            int K = 0;
            int V = 0;
            for (int i = 0; i < 3; i++)
            {
                K += Math.Abs(v[i]);
                V += Math.Abs(pos[i]);
            }
            return K * V;
        }
    }
}