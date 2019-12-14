using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AoC2019
{
    class Day14
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Day 14");

            string filename = "input";
            Dictionary<string, Reaction> reaction = new Dictionary<string, Reaction>();
            reaction.Add("ORE", new Reaction("ORE"));

            using (StreamReader sr = new StreamReader(filename))
            {
                string[] data = sr.ReadToEnd().Trim().Split("\n");
                foreach (string line in data)
                {
                    string[] formula = line.Split("=>");

                    // result
                    string[] item = formula[1].Trim().Split(" ");
                    if (!reaction.ContainsKey(item[1]))
                        reaction.Add(item[1], new Reaction(item[1], Int64.Parse(item[0])));
                    Reaction result = reaction[item[1]];
                    result.Output = Int64.Parse(item[0]);

                    // sources
                    foreach (string s in formula[0].Split(","))
                    {
                        item = s.Trim().Split(" ");
                        result.AddSource(item[1], Int64.Parse(item[0]));
                        if (!reaction.ContainsKey(item[1]))
                            reaction.Add(item[1], new Reaction(item[1], 0));    // Skeleton reaction, to add product to
                        reaction[item[1]].AddProduct(result.Name, result.Output);                        
                    }
                }
            }

            // Part 1
            long requiredOre = GetRequiredOre(reaction);
            System.Console.WriteLine($"1. {requiredOre}");

            // Part 2
            long target = 1_000_000_000_000;
            long lower = (target / requiredOre) - 1_000;
            long higher = (target / requiredOre) + 1_000_000_000;
            while (lower < higher)
            {
                long mid = (lower + higher) / 2;
                long guess = GetRequiredOre(reaction, mid);
                if (guess > target)
                {
                    // System.Console.WriteLine($"MORE: {guess}");
                    higher = mid;
                }
                else if (guess < target)
                {
                    // System.Console.WriteLine($"LESS: {guess}");
                    if (mid == lower) break;
                    lower = mid;
                }
                else
                {
                    lower = mid;
                    break;
                }
            }
            System.Console.WriteLine($"2. {lower}");
        }

        private static long GetRequiredOre(Dictionary<string, Reaction> reaction, long fuelTarget = 1)
        {
            IEnumerable<string> ordered = new Topological(reaction).GetOrdered();
            Dictionary<string, long> quantity = new Dictionary<string, long>(ordered.Count());
            quantity["FUEL"] = fuelTarget;
            
            foreach (string item in ordered)
            {
                long output = reaction[item].Output;
                long needed = quantity[item];
                long toMake = (long)Math.Ceiling((decimal)needed / output);
                foreach (var dependency in reaction[item].GetDependencies())
                {
                    if (quantity.ContainsKey(dependency.Key))
                        quantity[dependency.Key] += dependency.Value * toMake;
                    else
                        quantity.Add(dependency.Key, dependency.Value * toMake);
                }
            }
            return quantity["ORE"];
        }
    }

    class Topological
    {
        private List<string> depthFirstOrder;
        private HashSet<string> marked;

        public Topological (Dictionary<string, Reaction> reaction)
        {
            depthFirstOrder = new List<string>(reaction.Count);
            marked = new HashSet<string>(reaction.Count);

            foreach(string item in reaction.Keys)
                if (!marked.Contains(item))
                    DepthFirstSearch(reaction, item);
            
            // depthFirstOrder.Reverse();
        }

        private void DepthFirstSearch (Dictionary<string, Reaction> reaction, string start)
        {
            marked.Add(start);

            foreach (var item in reaction[start].GetProducts())
                if (!marked.Contains(item.Key))
                    DepthFirstSearch(reaction, item.Key);

            depthFirstOrder.Add(start);
        }

        public IEnumerable<string> GetOrdered() => depthFirstOrder;
    }

    class Reaction
    {
        public string Name { get; }
        public long Output { get; set; }
        private Dictionary<string,long> input = new Dictionary<string, long>();       // Chemicals that go into Name
        private Dictionary<string, long> product = new Dictionary<string, long>();    // Chemicals that require Name

        public Reaction (string name, long output = 1)
        {
            this.Name = name;
            this.Output = output;
        }

        public void AddSource(string name, long quantity)
        {
            input.Add(name, quantity);
        }

        public void AddProduct(string name, long quantity)
        {
            product.Add(name, quantity);
        }

        public IEnumerable<KeyValuePair<string, long>> GetDependencies () => input;

        public IEnumerable<KeyValuePair<string, long>> GetProducts () => product;
    }

    class Ore : Reaction
    {
        public Ore () : base("Ore") { }
    }
}