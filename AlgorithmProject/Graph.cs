using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace AlgorithmProject
{
    class CommonInfo
    {
        private int Frequency;
        private List<string> CommonMovies;

        public CommonInfo()
        {
            Frequency = 0;
            CommonMovies = new List<string>();
        }

        public void IncreaseFrequency()
        {
            Frequency++;
        }

        public void AddCommonMovie(string MovieName)
        {
            CommonMovies.Add(MovieName);
        }

        int GetFrequency()
        {
            return Frequency;
        }

        List<string> GetCommonMovies()
        {
            return CommonMovies;
        }
    }


    static class Graph
    {

        public static Dictionary<string, Dictionary<string, CommonInfo>> MyGraph;
        static FileStream MyDataFile;
        static FileStream OutputFile;
        static StreamReader MyReader;
        static StreamWriter MyWriter;
        static Graph()
        {
            MyGraph = new Dictionary<string, Dictionary<string, CommonInfo>>();
        }

        public static void ReadGraph(string FileName)
        {
            Stopwatch MyWatch = new Stopwatch();
            MyWatch.Start();
            MyDataFile = new FileStream(FileName, FileMode.Open);
            OutputFile = new FileStream("Output.txt", FileMode.Create);
            MyReader = new StreamReader(MyDataFile);
            MyWriter = new StreamWriter(OutputFile);
            string[] Actors;

            while (MyReader.Peek() != -1)
            {


                Actors = MyReader.ReadLine().Split('/');
                int NumberOfActors = Actors.Length;

                Console.WriteLine();
                Console.WriteLine("Movie Name : " + Actors[0]);
                Console.WriteLine();

                MyWriter.WriteLine();
                MyWriter.WriteLine("Movie Name : " + Actors[0]);
                MyWriter.WriteLine();

                // Start From Fisrt Actor
                for (int i = 1; i < NumberOfActors; i++)
                {
                    if (!MyGraph.ContainsKey(Actors[i]))
                    {
                        MyGraph.Add(Actors[i], new Dictionary<string, CommonInfo>());
                    }

                    for (int j = i + 1; j < NumberOfActors; j++)
                    {
                        if (!MyGraph[Actors[i]].ContainsKey(Actors[j]))
                        {
                            MyGraph[Actors[i]].Add(Actors[j], new CommonInfo());
                        }

                        MyGraph[Actors[i]][Actors[j]].IncreaseFrequency();
                        MyGraph[Actors[i]][Actors[j]].AddCommonMovie(Actors[0]);

                        if (!MyGraph.ContainsKey(Actors[j]))
                        {
                            MyGraph.Add(Actors[j], new Dictionary<string, CommonInfo>());
                        }

                        if (!MyGraph[Actors[j]].ContainsKey(Actors[i]))
                        {
                            MyGraph[Actors[j]].Add(Actors[i], new CommonInfo());
                        }

                        MyGraph[Actors[j]][Actors[i]].IncreaseFrequency();
                        MyGraph[Actors[j]][Actors[i]].AddCommonMovie(Actors[0]);

                        Console.WriteLine(Actors[i] + " Acted With " + Actors[j]);
                        MyWriter.WriteLine(Actors[i] + " Acted With " + Actors[j]);
                    }
                }
            }
            Console.WriteLine();
            MyWriter.WriteLine();
            MyWriter.WriteLine("Created Graph in : " + MyWatch.Elapsed.TotalSeconds + "Seconds");
            Console.WriteLine("Created Graph in : " + MyWatch.Elapsed.TotalSeconds + "Seconds");
            MyWatch.Stop();
        }

    }
}
