using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Profiling;

namespace Nothke.AStar
{
    // A* needs only a WeightedGraph and a Vector2Int type L, and does *not*
    // have to be a grid. However, in the example code I am using a grid.
    public interface WeightedGraph<L>
    {
        double Cost(L a, L b);
        void FillNeighbors(ref List<L> list, L id);
        int TotalSize { get; }
    }

    public class SquareGrid : WeightedGraph<Vector2Int>
    {
        // Implementation notes: I made the fields public for convenience,
        // but in a real project you'll probably want to follow standard
        // style and make them private.

        public readonly Vector2Int[] DIRS = new[]
            {
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1)
        };

        public int width, height;
        public HashSet<Vector2Int> walls = new HashSet<Vector2Int>();
        public HashSet<Vector2Int> forests = new HashSet<Vector2Int>();

        public int TotalSize => width * height;

        public void AddWallsRect(RectInt rect)
        {
            for (int x = rect.x; x < rect.xMax; x++)
            {
                for (int y = rect.y; y < rect.yMax; y++)
                {
                    var p = new Vector2Int(x, y);
                    if (InBounds(p))
                        walls.Add(p);
                }
            }
        }

        public SquareGrid(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public bool InBounds(Vector2Int id)
        {
            return 0 <= id.x && id.x < width
                && 0 <= id.y && id.y < height;
        }

        public bool Passable(Vector2Int id)
        {
            return !walls.Contains(id);
        }

        public double Cost(Vector2Int a, Vector2Int b)
        {
            return forests.Contains(b) ? 5 : 1;
        }

        public void FillNeighbors(ref List<Vector2Int> list, Vector2Int id)
        {
            list.Clear();

            for (int i = 0; i < DIRS.Length; i++)
            {
                Vector2Int next = new Vector2Int(id.x + DIRS[i].x, id.y + DIRS[i].y);
                if (InBounds(next) && Passable(next))
                {
                    list.Add(next);
                }
            }
        }
    }

    public class PriorityQueue<T>
    {
        // I'm using an unsorted array for this example, but ideally this
        // would be a binary heap. There's an open issue for adding a binary
        // heap to the standard C# library: https://github.com/dotnet/corefx/issues/574
        //
        // Until then, find a binary heap class:
        // * https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
        // * http://visualstudiomagazine.com/articles/2012/11/01/priority-queues-with-c.aspx
        // * http://xfleury.github.io/graphsearch.html
        // * http://stackoverflow.com/questions/102398/priority-queue-in-net

        private List<(T, double)> elements = new List<(T, double)>();

        public int Count => elements.Count;

        public void Enqueue(T item, double priority)
        {
            elements.Add((item, priority));
        }

        public T Dequeue()
        {
            int bestIndex = 0;

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Item2 < elements[bestIndex].Item2)
                {
                    bestIndex = i;
                }
            }

            T bestItem = elements[bestIndex].Item1;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }

        public void Clear()
        {
            elements.Clear();
        }
    }

    /* NOTE about types: in the main article, in the Python code I just
     * use numbers for costs, heuristics, and priorities. In the C++ code
     * I use a typedef for this, because you might want int or double or
     * another type. In this C# code I use double for costs, heuristics,
     * and priorities. You can use an int if you know your values are
     * always integers, and you can use a smaller size number if you know
     * the values are always small. */

    public class AStar
    {
        public Dictionary<Vector2Int, Vector2Int> cameFrom;
        public Dictionary<Vector2Int, double> costSoFar;
        PriorityQueue<Vector2Int> frontier;
        List<Vector2Int> neighbors;

        // Note: a generic version of A* would abstract over Vector2Int and
        // also Heuristic
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double Heuristic(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        public void Search(WeightedGraph<Vector2Int> graph, Vector2Int start, Vector2Int goal, bool preferForward = false)
        {
            Profiler.BeginSample("AStar Search");

            cameFrom.Clear();
            costSoFar.Clear();
            frontier.Clear();

            frontier.Enqueue(start, 0);

            cameFrom[start] = start;
            costSoFar[start] = 0;

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current.Equals(goal))
                    break;

                graph.FillNeighbors(ref neighbors, current);

                if (preferForward)
                {
                    // Prefer going forward
                    Vector2Int last = cameFrom[current];
                    Vector2Int nextInDir = 2 * current - last;

                    for (int i = 0; i < neighbors.Count; i++)
                    {
                        if (neighbors[i] == nextInDir)
                        {
                            if (i == 0) break;

                            var temp = neighbors[0];
                            neighbors[0] = neighbors[i];
                            neighbors[i] = temp;
                            break;
                        }
                    }
                }

                for (int i = 0; i < neighbors.Count; i++)
                {
                    var next = neighbors[i];

                    double newCost = costSoFar[current]
                        + graph.Cost(current, next);
                    if (!costSoFar.ContainsKey(next)
                        || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        double priority = newCost + Heuristic(next, goal);
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;
                    }
                }
            }

            Profiler.EndSample();
        }

        public AStar(WeightedGraph<Vector2Int> graph)
        {
            ResetSize(graph.TotalSize);
            frontier = new PriorityQueue<Vector2Int>();
            neighbors = new List<Vector2Int>(4);
        }

        public void ResetSize(int totalGridSize)
        {
            cameFrom = new Dictionary<Vector2Int, Vector2Int>(totalGridSize);
            costSoFar = new Dictionary<Vector2Int, double>(totalGridSize);
        }

        public void FillPath(ref List<Vector2Int> list, Vector2Int start, Vector2Int goal)
        {
            Vector2Int current = goal;
            Vector2Int ptr;
            int c = 0;
            while (cameFrom.TryGetValue(current, out ptr) && current != start)
            {
                list.Add(ptr);
                current = ptr;

                c++;
                if (c > 1000)
                {
                    Debug.LogError("INFINITE LOOP");
                    break;
                }
            }

            list.Reverse();
        }
    }
}

