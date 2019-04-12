using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Reading Movies Information ");
            Graph.ReadGraph("Input.txt");
            Console.WriteLine();

            string Source, Target;

            Source = "Z";
            Target = "L";

            Console.WriteLine("Shortest path between " + Source + " and " + Target + " : " + Graph.BFS(Source, Target));

            // Work in Progress
            //Console.WriteLine("Frequency between " + Source + " and " + Target + " : " + Graph.GetFrequency(Source, Target));
            Console.WriteLine();
        }
    }
}
