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
                        reaction.Add(item[1], new Reaction(item[1], Int32.Parse(item[0])));
                    Reaction result = reaction[item[1]];
                    result.Output = Int32.Parse(item[0]);

                    // sources
                    foreach (string s in formula[0].Split(","))
                    {
                        item = s.Trim().Split(" ");
                        result.AddSource(item[1], Int32.Parse(item[0]));
                        if (!reaction.ContainsKey(item[1]))
                            reaction.Add(item[1], new Reaction(item[1], 0));    // Skeleton reaction, to add product to
                        reaction[item[1]].AddProduct(result.Name, result.Output);                        
                    }
                }
            }

            IEnumerable<string> ordered = new Topological(reaction).GetOrdered();
            Dictionary<string, int> quantity = new Dictionary<string, int>(ordered.Count());
            quantity["FUEL"] = 1;

            foreach (string item in ordered)
            {
                int output = reaction[item].Output;
                int needed = quantity[item];
                int toMake = (int)Math.Ceiling((decimal)needed / output);
                foreach (var dependency in reaction[item].GetDependencies())
                {
                    if (quantity.ContainsKey(dependency.Key))
                        quantity[dependency.Key] += dependency.Value * toMake;
                    else
                        quantity.Add(dependency.Key, dependency.Value * toMake);
                }
            }
            System.Console.WriteLine($"1. {quantity["ORE"]}");
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
        public int Output { get; set; }
        private Dictionary<string,int> input = new Dictionary<string, int>();       // Chemicals that go into Name
        private Dictionary<string, int> product = new Dictionary<string, int>();    // Chemicals that require Name

        public Reaction (string name, int output = 1)
        {
            this.Name = name;
            this.Output = output;
        }

        public void AddSource(string name, int quantity)
        {
            input.Add(name, quantity);
        }

        public void AddProduct(string name, int quantity)
        {
            product.Add(name, quantity);
        }

        public IEnumerable<KeyValuePair<string, int>> GetDependencies () => input;

        public IEnumerable<KeyValuePair<string, int>> GetProducts () => product;
    }

    class Ore : Reaction
    {
        public Ore () : base("Ore") { }
    }
}