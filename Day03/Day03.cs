using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC2019
{
    class Day03
    {
        class Line
        {
            public int A, B;    // Start and end: x-coords for horizontal lines, y for verticals
            public int H;       // Position: y-coord for horizontal lines, x for verticals
            int D;              // The distance to the start of the line
            int X;              // Starting point of the line (to compute the distance to intersect)
            public Line(int h, int a, int b, int d, int x)
            {
                A = a;
                B = b;
                H = h;
                D = d;
                X = x;
            }
            public bool Intersects (Line that)
            {
                // Assume other line is perpendicular
                return this.A < that.H && that.H < this.B && that.A < this.H && this.H < that.B;
            }
            public int DistanceToIntersection (Line that)
            {
                if (!this.Intersects(that)) return Int32.MaxValue / 4;  // Divide by 4 because we're going to add these

                return D + Math.Abs(X - that.H);
            }
        }

        static void Main (string[] args)
        {
            List<Line> horzA = new List<Line>();
            List<Line> vertA = new List<Line>();
            List<Line> horzB = new List<Line>();
            List<Line> vertB = new List<Line>();

            Console.WriteLine($"Day 03");

            // Read file into lists
            string inputPath = @"input";
            using (StreamReader sr = new StreamReader(inputPath))
            {
                string[] data = sr.ReadLine().Split(',');
                int posX = 0;
                int posY = 0;
                int totalDist = 0;
                for (int i = 0; i < data.Length; i++)
                {
                    int d = Int32.Parse(data[i].Substring(1));
                    switch (data[i][0])
                    {
                        case 'R':
                            horzA.Add(new Line(posY, posX, posX + d, totalDist, posX));
                            posX += d;
                            break;
                        case 'D':
                            vertA.Add(new Line(posX, posY, posY + d, totalDist, posY));
                            posY += d;
                            break;
                        case 'L':
                            horzA.Add(new Line(posY, posX - d, posX, totalDist, posX));
                            posX -= d;
                            break;
                        case 'U':
                            vertA.Add(new Line(posX, posY - d, posY, totalDist, posY));
                            posY -= d;
                            break;
                    }
                    totalDist += d;
                }

                data = sr.ReadLine().Split(',');
                posX = 0;
                posY = 0;
                totalDist = 0;
                for (int i = 0; i < data.Length; i++)
                {
                    int d = Int32.Parse(data[i].Substring(1));
                    switch (data[i][0])
                    {
                        case 'R':
                            horzB.Add(new Line(posY, posX, posX + d, totalDist, posX));
                            posX += d;
                            break;
                        case 'D':
                            vertB.Add(new Line(posX, posY, posY + d, totalDist, posY));
                            posY += d;
                            break;
                        case 'L':
                            horzB.Add(new Line(posY, posX - d, posX, totalDist, posX));
                            posX -= d;
                            break;
                        case 'U':
                            vertB.Add(new Line(posX, posY - d, posY, totalDist, posY));
                            posY -= d;
                            break;
                    }
                    totalDist += d;
                }
            }

            // See which lines intersect
            // Only need to check horizontal against vertical and vice versa
            int dist = Int32.MaxValue;
            foreach (Line hor in horzA)
            {
                dist = vertB.Where(v => v.Intersects(hor)).Select(v => Math.Abs(v.H) + Math.Abs(hor.H)).Append(dist).Min();
            }
            foreach (Line ver in vertA)
            {
                dist = horzB.Where(h => h.Intersects(ver)).Select(h => Math.Abs(h.H) + Math.Abs(ver.H)).Append(dist).Min();
            }
            Console.WriteLine($"1. {dist}");

            // Sort the intersections by distance
            // Only need to check horizontal against vertical and vice versa
            dist = Int32.MaxValue;
            foreach (Line hor in horzA)
            {
                dist = vertB.Select(v => v.DistanceToIntersection(hor) + hor.DistanceToIntersection(v)).Append(dist).Min();
            }
            foreach (Line ver in vertA)
            {
                dist = horzB.Select(h => h.DistanceToIntersection(ver) + ver.DistanceToIntersection(h)).Append(dist).Min();
            }
            Console.WriteLine($"2. {dist}");
        }
    }
}