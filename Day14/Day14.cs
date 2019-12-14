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

            string filename = "input-ex-13312";
            Dictionary<string, Chemical> material = new Dictionary<string, Chemical>();
            material.Add("ORE", new Ore());
            using (StreamReader sr = new StreamReader(filename))
            {
                string[] data = sr.ReadToEnd().Trim().Split("\n");
                foreach (string line in data)
                {
                    string[] formula = line.Split("=>");
                    // result
                    string[] item = formula[1].Trim().Split(" ");
                    if (material.ContainsKey(item[1]))
                        material[item[1]].Quantity = Decimal.Parse(item[0]);
                    else
                        material.Add(item[1], new Chemical(item[1], Int32.Parse(item[0])));
                    Chemical result = material[item[1]];
                    // sources
                    foreach (string s in formula[0].Split(","))
                    {
                        item = s.Trim().Split(" ");
                        if (!material.ContainsKey(item[1]))
                        {
                            material.Add(item[1], new Chemical(item[1]));
                        }
                        result.AddSource(material[item[1]], Int32.Parse(item[0]));
                    }
                }
            }
            System.Console.WriteLine($"1. {material["FUEL"].GetOreRequired(1)}");
            foreach(Chemical item in material.Values) System.Console.WriteLine(item.ToString());
        }
    }

    class Chemical
    {
        string Name { get; }
        public decimal Quantity { get; set; }   // Quantity of this material produced by single reaction

        private Dictionary<Chemical,int> sourceMaterial = new Dictionary<Chemical, int>();

        public Chemical (string name, int quantity = 1)
        {
            this.Name = name;
            this.Quantity = quantity;
        }

        public void AddSource(Chemical chemical, int quantity)
        {
            sourceMaterial.Add(chemical, quantity);
        }

        public virtual decimal GetOreRequired(decimal units = 1)
        {
            decimal sum = 0;
            foreach (var item in sourceMaterial)
            {
                sum += item.Key.GetOreRequired(item.Value);
            }
            System.Console.WriteLine($"{sum} ORE makes {Quantity} {Name}");
            sum *= Math.Ceiling(units / Quantity);  // In integer increments
            return sum;
            //return sourceMaterial.Sum(x => x.Key.GetOreRequired(x.Value)) * units / Quantity;
        }

        public override string ToString()
        {
            string s = String.Join(", ", sourceMaterial.Select(x => $"{x.Value} {x.Key.Name}").ToArray());
            s += $" => {this.Quantity} {this.Name}";
            return s;
        }
    }

    class Ore : Chemical
    {
        public Ore () : base("Ore") { }

        public override decimal GetOreRequired(decimal units) => units;
    }
}