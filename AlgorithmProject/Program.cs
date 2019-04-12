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
            Graph.ReadQueries("Queries.txt");        
        }
    }
}
