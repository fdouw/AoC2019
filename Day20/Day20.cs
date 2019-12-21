using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC2019
{
    class Day20
    {
        public static void Main (string[] args)
        {
            System.Console.WriteLine("Day 20");

            List<Node> maze = new List<Node>();
            string filename = "input";
            using (StreamReader sr = new StreamReader(filename))
            {
                string line;
                List<char[]> tmp = new List<char[]>();
                while ((line = sr.ReadLine()) != null)
                {
                    tmp.Add(line.ToCharArray());
                }
                char[][] rawData = tmp.ToArray();

                // Skip the margins
                for (int y = 2; y < rawData.Length - 2; y++)
                {
                    for (int x = 2; x < rawData[y].Length - 2; x++)
                    {
                        if (rawData[y][x] == '.')
                        {
                            Node node = new Node(x,y);
                            maze.Add(node);
                            if (rawData[y-1][x] == '.')
                            {
                                Node other = maze.Where(n => n.X == x && n.Y == y - 1).Single();
                                node.AddNeighbour(other);
                                other.AddNeighbour(node);
                            }
                            else if (rawData[y-1][x] != '#')    // Only option left: letter (portal)
                            {
                                node.AddPortal($"{rawData[y-2][x]}{rawData[y-1][x]}");
                            }

                            if (rawData[y][x-1] == '.')
                            {
                                Node other = maze.Where(n => n.X == x - 1 && n.Y == y).Single();
                                node.AddNeighbour(other);
                                other.AddNeighbour(node);
                            }
                            else if (rawData[y][x-1] != '#')    // Only option left: letter (portal)
                            {
                                node.AddPortal($"{rawData[y][x-2]}{rawData[y][x-1]}");
                            }

                            if (rawData[y][x+1] != '.' && rawData[y][x+1] != '#')
                            {
                                node.AddPortal($"{rawData[y][x+1]}{rawData[y][x+2]}");
                            }
                            if (rawData[y+1][x] != '.' && rawData[y+1][x] != '#')
                            {
                                node.AddPortal($"{rawData[y+1][x]}{rawData[y+2][x]}");
                            }
                        }
                    }
                }

                // Connect the portals
                foreach (Node node in maze)
                {
                    if (node.Portal != null)
                    {
                        // This node is added to the other when that node is processed in this loop
                        // There maybe no other node with this portal, hence the foreach
                        foreach (Node other in maze.Where(n => n.Portal == node.Portal && n != node))
                        {
                            node.AddNeighbour(other);
                        }
                    }
                }

                // Search for a route
                int dist = BFS (maze);
                System.Console.WriteLine($"1. {dist}");

                // for (int y = 0; y < rawData.Length; y++)
                // {
                //     for (int x = 0; x < rawData[y].Length; x++)
                //     {
                //         System.Console.Write(rawData[y][x]);
                //     }
                //     System.Console.WriteLine();
                // }
            }
        }

        static int BFS (List<Node> maze)
        {
            Node start = maze.Single(n => n.Portal == "AA");
            Node finish = maze.Single(n => n.Portal == "ZZ");

            Queue<Node> searchBoundary = new Queue<Node>();
            searchBoundary.Enqueue(start);

            while (searchBoundary.Count > 0)
            {
                Node node = searchBoundary.Dequeue();
                if (node == finish)
                {
                    return node.Distance;
                }
                else
                {
                    foreach (Node nb in node.GetNeighbours())
                    {
                        if (nb.Mark(node.Distance + 1))
                        {
                            searchBoundary.Enqueue(nb);
                        }
                    }
                }
            }

            // Did not find route from start to finish
            return -1;
        }
    }

    class Node
    {
        internal int X { get; }
        internal int Y { get; }

        internal string Portal { get; private set; }

        internal bool Marked { get; private set; }
        internal int Distance { get; private set; }

        private List<Node> neighbour = new List<Node>(4);

        public Node (int x, int y)
        {
            this.X = x;
            this.Y = y;
            this.Portal = null;
        }

        internal void BfsReset ()
        {
            Marked = false;
            Distance = Int32.MaxValue;
        }

        internal bool Mark (int dist)
        {
            if (!Marked)
            {
                Marked = true;
                Distance = dist;
                return true;
            }
            return false;
        }

        public void AddNeighbour (Node nb)
        {
            if (!neighbour.Contains(nb)) neighbour.Add(nb);
        }

        public void AddPortal (string name)
        {
            this.Portal = name;
        }

        public IEnumerable<Node> GetNeighbours () => neighbour;
    }
}