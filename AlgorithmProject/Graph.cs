using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace AlgorithmProject
{
    // Common Inforamtion between a pair of actors
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

        public int GetFrequency()
        {
            return Frequency;
        }

        List<string> GetCommonMovies()
        {
            return CommonMovies;
        }
    }

    // State of each actor ,to be later used in shortest path
    class ActorState
    {
        public bool Visited;
        public int DistanceFromSource;
        public string Parent;

        public ActorState()
        {
            Visited = false;
            DistanceFromSource = int.MaxValue;
            Parent = "Not Set";
        }

        public void MarkVisted(int Distance,string CurrentParent)
        {
            Visited = true;
            DistanceFromSource = Distance;
            Parent = CurrentParent;
        }
    }

    static class Graph
    {
        //Grapg of Relations the strings are actor names 
        private static Dictionary<string, Dictionary<string, CommonInfo>> Relations;
        private static Dictionary<string, ActorState> ActorStates;
        private static int NumberOfActors;
        private static FileStream MyDataFile;
        private static FileStream OutputFile;
        private static StreamReader MyReader;
        private static StreamWriter MyWriter;

        static Graph()
        {
            Relations = new Dictionary<string, Dictionary<string, CommonInfo>>();
            ActorStates = new Dictionary<string, ActorState>();
        }
        private static void ReIntialize()
        {
            // Rest each Actor's State
            foreach (ActorState CurrentActor in ActorStates.Values)
            {
                CurrentActor.Visited = false;
                CurrentActor.DistanceFromSource = int.MaxValue;
                CurrentActor.Parent = "Not Set";
            }
        }

        public static int GetFrequency(string Source, string Target)
        {
            return Relations[Source][Target].GetFrequency();
        }

        public static int BFS(string Source, string Target)
        {
            ActorStates[Source].MarkVisted(0, "Source");
            int ShortestPath;

            Queue<string> TraverseQueue = new Queue<string>();

            string CurrentActor;
            TraverseQueue.Enqueue(Source);

            while (TraverseQueue.Count > 0)
            {
                CurrentActor = TraverseQueue.Dequeue();
                foreach (string Neighbour in Relations[CurrentActor].Keys)
                {
                    if (!ActorStates[Neighbour].Visited)
                    {
                        ActorStates[Neighbour].MarkVisted(ActorStates[CurrentActor].DistanceFromSource + 1, CurrentActor);
                        if (Neighbour == Target)
                        {
                            TraverseQueue.Clear();
                            break;
                        }
                        TraverseQueue.Enqueue(Neighbour);
                    }
                }
            }

            ShortestPath = ActorStates[Target].DistanceFromSource;
            ReIntialize();
            return ShortestPath;
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
                // Read a movie then split it by '/' where Actors[0] is the movie name 
                Actors = MyReader.ReadLine().Split('/');
                int ActorsPerMovie = Actors.Length;

                // Loging Info
                Console.WriteLine();
                Console.WriteLine("Movie Name : " + Actors[0]);
                Console.WriteLine();

                MyWriter.WriteLine();
                MyWriter.WriteLine("Movie Name : " + Actors[0]);
                MyWriter.WriteLine();

                // Start From Fisrt Actor
                for (int i = 1; i < ActorsPerMovie; i++)
                {
                    // if the actor is not in the Relations Graph  add it to both Relations and ActorStates for later use in BFS 
                    if (!Relations.ContainsKey(Actors[i]))
                    {
                        NumberOfActors++;
                        ActorStates.Add(Actors[i], new ActorState());
                        Relations.Add(Actors[i], new Dictionary<string, CommonInfo>());
                    }

                    // Add to each actor in Relations the actors he acted with in the Current Movie (undirected-both ways)
                    // if a pair of actors acted together before just increase the frequency and add the movie name
                    for (int j = i + 1; j < ActorsPerMovie; j++)
                    {
                        if (!Relations[Actors[i]].ContainsKey(Actors[j]))
                        {
                            Relations[Actors[i]].Add(Actors[j], new CommonInfo());
                        }

                        Relations[Actors[i]][Actors[j]].IncreaseFrequency();
                        Relations[Actors[i]][Actors[j]].AddCommonMovie(Actors[0]);

                        if (!Relations.ContainsKey(Actors[j]))
                        {
                            NumberOfActors++;
                            ActorStates.Add(Actors[j], new ActorState());
                            Relations.Add(Actors[j], new Dictionary<string, CommonInfo>());
                        }

                        if (!Relations[Actors[j]].ContainsKey(Actors[i]))
                        {
                            Relations[Actors[j]].Add(Actors[i], new CommonInfo());
                        }

                        Relations[Actors[j]][Actors[i]].IncreaseFrequency();
                        Relations[Actors[j]][Actors[i]].AddCommonMovie(Actors[0]);

                        Console.WriteLine(Actors[i] + " Acted With " + Actors[j]);
                        MyWriter.WriteLine(Actors[i] + " Acted With " + Actors[j]);
                    }
                }
            }
            // Loging Info
            Console.WriteLine();
            MyWriter.WriteLine();
            MyWriter.WriteLine("Created Graph in : " + MyWatch.Elapsed.TotalSeconds + "Seconds");
            Console.WriteLine("Created Graph in : " + MyWatch.Elapsed.TotalSeconds + "Seconds");
            MyWatch.Stop();
        }

    }
}
