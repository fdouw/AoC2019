using System;
using System.Collections.Generic;
using System.IO;

namespace AoC2019
{
    class Day06
    {
        class Node
        {
            public string name { get; }
            private Node _parent;
            public Node parent {
                get { return _parent; }
                set
                {
                    if (_parent != null && _parent != value)
                        throw new ArgumentException($"Node[{name}] already has a parent.");
                    _parent = value;
                }
            }
            public bool santaTrace { get; set; }
            public bool youTrace { get; set; }

            public int orbitCount => (parent == null) ? 0 : parent.orbitCount + 1;

            public Node(string name, Node parent)
            {
                this.name = name;
                this.parent = parent;
            }
        }

        static void Main (string[] args)
        {
            string filename = @"input";
            Dictionary<String,Node> planets = new Dictionary<String,Node>();
            using (StreamReader sr = new StreamReader(filename))
            {
                string[] p;
                while ((p = sr.ReadLine()?.Split(')')) != null)
                {
                    if (!planets.ContainsKey(p[0])) planets[p[0]] = new Node(p[0], null);
                    if (!planets.ContainsKey(p[1])) planets[p[1]] = new Node(p[1], planets[p[0]]);
                    else planets[p[1]].parent = planets[p[0]];
                }
            }

            System.Console.WriteLine("Day 06");

            // Part 1: count orbits for all planets
            int orbits = 0;
            foreach (var item in planets)
            {
                orbits += item.Value.orbitCount;
            }
            System.Console.WriteLine($"1. {orbits}");

            // Part 2: find common root orbit (CRO).  Transfers needed is the number of orbits
            // between You and the CRO and between Santa and the CRO.
            // Trace route from Santa to COM
            Node cur = planets["SAN"];
            while (cur.parent != null)
            {
                cur = cur.parent;
                cur.santaTrace = true;
            }
            // Trace route from You to COM: but stop at CRO.
            cur = planets["YOU"];
            while (cur.parent != null)
            {
                cur = cur.parent;
                if (cur.santaTrace) break;
            }
            // Last -2 is because you end up orbitting the same planet (therefore left with
            // 2 orbits distance).
            int transfers = planets["SAN"].orbitCount + planets["YOU"].orbitCount - 2 * cur.orbitCount - 2;
            System.Console.WriteLine($"2. {transfers}");
        }
    }
}