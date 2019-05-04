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
        public int CommonMovie;

        public CommonInfo()
        {
            Frequency = 0;
        }

        public void IncreaseFrequency()
        {
            Frequency++;
        }

        public void SetCommonMovie(int MovieName)
        {
            CommonMovie = MovieName;
        }

        public int GetFrequency()
        {
            return Frequency;
        }

        int GetCommonMovie()
        {
            return CommonMovie;
        }
    }

    // State of each actor ,to be later used in shortest path
    class ActorState
    {
        public bool Visited;
        public int DistanceFromSource, Frequency;
        public int Parent;

        public ActorState()
        {
            Visited = false;
            DistanceFromSource = int.MaxValue;
            Frequency = 0;
            Parent = -1;
        }

        public void MarkVisted(int Distance, int NewFrequency, int CurrentParent)
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
            Parent = -1;
        }
    }

    static class Graph
    {
        //Graph of Relations the strings are actor names 
        private static Dictionary<int, Dictionary<int, CommonInfo>> Relations;
        private static Dictionary<int, ActorState> ActorStates;
        private static Dictionary<string, int> ActorID;
        private static Dictionary<int, string> ActorName;
        private static Dictionary<int, string> MovieName;

        private static int NumberOfActors,NumberOfMovies;
        private static FileStream MyDataFile;
        private static StreamReader MyReader;

        static Graph()
        {
            Relations = new Dictionary<int, Dictionary<int, CommonInfo>>();
            ActorStates = new Dictionary<int, ActorState>();
            ActorID = new Dictionary<string, int>();
            ActorName = new Dictionary<int, string>();
            MovieName = new Dictionary<int, string>();
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
            int SourceID = ActorID[Source], TargetID = ActorID[Target];
            int Parent, CurrentActor = TargetID;
            string MoviePath = "", ActorPath = Target;

            if (ActorStates[TargetID].Parent == -1)
            {
                return new Tuple<string, string>("No Path Found", "No Path Found");
            }

            while (CurrentActor != SourceID)
            {
                Parent = ActorStates[CurrentActor].Parent;
                MoviePath = MovieName[Relations[CurrentActor][Parent].CommonMovie] + " => " + MoviePath;
                ActorPath = ActorName[Parent] + " -> " + ActorPath;
                CurrentActor = Parent;
            }

            return new Tuple<string, string>(ActorPath, MoviePath);
        }

        public static string GetTwoActorsRelation(string Source, string Target)
        {
            if (!ActorID.ContainsKey(Source) || !ActorID.ContainsKey(Target) || Source == Target)
            {
                return "Please Enter Correct Actor Names";
            }

            string Result = "";
            BFS(Source, Target);
            Tuple<string, string> ShortestPath = GetPath(Source, Target);
            string ActorPath = ShortestPath.Item1;
            string MoviePath = ShortestPath.Item2;

            ActorState TargetResult = ActorStates[ActorID[Target]];

            Result += Source + "/" + Target + "\n";
            Result += "DoS = " + TargetResult.DistanceFromSource + ", RS = " + TargetResult.Frequency + "\n";
            Result += "Chain Of Actors : " + ActorPath + "\n";
            Result += "Chain Of Movies : " + MoviePath + "\n\n";
            return Result;

        }

        public static string DistributionOfShotestPath(string Source)
        {
            if (!ActorID.ContainsKey(Source))
            {
                return "Please Enter Correct Actor Names";
            }
            int SourceID = ActorID[Source];
            ResetActorStates();

            ActorStates[SourceID].MarkVisted(0, 0, -1);

            Queue<int> TraverseQueue = new Queue<int>();
            SortedDictionary<int, int> Distribution = new SortedDictionary<int, int>();
            int CurrentActor;
            string Result = "Shortest Path \t\t\t Frequency \n0 \t\t\t\t 1 \n"; ;
            TraverseQueue.Enqueue(SourceID);
            while (TraverseQueue.Any())
            {
                CurrentActor = TraverseQueue.Dequeue();
                foreach (int Neighbour in Relations[CurrentActor].Keys)
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
            if (!ActorID.ContainsKey(Source) || !ActorID.ContainsKey(Target) || Source == Target)
            {
                return "Please Enter Correct Actor Names";
            }

            string Result = "";
            Dijkstra(Source, Target);
            Tuple<string, string> ShortestPath = GetPath(Source, Target);
            string ActorPath = ShortestPath.Item1;
            string MoviePath = ShortestPath.Item2;
            ActorState TargetResult = ActorStates[ActorID[Target]];
            Result += Source + "/" + Target + "\n";
            Result += "Path Length : " + TargetResult.DistanceFromSource + "   Total Frequency : " + TargetResult.Frequency + "\n";
            Result += "Chain Of Actors : " + ActorPath + "\n";
            Result += "Chain Of Movies : " + MoviePath + "\n\n";

            return Result;
        }

        //private static string BidirectionalSearch(string Source, string Target)
        //{

        //    if (Relations[Source].ContainsKey(Target))
        //    {
        //        ActorStates[Target].MarkVisted(1, Relations[Source][Target].GetFrequency(), Source);
        //        return Source + " -> " + Target;
        //    }

        //    ResetActorStates();
        //    Queue<string> ForwardQueue = new Queue<string>();
        //    Queue<string> BackwardQueue = new Queue<string>();
        //    ActorStates[Source].MarkVisted(0, 0, Source);
        //    ActorStates[Target].MarkVisted(0, 0, Target);
        //    ForwardQueue.Enqueue(Source);
        //    BackwardQueue.Enqueue(Target);
        //    string IntersectingActor = "";
        //    string ForwardParent = "", BackwardParent = "";
        //    int TotalDistance = 0, TotalFrequency = 0;
        //    int FIntersection = int.MaxValue;
        //    int Bintersection = int.MaxValue;
        //    bool Intersected = false;
        //    int MinimumDistance = int.MaxValue;
        //    int MaximumFrequency = -1;
        //    string CurrentIntersection, CurrentForwardParent, CurrentBackwardParent;

        //    while (ForwardQueue.Any() || BackwardQueue.Any())
        //    {
        //        if (ForwardQueue.Any() && ForwardQueue.Count > BackwardQueue.Count)
        //        {
        //            string CurrentActor = ForwardQueue.Dequeue();
        //            foreach (string Neighbour in Relations[CurrentActor].Keys)
        //            {

        //                if (BackwardQueue.Contains(Neighbour))
        //                {
        //                    if (!Intersected)
        //                    {
        //                        FIntersection = ActorStates[CurrentActor].DistanceFromSource + 1;
        //                        Intersected = true;
        //                    }

        //                    CurrentIntersection = Neighbour;
        //                    CurrentForwardParent = CurrentActor;
        //                    CurrentBackwardParent = ActorStates[Neighbour].Parent;
        //                    TotalDistance = ActorStates[CurrentBackwardParent].DistanceFromSource + ActorStates[CurrentForwardParent].DistanceFromSource + 2;
        //                    TotalFrequency = ActorStates[CurrentBackwardParent].Frequency + ActorStates[CurrentForwardParent].Frequency;
        //                    TotalFrequency += Relations[CurrentForwardParent][CurrentIntersection].GetFrequency();
        //                    TotalFrequency += Relations[CurrentBackwardParent][CurrentIntersection].GetFrequency();

        //                    if (TotalDistance <= MinimumDistance && TotalFrequency > MaximumFrequency)
        //                    {
        //                        IntersectingActor = CurrentIntersection;
        //                        ForwardParent = CurrentForwardParent;
        //                        BackwardParent = CurrentBackwardParent;
        //                        MinimumDistance = TotalDistance;
        //                        MaximumFrequency = TotalFrequency;
        //                    }                           
        //                }
        //                else if (!ActorStates[Neighbour].Visited)
        //                {
        //                    int Distance = ActorStates[CurrentActor].DistanceFromSource + 1;
        //                    int Frequency = ActorStates[CurrentActor].Frequency + Relations[CurrentActor][Neighbour].GetFrequency();
        //                    ActorStates[Neighbour].MarkVisted(Distance, Frequency, CurrentActor);
        //                    ForwardQueue.Enqueue(Neighbour);
        //                    if (Distance > FIntersection)
        //                    {
        //                        ForwardQueue.Clear();
        //                        break;
        //                    }
        //                }
        //                else if (ActorStates[CurrentActor].DistanceFromSource + 1 == ActorStates[Neighbour].DistanceFromSource)
        //                {
        //                    int AltFrequency = ActorStates[CurrentActor].Frequency + Relations[CurrentActor][Neighbour].GetFrequency();
        //                    if (AltFrequency > ActorStates[Neighbour].Frequency)
        //                    {
        //                        ActorStates[Neighbour].Frequency = AltFrequency;
        //                        ActorStates[Neighbour].Parent = CurrentActor;
        //                    }
        //                }
        //            }
        //        }
        //        else if (BackwardQueue.Any())
        //        {
        //            string CurrentActor = BackwardQueue.Dequeue();
        //            foreach (string Neighbour in Relations[CurrentActor].Keys)
        //            {

        //                if (ForwardQueue.Contains(Neighbour))
        //                {
        //                    if (!Intersected)
        //                    {
        //                        Bintersection = ActorStates[CurrentActor].DistanceFromSource + 1;
        //                        Intersected = true;
        //                    }

        //                    CurrentIntersection = Neighbour;
        //                    CurrentForwardParent = ActorStates[Neighbour].Parent;
        //                    CurrentBackwardParent = CurrentActor;
        //                    TotalDistance = ActorStates[CurrentBackwardParent].DistanceFromSource + ActorStates[CurrentForwardParent].DistanceFromSource + 2;
        //                    TotalFrequency = ActorStates[CurrentBackwardParent].Frequency + ActorStates[CurrentForwardParent].Frequency;
        //                    TotalFrequency += Relations[CurrentForwardParent][CurrentIntersection].GetFrequency();
        //                    TotalFrequency += Relations[CurrentBackwardParent][CurrentIntersection].GetFrequency();

        //                    if (TotalDistance <= MinimumDistance && TotalFrequency > MaximumFrequency)
        //                    {
        //                        IntersectingActor = CurrentIntersection;
        //                        ForwardParent = CurrentForwardParent;
        //                        BackwardParent = CurrentBackwardParent;
        //                        MinimumDistance = TotalDistance;
        //                        MaximumFrequency = TotalFrequency;
        //                    }
        //                }
        //                else if (!ActorStates[Neighbour].Visited)
        //                {
        //                    int Distance = ActorStates[CurrentActor].DistanceFromSource + 1;
        //                    int Frequency = ActorStates[CurrentActor].Frequency + Relations[CurrentActor][Neighbour].GetFrequency();
        //                    ActorStates[Neighbour].MarkVisted(Distance, Frequency, CurrentActor);
        //                    BackwardQueue.Enqueue(Neighbour);
        //                    if (Distance > Bintersection)
        //                    {
        //                        BackwardQueue.Clear();
        //                        break;
        //                    }
        //                }
        //                else if (ActorStates[CurrentActor].DistanceFromSource + 1 == ActorStates[Neighbour].DistanceFromSource)
        //                {
        //                    int AltFrequency = ActorStates[CurrentActor].Frequency + Relations[CurrentActor][Neighbour].GetFrequency();
        //                    if (AltFrequency > ActorStates[Neighbour].Frequency)
        //                    {
        //                        ActorStates[Neighbour].Frequency = AltFrequency;
        //                        ActorStates[Neighbour].Parent = CurrentActor;
        //                    }
        //                }

        //            }
        //        }

        //    }


        //    ActorStates[Target].DistanceFromSource = MinimumDistance;
        //    ActorStates[Target].Frequency = MaximumFrequency;
        //    string Actor = ForwardParent, Path = IntersectingActor;
        //    return "--";
        //    Path = Actor + " -> " + Path;
        //    string Parent;
        //    while (Actor != Source)
        //    {
        //        Parent = ActorStates[Actor].Parent;
        //        Path = Parent + " -> " + Path;
        //        Actor = Parent;
        //    }

        //    Actor = BackwardParent;
        //    Path += " - > " + Actor;
        //    while (Actor != Target)
        //    {
        //        Parent = ActorStates[Actor].Parent;
        //        Path += " - > " + Parent;
        //        Actor = Parent;
        //    }
        //    return Path;
        //}

        private static void BFS(string Source, string Target)
        {
            int SourceID = ActorID[Source], TargetID = ActorID[Target];
            if (Relations[SourceID].ContainsKey(TargetID))
            {
                ActorStates[TargetID].MarkVisted(1, Relations[SourceID][TargetID].GetFrequency(), SourceID);
                return;
            }

            ResetActorStates();
            ActorStates[SourceID].MarkVisted(0, 0, -1);

            Queue<int> TraverseQueue = new Queue<int>();
            int CurrentActor;
            TraverseQueue.Enqueue(SourceID);
            while (TraverseQueue.Any())
            {
                CurrentActor = TraverseQueue.Dequeue();
                foreach (int Neighbour in Relations[CurrentActor].Keys)
                {
                    if (!ActorStates[Neighbour].Visited)
                    {
                        int Distance = ActorStates[CurrentActor].DistanceFromSource + 1;
                        int Frequency = ActorStates[CurrentActor].Frequency + Relations[CurrentActor][Neighbour].GetFrequency();
                        ActorStates[Neighbour].MarkVisted(Distance, Frequency, CurrentActor);
                        TraverseQueue.Enqueue(Neighbour);
                        if (Distance > ActorStates[TargetID].DistanceFromSource)
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

            int SourceID = ActorID[Source], TargetID = ActorID[Target];

            ResetActorStates();
            SimplePriorityQueue<int> DijkstraQueue = new SimplePriorityQueue<int>();
            ActorStates[SourceID].MarkVisted(0, 0, -1);
            DijkstraQueue.Enqueue(SourceID, -1);
            int CurrentActor;
            while (DijkstraQueue.Any())
            {
                CurrentActor = DijkstraQueue.Dequeue();
                foreach (int Neighbour in Relations[CurrentActor].Keys)
                {
                    int Distance = ActorStates[CurrentActor].DistanceFromSource + 1;
                    int Frequency = ActorStates[CurrentActor].Frequency + Relations[CurrentActor][Neighbour].GetFrequency();
                    if (!ActorStates[Neighbour].Visited && Frequency > ActorStates[Neighbour].Frequency)
                    {
                        ActorStates[Neighbour].MarkVisted(Distance, Frequency, CurrentActor);
                        DijkstraQueue.Enqueue(Neighbour, -ActorStates[Neighbour].Frequency);
                    }
                    if (Neighbour == TargetID)
                    {
                        DijkstraQueue.Clear();
                        break;
                    }
                }
            }
        }

        public static string MST(string Source)
        {
            if (!ActorID.ContainsKey(Source))
            {
                return "Please Enter Correct Actor Names";
            }

            ResetActorStates();
            int SourceID = ActorID[Source];
            ActorStates[SourceID].MarkVisted(0, 0, -1);

            Queue<int> TraverseQueue = new Queue<int>();
            int CurrentActor;
            string Result = "";
            TraverseQueue.Enqueue(SourceID);
            HashSet<int> MST = new HashSet<int>();
            while (TraverseQueue.Any())
            {
                CurrentActor = TraverseQueue.Dequeue();
                foreach (int Neighbour in Relations[CurrentActor].Keys)
                {
                    if (!ActorStates[Neighbour].Visited)
                    {
                        if (!MST.Contains(Relations[CurrentActor][Neighbour].CommonMovie))
                        {
                            MST.Add(Relations[CurrentActor][Neighbour].CommonMovie);
                            Result += MovieName[Relations[CurrentActor][Neighbour].CommonMovie] + "\n";
                        }
                        int Distance = ActorStates[CurrentActor].DistanceFromSource + 1;
                        int Frequency = ActorStates[CurrentActor].Frequency + Relations[CurrentActor][Neighbour].GetFrequency();
                        ActorStates[Neighbour].MarkVisted(Distance, Frequency, CurrentActor);
                        TraverseQueue.Enqueue(Neighbour);
                    }
                }
            }
            return MST.Count + " \n" + Result;
        }

        public static void ClearGraph()
        {
            ActorStates.Clear();
            Relations.Clear();
            ActorName.Clear();
            ActorID.Clear();
            MovieName.Clear();
        }

        public static string[] ReadGraph(string FileName)
        {
            ClearGraph();
            Stopwatch MyWatch = new Stopwatch();
            MyWatch.Start();
            MyDataFile = new FileStream(FileName, FileMode.Open);
            MyReader = new StreamReader(MyDataFile);
            string[] Actors;
            int ActorI = 0,ActorJ = 0;
            while (MyReader.Peek() != -1)
            {
                // Read a movie then split it by '/' where Actors[0] is the movie name 
                Actors = MyReader.ReadLine().Split('/');
                int ActorsPerMovie = Actors.Length;

                NumberOfMovies++;
                MovieName.Add(NumberOfMovies, Actors[0]);

                // Start From Fisrt Actor
                for (int i = 1; i < ActorsPerMovie; i++)
                {
                    // if the actor is not in the Relations Graph  add it to both Relations and ActorStates for later use in BFS 
                    if (!ActorID.ContainsKey(Actors[i]))
                    {
                        NumberOfActors++;
                        ActorID.Add(Actors[i], NumberOfActors);
                        ActorName.Add(NumberOfActors, Actors[i]);
                        ActorStates.Add(NumberOfActors, new ActorState());
                        Relations.Add(NumberOfActors, new Dictionary<int, CommonInfo>());
                        ActorI = NumberOfActors;
                    }
                    else
                    {
                        ActorI = ActorID[Actors[i]];
                    }
                    
                    // Add to each actor in Relations the actors he acted with in the Current Movie (undirected-both ways)
                    // if a pair of actors acted together before just increase the frequency and add the movie name
                    for (int j = i + 1; j < ActorsPerMovie; j++)
                    {

                        if (!ActorID.ContainsKey(Actors[j]))
                        {
                            NumberOfActors++;
                            ActorID.Add(Actors[j], NumberOfActors);
                            ActorName.Add(NumberOfActors, Actors[j]);
                            ActorStates.Add(NumberOfActors, new ActorState());
                            Relations.Add(NumberOfActors, new Dictionary<int, CommonInfo>());
                            ActorJ = NumberOfActors;
                        }
                        else
                        {
                            ActorJ = ActorID[Actors[j]];
                        }

                        if (!Relations[ActorI].ContainsKey(ActorJ))
                        {
                            Relations[ActorI].Add(ActorJ, new CommonInfo());
                            Relations[ActorI][ActorJ].SetCommonMovie(NumberOfMovies);
                        }

                        Relations[ActorI][ActorJ].IncreaseFrequency();


                        if (!Relations[ActorJ].ContainsKey(ActorI))
                        {
                            Relations[ActorJ].Add(ActorI, new CommonInfo());
                            Relations[ActorJ][ActorI].SetCommonMovie(NumberOfMovies);
                        }

                        Relations[ActorJ][ActorI].IncreaseFrequency();

                    }
                }
            }

            MyWatch.Stop();
            MyReader.Dispose();
            MyDataFile.Dispose();
            return ActorID.Keys.ToArray();
        }

    }

}
