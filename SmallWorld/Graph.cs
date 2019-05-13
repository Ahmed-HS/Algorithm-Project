using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace AlgorithmProject
{
    // Common Inforamtion between a pair of actors
    class CommonInfo
    {
        public int Frequency { get; set; } // Θ(1)
        public int CommonMovie { get; set; } // Θ(1)

        // Θ(1)
        public CommonInfo()
        {
            Frequency = 0; 
        }

        // Θ(1)
        public void IncreaseFrequency()
        {
            Frequency++; 
        }

        // Θ(1)
        public void SetCommonMovie(int MovieName)
        {
            CommonMovie = MovieName;
        }
    }

    // Represents an edge between two actors
    class Edge
    {
        public int ActorS { get; set; } // Θ(1)
        public int ActorE { get; set; } // Θ(1)
        public int Frequency { get; set; } // Θ(1)

        // Θ(1)
        public Edge(int ActorS, int ActorE,int Frequency)
        {
            this.ActorS = ActorS; 
            this.ActorE = ActorE; 
            this.Frequency = Frequency; 
        }
    }

    // Disjoint set data structure
    class DisjointSet
    {
        int[] Array;
        int[] Size;

        // Θ(Count)
        public DisjointSet(int Count)
        {
            Array = new int[Count + 1];
            Size = new int[Count + 1];
            for (int i = 1; i <= Count; i++)
            {
                Array[i] = i;
                Size[i] = 1;
            }
        }

        // O(log(N))
        public void Union(int i, int j)
        {
            // O(log(N))
            int RootI = GetRoot(i);
            int RootJ = GetRoot(j);
            
            if (Size[i] < Size[j])
            {
                Array[RootI] = Array[RootJ];
                Size[RootJ] += Size[RootI]; 
            }
            else
            {
                Array[RootJ] = Array[RootI]; 
                Size[RootI] += Size[RootJ]; 
            }
        }

        public bool SameSet(int i, int j)
        {
            return GetRoot(i) == GetRoot(j);
        }

        // O(log(N))
        public int GetRoot(int i)
        {
            // O(log(N))
            while (Array[i] != i)
            {
                Array[i] = Array[Array[i]]; // O(log(N))
                i = Array[i]; // O(log(N))
            }
            return i;
        }
    }

    // State of each actor ,to be later used in shortest path
    class ActorState
    {
        public bool Visited { get; set; } // Θ(1);
        public int DistanceFromSource { get; set; } // Θ(1)
        public int  Frequency { get; set; } // Θ(1)
        public int Parent { get; set; } // Θ(1)

        // Θ(1)
        public ActorState()
        {
            Visited = false;
            DistanceFromSource = int.MaxValue;
            Frequency = 0;
            Parent = -1;
        }

        // Θ(1)
        public void MarkVisted(int Distance, int NewFrequency, int CurrentParent)
        {
            Visited = true;
            DistanceFromSource = Distance;
            Frequency = NewFrequency;
            Parent = CurrentParent;
        }

        // Θ(1)
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

        // Θ(1)
        static Graph()
        {
            Relations = new Dictionary<int, Dictionary<int, CommonInfo>>();
            ActorStates = new Dictionary<int, ActorState>();
            ActorID = new Dictionary<string, int>();
            ActorName = new Dictionary<int, string>();
            MovieName = new Dictionary<int, string>();
        }

        // Θ(V)
        private static void ResetActorStates()
        {
            // Reset each Actor's State
            foreach (ActorState CurrentActor in ActorStates.Values)
            {
                CurrentActor.Reset();
            }
        }

        //O(Q*(V + E))
        public static async Task<string> ReadQueries(StorageFile QueriesFile)
        {
            //Θ(Q)
            IList<string> Queries = await Windows.Storage.FileIO.ReadLinesAsync(QueriesFile);

            string[] CurrentQuery;
            string Source, Target, QueriesResult = "";

            //Θ(Q)
            for (int i = 0; i < Queries.Count; i++)
            {
                CurrentQuery = Queries[i].Split('/');
                Source = CurrentQuery[0];
                Target = CurrentQuery[1];
                QueriesResult += GetTwoActorsRelation(Source, Target);  //O(V + E)

            }
            return QueriesResult;

        }

        //Θ(P)
        private static Tuple<string, string> GetPath(string Source, string Target)
        {
            int SourceID = ActorID[Source], TargetID = ActorID[Target];
            int Parent, CurrentActor = TargetID;
            string MoviePath = "", ActorPath = Target;

            if (ActorStates[TargetID].Parent == -1)
            {
                return new Tuple<string, string>("No Path Found", "No Path Found");
            }

            //Θ(P)
            while (CurrentActor != SourceID)
            {
                Parent = ActorStates[CurrentActor].Parent;
                MoviePath = MovieName[Relations[CurrentActor][Parent].CommonMovie] + " => " + MoviePath;
                ActorPath = ActorName[Parent] + " -> " + ActorPath;
                CurrentActor = Parent;
            }

            return new Tuple<string, string>(ActorPath, MoviePath);
        }

        //O(V + E)
        public static string GetTwoActorsRelation(string Source, string Target)
        {
            if (!ActorID.ContainsKey(Source) || !ActorID.ContainsKey(Target) || Source == Target)
            {
                return "Please Enter Correct Actor Names";
            }

            string Result = "";
            BFS(Source, Target); //O(V + E)
            Tuple<string, string> ShortestPath = GetPath(Source, Target); //O(P)
            string ActorPath = ShortestPath.Item1;
            string MoviePath = ShortestPath.Item2;

            ActorState TargetResult = ActorStates[ActorID[Target]];

            Result += Source + "/" + Target + "\n";
            Result += "DoS = " + TargetResult.DistanceFromSource + ", RS = " + TargetResult.Frequency + "\n";
            Result += "Chain Of Actors : " + ActorPath + "\n";
            Result += "Chain Of Movies : " + MoviePath + "\n\n";
            return Result;

        }

        // O((Log N) * (V + E))
        public static string DistributionOfShotestPath(string Source)
        {
            if (!ActorID.ContainsKey(Source))
            {
                return "Please Enter Correct Actor Names";
            }

            int SourceID = ActorID[Source];

            ResetActorStates(); // Θ(V)

            ActorStates[SourceID].MarkVisted(0, 0, -1);

            Queue<int> TraverseQueue = new Queue<int>();
            SortedDictionary<int, int> Distribution = new SortedDictionary<int, int>();

            int CurrentActor;
            string Result = "Shortest Path \t\t\t Frequency \n0 \t\t\t\t 1 \n"; ;

            TraverseQueue.Enqueue(SourceID);

            // O(V)
            while (TraverseQueue.Any())
            {
                CurrentActor = TraverseQueue.Dequeue();
                foreach (int Neighbour in Relations[CurrentActor].Keys) // Θ(Neighbour)
                {
                    if (!ActorStates[Neighbour].Visited) // O(E)
                    {
                        int Distance = ActorStates[CurrentActor].DistanceFromSource + 1;
                        int Frequency = ActorStates[CurrentActor].Frequency + Relations[CurrentActor][Neighbour].Frequency;
                        ActorStates[Neighbour].MarkVisted(Distance, Frequency, CurrentActor);
                        TraverseQueue.Enqueue(Neighbour);

                        if (Distribution.ContainsKey(ActorStates[Neighbour].DistanceFromSource)) // Θ(log(N))
                        {
                            Distribution[ActorStates[Neighbour].DistanceFromSource]++; // Θ(log(N))
                        }
                        else
                        {
                            Distribution.Add(ActorStates[Neighbour].DistanceFromSource, 1); // Θ(log(N))
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

        // O(V + E)
        private static void BFS(string Source, string Target)
        {
            int SourceID = ActorID[Source], TargetID = ActorID[Target];
            if (Relations[SourceID].ContainsKey(TargetID))
            {
                ActorStates[TargetID].MarkVisted(1, Relations[SourceID][TargetID].Frequency, SourceID);
                return;
            }

            ResetActorStates(); // Θ(V)
            ActorStates[SourceID].MarkVisted(0, 0, -1);

            Queue<int> TraverseQueue = new Queue<int>();

            int CurrentActor;
            TraverseQueue.Enqueue(SourceID);

            // O(V)
            while (TraverseQueue.Any())
            {
                CurrentActor = TraverseQueue.Dequeue();
                foreach (int Neighbour in Relations[CurrentActor].Keys) // Θ(Neighbour)
                {
                    if (!ActorStates[Neighbour].Visited) // O(E)
                    {
                        int Distance = ActorStates[CurrentActor].DistanceFromSource + 1;
                        int Frequency = ActorStates[CurrentActor].Frequency + Relations[CurrentActor][Neighbour].Frequency;
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
                        int AltFrequency = ActorStates[CurrentActor].Frequency + Relations[CurrentActor][Neighbour].Frequency;
                        if (AltFrequency > ActorStates[Neighbour].Frequency)
                        {
                            ActorStates[Neighbour].Frequency = AltFrequency;
                            ActorStates[Neighbour].Parent = CurrentActor;
                        }
                    }
                }
            }
        }

        public static string MST()
        {
            string Result = "";
            int Count = 0;
            DisjointSet ActorSet = new DisjointSet(NumberOfActors); // Θ(V)
            HashSet<int> Movies = new HashSet<int>();
            List<Edge> AllEdges = new List<Edge>();

            foreach (int Actor in Relations.Keys) // Θ(V)
            {
                foreach (int Neighbour in Relations[Actor].Keys) // Θ(Neighbour)
                {
                    Edge CurrentEdge = new Edge(Actor, Neighbour, Relations[Actor][Neighbour].Frequency); // Θ(E)
                    AllEdges.Add(CurrentEdge);
                }
            }

            AllEdges.Sort((x, y) => x.Frequency.CompareTo(y.Frequency)); // Average Case O(N*Log(N)) - Worst Case // O(N^2)

            foreach (Edge Currentedge in AllEdges)
            {
                if (!ActorSet.SameSet(Currentedge.ActorS, Currentedge.ActorE)) // O(log(N))
                {
                    if (!Movies.Contains(Relations[Currentedge.ActorS][Currentedge.ActorE].CommonMovie)) // O(1) with constant factor
                    {
                        Movies.Add(Relations[Currentedge.ActorS][Currentedge.ActorE].CommonMovie); // O(1) with constant factor
                        Result += MovieName[Relations[Currentedge.ActorS][Currentedge.ActorE].CommonMovie] + "\n";
                    }

                    ActorSet.Union(Currentedge.ActorS, Currentedge.ActorE); // O(log(N))
                    Count++;
                }

                if (Count == NumberOfActors - 1)
                {
                    break;
                }
            }

            return Movies.Count + "\n" + Result;
        }

        // Θ(1)
        public static void ClearGraph()
        {
            NumberOfActors = 0;
            NumberOfMovies = 0;
            ActorStates.Clear();
            Relations.Clear();
            ActorName.Clear();
            ActorID.Clear();
            MovieName.Clear();
        }

        // Θ(Movies*(ActorsPerMovie^2))
        public static async Task<string[]> ReadGraph(StorageFile FileName)
        {
            ClearGraph(); // Θ(1)
            IList<string> MoviesFile = await Windows.Storage.FileIO.ReadLinesAsync(FileName); // Θ(Movies)
            string[] Actors;
            int ActorI = 0,ActorJ = 0;

            // Θ(Movies)
            for (int k = 0; k < MoviesFile.Count; k++)
            {
                // Read a movie then split it by '/' where Actors[0] is the movie name 
                Actors = MoviesFile[k].Split('/'); // Θ(Length)
                int ActorsPerMovie = Actors.Length;

                NumberOfMovies++;
                MovieName.Add(NumberOfMovies, Actors[0]);

                // Start From Fisrt Actor  // Θ(ActorsPerMovie)
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
                    // Θ(ActorsPerMovie - 1)
                    for (int j = i + 1; j < ActorsPerMovie; j++)
                    {
                        // Θ(M*(A^2))
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

            return ActorID.Keys.ToArray(); // Θ(V)
        }

    }

}
