using System;
using System.Collections.Generic;
using System.IO;

namespace AoC2019
{
    class Point
    {
        public int X { get; }
        public int Y { get; }
        
        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    class LineSegment
    {
        public Point A { get; }
        public Point B { get; }
        public int dx { get => B.X - A.X; }
        public int dy { get => B.Y - A.Y; }
        public double Angle { get => Math.Atan2(dy, dx); }
        public double Length { get => Math.Sqrt(dx * dx + dy * dy); }
        public decimal Slope { 
            get
            {
                if (dx == 0) return (dy < 0) ? Decimal.MinValue : Decimal.MaxValue;
                return Decimal.Divide(dy, dx);
            }
        }

        public LineSegment(Point a, Point b)
        {
            this.A = a;
            this.B = b;
        }

        public bool IsAligned(LineSegment that) => this.Angle == that.Angle;
    }

    class Day10
    {
        static void Main (string[] args)
        {
            System.Console.WriteLine("Day 10");

            // Part 1
            string filename = (args.Length == 1) ? args[0] : "input";
            List<Point> astroids = new List<Point>();
            using (StreamReader sr = new StreamReader(filename))
            {
                int y = 0;
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    for (int x = 0; x < line.Length; x++)
                    {
                        if (line[x] == '#')
                        {
                            astroids.Add(new Point(x, y));
                            // System.Console.WriteLine($"({x},{y})");
                        }
                    }
                    y++;
                }
            }

            int currentMax = 0;
            Point currentBest = null;
            foreach (Point p in astroids)
            {
                HashSet<double> angles = new HashSet<double>();
                foreach (Point q in astroids)
                {
                     angles.Add(new LineSegment(p, q).Angle);
                }
                if (angles.Count > currentMax)
                {
                    currentMax = angles.Count;
                    currentBest = p;
                }
            }
            System.Console.WriteLine($"1. {currentMax}");
            System.Console.WriteLine($"({currentBest.X},{currentBest.Y})");
        }
    }
}