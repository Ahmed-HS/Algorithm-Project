using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace AlgorithmProject
{
    // Common Inforamtion between a pair of actors
    class CommonInfo
    {
        private int Frequency;
        public string CommonMovie;

        public CommonInfo()
        {
            Frequency = 0;
        }

        public void IncreaseFrequency()
        {
            Frequency++;
        }

        public void SetCommonMovie(string MovieName)
        {
            CommonMovie = MovieName;
        }

        public int GetFrequency()
        {
            return Frequency;
        }

        string GetCommonMovie()
        {
            return CommonMovie;
        }
    }

    // State of each actor ,to be later used in shortest path
    class ActorState
    {
        public bool Visited;
        public int DistanceFromSource,Frequency;
        public string Parent;

        public ActorState()
        {
            Visited = false;
            DistanceFromSource = int.MaxValue;
            Frequency = 0;
            Parent = "Not Set";
        }

        public void MarkVisted(int Distance, int NewFrequency ,string CurrentParent)
        {
            Visited = true;
            DistanceFromSource = Distance;
            Frequency = NewFrequency;
            Parent = CurrentParent;
        }

        public void Reset()
        {
            Visited = false;
            DistanceFromSource = int.MaxValue;
            Frequency = 0;
            Parent = "Not Set";
        }
    }

    static class Graph
    {
        //Graph of Relations the strings are actor names 
        private static Dictionary<string, Dictionary<string, CommonInfo>> Relations;
        private static Dictionary<string, ActorState> ActorStates;
        
        private static int NumberOfActors;
        private static FileStream MyDataFile;
        private static StreamReader MyReader;

        static Graph()
        {
            Relations = new Dictionary<string, Dictionary<string, CommonInfo>>();
            ActorStates = new Dictionary<string, ActorState>();
        }

        private static void ResetActorStates()
        {
            // Reset each Actor's State
            foreach (ActorState CurrentActor in ActorStates.Values)
            {
                CurrentActor.Reset();
            }
            ActorStates["Null"].MarkVisted(int.MaxValue, int.MaxValue, "Null");
        }

        public static string ReadQueries(string FileName)
        {
            MyDataFile = new FileStream(FileName, FileMode.Open);
            MyReader = new StreamReader(MyDataFile);

            string[] CurrentQuery;
            string Source, Target, QueriesResult = "";

            while (MyReader.Peek() != -1)
            {
                CurrentQuery = MyReader.ReadLine().Split('/');
                Source = CurrentQuery[0];
                Target = CurrentQuery[1];
                QueriesResult += GetTwoActorsRelation(Source, Target);      

            }
            MyReader.Dispose();
            MyDataFile.Dispose();
            return QueriesResult;

        }

        private static Tuple<string,string> GetPath(string Source, string Target)
        {
            string Parent , CurrentActor = Target;
            string MoviePath = "" , ActorPath = Target;

            while (CurrentActor != Source)
            {
                Parent = ActorStates[CurrentActor].Parent;
                MoviePath = Relations[CurrentActor][Parent].CommonMovie + " => " + MoviePath;
                ActorPath = Parent + " -> " + ActorPath;
                CurrentActor = Parent;
            }

            return new Tuple<string, string>(ActorPath,MoviePath);
        }

        private static void BFSReset(string Source, string Target)
        {
            ActorStates[Source].Reset();

            Queue<string> TraverseQueue = new Queue<string>();

            string CurrentActor;
            TraverseQueue.Enqueue(Source);
            while (TraverseQueue.Count > 0)
            {
                CurrentActor = TraverseQueue.Dequeue();
                foreach (string Neighbour in Relations[CurrentActor].Keys)
                {
                    if (ActorStates[Neighbour].Visited)
                    {
                        ActorStates[Neighbour].Reset();
                        TraverseQueue.Enqueue(Neighbour);
                    }
                  
                }
            }

        }

        public static string GetTwoActorsRelation(string Source,string Target)
        {
            if (!Relations.ContainsKey(Source) || !Relations.ContainsKey(Target) || Source == Target)
            {
                return "Please Enter Correct Actor Names";
            }

            string Result="";
            BFS(Source, Target);
            Tuple<string, string> ShortestPath = GetPath(Source, Target);
            string ActorPath = ShortestPath.Item1;
            string MoviePath = ShortestPath.Item2;

            Result += Source + "/" + Target + "\n";
            Result += "DoS = " + ActorStates[Target].DistanceFromSource + ", RS = " + ActorStates[Target].Frequency + "\n";
            Result += "Chain Of Actors : " + ActorPath + "\n";
            Result += "Chain Of Movies : " + MoviePath + "\n\n";
            //BFSReset(Source, Target);
            return Result;

        }

        public static string GetOneToAllRelation(string Source)
        {
            if (!Relations.ContainsKey(Source))
            {
                return "Please Enter Correct Actor Names";
            }

            string Result = "Shortest Path \t\t Frequency\n";
            BFS(Source);
            SortedDictionary<int, int> Distribution = new SortedDictionary<int, int>();

            foreach (string CurrentActor in ActorStates.Keys)
            {
                if (Distribution.ContainsKey(ActorStates[CurrentActor].DistanceFromSource))
                {
                    Distribution[ActorStates[CurrentActor].DistanceFromSource]++;
                }
                else if(ActorStates[CurrentActor].DistanceFromSource != int.MaxValue)
                {
                    Distribution.Add(ActorStates[CurrentActor].DistanceFromSource, 1);
                }             
            }


            foreach(int ShortestPath in Distribution.Keys)
            {
                Result += ShortestPath + "\t\t\t\t" + Distribution[ShortestPath] + "\n";
            }

            return Result;
        }

        private static void BFS(string Source, string Target = "Null")
        {
            ResetActorStates();

            ActorStates[Source].MarkVisted(0, 0,"Source");

            Queue<string> TraverseQueue = new Queue<string>();
            string CurrentActor;
            TraverseQueue.Enqueue(Source);
            while (TraverseQueue.Any())
            {
                CurrentActor = TraverseQueue.Dequeue();
                foreach (string Neighbour in Relations[CurrentActor].Keys)
                {
                    if (!ActorStates[Neighbour].Visited)
                    {                      
                        int Distance = ActorStates[CurrentActor].DistanceFromSource + 1;
                        int Frequency = ActorStates[CurrentActor].Frequency + Relations[CurrentActor][Neighbour].GetFrequency();
                        ActorStates[Neighbour].MarkVisted(Distance, Frequency, CurrentActor);
                        TraverseQueue.Enqueue(Neighbour);
                        if (Distance > ActorStates[Target].DistanceFromSource)
                        {
                            TraverseQueue.Clear();
                            break;
                        }
                    }// Checking if current path is equal to the shortest path in length and sets the frequency to the max between the two
                    else if (ActorStates[CurrentActor].DistanceFromSource + 1 == ActorStates[Neighbour].DistanceFromSource)
                    {
                        int AltFrequency = ActorStates[CurrentActor].Frequency + Relations[CurrentActor][Neighbour].GetFrequency();
                        if (AltFrequency > ActorStates[Neighbour].Frequency)
                        {
                            ActorStates[Neighbour].Frequency = AltFrequency;
                            ActorStates[Neighbour].Parent = CurrentActor;
                        }
                    }
                }
            }
        }

        public static void ClearGraph()
        {
            ActorStates.Clear();
            Relations.Clear();
        }

        public static string[] ReadGraph(string FileName)
        {
            ClearGraph();
            Stopwatch MyWatch = new Stopwatch();
            MyWatch.Start();
            MyDataFile = new FileStream(FileName , FileMode.Open);
            MyReader = new StreamReader(MyDataFile);
            string[] Actors;
            ActorStates.Add("Null", new ActorState());
            ActorStates["Null"].MarkVisted(int.MaxValue, int.MaxValue, "Null");
            while (MyReader.Peek() != -1)
            {
                // Read a movie then split it by '/' where Actors[0] is the movie name 
                Actors = MyReader.ReadLine().Split('/');
                int ActorsPerMovie = Actors.Length;

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
                            Relations[Actors[i]][Actors[j]].SetCommonMovie(Actors[0]);
                        }

                        Relations[Actors[i]][Actors[j]].IncreaseFrequency();
                        

                        if (!Relations.ContainsKey(Actors[j]))
                        {
                            NumberOfActors++;
                            ActorStates.Add(Actors[j], new ActorState());
                            Relations.Add(Actors[j], new Dictionary<string, CommonInfo>());
                        }

                        if (!Relations[Actors[j]].ContainsKey(Actors[i]))
                        {
                            Relations[Actors[j]].Add(Actors[i], new CommonInfo());
                            Relations[Actors[j]][Actors[i]].SetCommonMovie(Actors[0]);
                        }

                        Relations[Actors[j]][Actors[i]].IncreaseFrequency();

                    }
                }
            }

            MyWatch.Stop();
            MyReader.Dispose();
            MyDataFile.Dispose();
            string[] AllActors = ActorStates.Keys.ToArray();
            AllActors[0] = "Select an actor";
            return AllActors;
        }

    }

}
