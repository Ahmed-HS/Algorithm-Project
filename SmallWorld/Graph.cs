using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using Priority_Queue;

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
        public int DistanceFromSource, Frequency;
        public string Parent;

        public ActorState()
        {
            Visited = false;
            DistanceFromSource = int.MaxValue;
            Frequency = 0;
            Parent = "Not Set";
        }

        public void MarkVisted(int Distance, int NewFrequency, string CurrentParent)
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

        public void Dijkstra()
        {
            Visited = false;
            DistanceFromSource = 0;
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

        private static Tuple<string, string> GetPath(string Source, string Target)
        {
            string Parent, CurrentActor = Target;
            string MoviePath = "", ActorPath = Target;

            if (ActorStates[Target].Parent == "Not Set")
            {
                return new Tuple<string, string>("No Path Found", "No Path Found");
            }

            while (CurrentActor != Source)
            {
                Parent = ActorStates[CurrentActor].Parent;
                MoviePath = Relations[CurrentActor][Parent].CommonMovie + " => " + MoviePath;
                ActorPath = Parent + " -> " + ActorPath;
                CurrentActor = Parent;
            }

            return new Tuple<string, string>(ActorPath, MoviePath);
        }

        public static string GetTwoActorsRelation(string Source, string Target)
        {
            if (!Relations.ContainsKey(Source) || !Relations.ContainsKey(Target) || Source == Target)
            {
                return "Please Enter Correct Actor Names";
            }

            string Result = "";
            BFS(Source, Target);
            Tuple<string, string> ShortestPath = GetPath(Source, Target);
            string ActorPath = ShortestPath.Item1;
            string MoviePath = ShortestPath.Item2;

            ActorState TargetResult = ActorStates[Target];

            Result += Source + "/" + Target + "\n";
            Result += "DoS = " + TargetResult.DistanceFromSource + ", RS = " + TargetResult.Frequency + "\n";
            Result += "Chain Of Actors : " + ActorPath + "\n";
            Result += "Chain Of Movies : " + MoviePath + "\n\n";
            return Result;

        }

        public static string DistributionOfShotestPath(string Source)
        {
            if (!Relations.ContainsKey(Source))
            {
                return "Please Enter Correct Actor Names";
            }

            ResetActorStates();

            ActorStates[Source].MarkVisted(0, 0, "Source");

            Queue<string> TraverseQueue = new Queue<string>();
            SortedDictionary<int, int> Distribution = new SortedDictionary<int, int>();
            string CurrentActor, Result = "Shortest Path \t\t\t Frequency \n0 \t\t\t\t 1 \n";
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
                        if (Distribution.ContainsKey(ActorStates[Neighbour].DistanceFromSource))
                        {
                            Distribution[ActorStates[Neighbour].DistanceFromSource]++;
                        }
                        else
                        {
                            Distribution.Add(ActorStates[Neighbour].DistanceFromSource, 1);
                        }
                    }
                }
            }

            foreach (int ShortestPath in Distribution.Keys)
            {
                Result += ShortestPath + "\t\t\t\t" + Distribution[ShortestPath] + "\n";
            }

            return Result;
        }

        public static string GetStrongestPath(string Source, string Target)
        {
            if (!Relations.ContainsKey(Source) || !Relations.ContainsKey(Target) || Source == Target)
            {
                return "Please Enter Correct Actor Names";
            }

            string Result = "";
            Dijkstra(Source, Target);
            Tuple<string, string> ShortestPath = GetPath(Source, Target);
            string ActorPath = ShortestPath.Item1;
            string MoviePath = ShortestPath.Item2;
            ActorState TargetResult = ActorStates[Target];
            Result += Source + "/" + Target + "\n";
            Result += "Path Length : " + TargetResult.DistanceFromSource + "   Total Frequency : " + TargetResult.Frequency + "\n";
            Result += "Chain Of Actors : " + ActorPath + "\n";
            Result += "Chain Of Movies : " + MoviePath + "\n\n";

            return Result;
        }

        private static string BidirectionalSearch(string Source, string Target)
        {

            if (Relations[Source].ContainsKey(Target))
            {
                ActorStates[Target].MarkVisted(1, Relations[Source][Target].GetFrequency(), Source);
                return Source + " -> " + Target;
            }

            ResetActorStates();
            Queue<string> ForwardQueue = new Queue<string>();
            Queue<string> BackwardQueue = new Queue<string>();
            ActorStates[Source].MarkVisted(0, 0, "Source");
            ActorStates[Target].MarkVisted(0, 0, "Target");
            ForwardQueue.Enqueue(Source);
            BackwardQueue.Enqueue(Target);
            string IntersectingActor = "";
            string ForwardParent = "", BackwardParent = "";
            int ForwardDistance = 0, BackwardDistance = 0;
            int ForwardFrequency = 0, BackwardFrequency = 0;
            int TotalDistance = 0, TotalFrequency = 0;
            int FIntersection = int.MaxValue;
            int Bintersection = int.MaxValue;
            bool Intersected = false;
            List<Tuple<string, string, string>> Common = new List<Tuple<string, string, string>>();
            while (ForwardQueue.Any() || BackwardQueue.Any())
            {
                if (ForwardQueue.Any())
                {
                    string CurrentActor = ForwardQueue.Dequeue();
                    foreach (string Neighbour in Relations[CurrentActor].Keys)
                    {

                        if (BackwardQueue.Contains(Neighbour))
                        {
                            if (!Intersected)
                            {
                                FIntersection = ActorStates[CurrentActor].DistanceFromSource + 1;
                            }
                            ForwardParent = CurrentActor;
                            BackwardParent = ActorStates[Neighbour].Parent;
                            Intersected = true;
                            Common.Add(new Tuple<string, string, string>(Neighbour, ForwardParent, BackwardParent));
                        }
                        else if (!ActorStates[Neighbour].Visited)
                        {
                            int Distance = ActorStates[CurrentActor].DistanceFromSource + 1;
                            int Frequency = ActorStates[CurrentActor].Frequency + Relations[CurrentActor][Neighbour].GetFrequency();
                            ActorStates[Neighbour].MarkVisted(Distance, Frequency, CurrentActor);
                            ForwardQueue.Enqueue(Neighbour);
                            if (Distance > FIntersection)
                            {
                                ForwardQueue.Clear();
                                break;
                            }
                        }
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


                if (BackwardQueue.Any())
                {
                    string CurrentActor = BackwardQueue.Dequeue();
                    foreach (string Neighbour in Relations[CurrentActor].Keys)
                    {

                        if (ForwardQueue.Contains(Neighbour))
                        {
                            if (!Intersected)
                            {
                                Bintersection = ActorStates[CurrentActor].DistanceFromSource + 1;
                            }
                            BackwardParent = CurrentActor;
                            ForwardParent = ActorStates[Neighbour].Parent;
                            Intersected = true;
                            Common.Add(new Tuple<string, string, string>(Neighbour, ForwardParent, BackwardParent));
                        }
                        else if (!ActorStates[Neighbour].Visited)
                        {
                            int Distance = ActorStates[CurrentActor].DistanceFromSource + 1;
                            int Frequency = ActorStates[CurrentActor].Frequency + Relations[CurrentActor][Neighbour].GetFrequency();
                            ActorStates[Neighbour].MarkVisted(Distance, Frequency, CurrentActor);
                            BackwardQueue.Enqueue(Neighbour);
                            if (Distance > Bintersection)
                            {
                                BackwardQueue.Clear();
                                break;
                            }
                        }
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

            int MinimumDistance = int.MaxValue;
            int MaximumFrequency = -1;
            string IActor, FParent, BParent;
            foreach (Tuple<string, string, string> Intersection in Common)
            {
                IActor = Intersection.Item1;
                FParent = Intersection.Item2;
                BParent = Intersection.Item3;
                TotalDistance = ActorStates[BParent].DistanceFromSource + ActorStates[FParent].DistanceFromSource + 2;
                TotalFrequency = ActorStates[BParent].Frequency + ActorStates[FParent].Frequency;
                TotalFrequency += Relations[FParent][IActor].GetFrequency();
                TotalFrequency += Relations[BParent][IActor].GetFrequency();

                if (TotalDistance <= MinimumDistance && TotalFrequency > MaximumFrequency)
                {
                    IntersectingActor = IActor;
                    ForwardParent = FParent;
                    BackwardParent = BParent;
                    MinimumDistance = TotalDistance;
                    MaximumFrequency = TotalFrequency;
                }
            }

            ActorStates[Target].DistanceFromSource = MinimumDistance;
            ActorStates[Target].Frequency = MaximumFrequency;
            string Actor = ForwardParent, Path = IntersectingActor;
            return "--";
            Path = Actor + " -> " + Path;
            string Parent;
            while (Actor != Source)
            {
                Parent = ActorStates[Actor].Parent;
                Path = Parent + " -> " + Path;
                Actor = Parent;
            }

            Actor = BackwardParent;
            Path += " - > " + Actor;
            while (Actor != Target)
            {
                Parent = ActorStates[Actor].Parent;
                Path += " - > " + Parent;
                Actor = Parent;
            }
            return Path;
        }

        private static void BFS(string Source, string Target)
        {
            ResetActorStates();
            ActorStates[Source].MarkVisted(0, 0, "Source");

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

        private static void Dijkstra(string Source, string Target)
        {
            ResetActorStates();
            SimplePriorityQueue<string> DijkstraQueue = new SimplePriorityQueue<string>(); ;
            ActorStates[Source].MarkVisted(0, 0, "Source");
            DijkstraQueue.Enqueue(Source, -1);
            string CurrentActor;
            while (DijkstraQueue.Any())
            {
                CurrentActor = DijkstraQueue.Dequeue();
                foreach (string Neighbour in Relations[CurrentActor].Keys)
                {
                    int Distance = ActorStates[CurrentActor].DistanceFromSource + 1;
                    int Frequency = ActorStates[CurrentActor].Frequency + Relations[CurrentActor][Neighbour].GetFrequency();
                    if (!ActorStates[Neighbour].Visited && Frequency > ActorStates[Neighbour].Frequency)
                    {
                        ActorStates[Neighbour].MarkVisted(Distance, Frequency, CurrentActor);
                        DijkstraQueue.Enqueue(Neighbour, -ActorStates[Neighbour].Frequency);
                    }
                    if (Neighbour == Target)
                    {
                        DijkstraQueue.Clear();
                        break;
                    }
                }
            }
        }

        public static string MST(string Source)
        {
            if (!Relations.ContainsKey(Source))
            {
                return "Please Enter Correct Actor Names";
            }

            ResetActorStates();

            ActorStates[Source].MarkVisted(0, 0, "Source");

            Queue<string> TraverseQueue = new Queue<string>();
            int Count = 0;
            string CurrentActor, Result = "";
            TraverseQueue.Enqueue(Source);
            HashSet<string> MST = new HashSet<string>();
            while (TraverseQueue.Any())
            {
                CurrentActor = TraverseQueue.Dequeue();
                foreach (string Neighbour in Relations[CurrentActor].Keys)
                {
                    if (!ActorStates[Neighbour].Visited)
                    {
                        if (!MST.Contains(Relations[CurrentActor][Neighbour].CommonMovie))
                        {
                            MST.Add(Relations[CurrentActor][Neighbour].CommonMovie);
                            Count++;
                            Result += Relations[CurrentActor][Neighbour].CommonMovie + "\n";
                        }
                        int Distance = ActorStates[CurrentActor].DistanceFromSource + 1;
                        int Frequency = ActorStates[CurrentActor].Frequency + Relations[CurrentActor][Neighbour].GetFrequency();
                        ActorStates[Neighbour].MarkVisted(Distance, Frequency, CurrentActor);
                        TraverseQueue.Enqueue(Neighbour);
                    }
                }
            }
            return Count + " \n" + Result;
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
            MyDataFile = new FileStream(FileName, FileMode.Open);
            MyReader = new StreamReader(MyDataFile);
            string[] Actors;
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
            return ActorStates.Keys.ToArray();
        }

    }

}
