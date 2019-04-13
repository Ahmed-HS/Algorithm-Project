using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
namespace AlgorithmProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch MyWatch = new Stopwatch();
            MyWatch.Start();
            Console.WriteLine("Reading Movies Information ");
            Graph.ReadGraph("Input.txt");
            Graph.ReadQueries("Queries.txt");
            Console.WriteLine("Number Of Actors : " + Graph.NumberOfActors);
            Console.WriteLine("Total Running Time : " + MyWatch.Elapsed.TotalSeconds);
            Console.WriteLine();
            MyWatch.Stop();
        }
    }
}
