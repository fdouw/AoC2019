using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        private const double Offset = 3 * Math.PI / 2;
        public double Angle { get => Math.PI / 2 + Math.Atan2(dy, dx); }
        public int Length2 { get => (dx * dx + dy * dy); }

        public LineSegment(Point a, Point b)
        {
            this.A = a;
            this.B = b;
        }
    }

    class Day10
    {
        static void Main (string[] args)
        {
            System.Console.WriteLine("Day 10");

            // Read the data first
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
                        }
                    }
                    y++;
                }
            }

            // Part 1
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

            // Part 2
            List<LineSegment> lines = new List<LineSegment>(currentMax);
            foreach (Point q in astroids)
            {
                lines.Add(new LineSegment(currentBest, q));
            }
            LineSegment[] lineArray = lines.OrderBy(p => p.Angle).ThenBy(p => p.Length2).ToArray();

            double currentAngle = -1;
            int count = 0;
            int i = 0;
            HashSet<int> vaporised = new HashSet<int>(200);
            while(lineArray[i].Angle < 0) i++;  // 
            i--;
            while (true)
            {
                i++;
                if (vaporised.Contains(i)) continue;
                LineSegment ls = lineArray[i % lineArray.Length];
                if (ls.Angle == currentAngle) continue;
                System.Console.WriteLine($"VAP {i,3}: ({ls.B.X,2},{ls.B.Y,2}), angle: {ls.Angle}, len2: {ls.Length2}");
                currentAngle = ls.Angle;
                vaporised.Add(i);
                count++;
                if (count == 200)
                {
                    System.Console.WriteLine($"2. ({ls.B.X},{ls.B.Y}) => {100 * ls.B.X + ls.B.Y}");
                    break;
                }
            }
        }
    }
}