
using System;
using System.Collections.Generic;
using System.Linq;

namespace GenericDijkstra
{
    internal struct PathTo<T>
    {
        public T parent;
        public bool hasParent;
        public double cost;
    }
    public static class Diskstra
    {
        public static (List<T>, double)? Run<T>(
            T start,
            Func<T, IEnumerable<(T, double)>> successors,
            Func<T, bool> isEnd
        ) where T : notnull
        {
            HashSet<T> visited = new();
            Dictionary<T, PathTo<T>> paths = new() { { start, new PathTo<T> { cost = 0 } } };
            var curr = start;
            while (true)
            {
                var currPath = paths[curr];
                // Check for end
                if (isEnd(curr))
                {
                    var fullCost = currPath.cost;
                    var fullPath = new List<T> { curr };
                    while (currPath.hasParent)
                    {
                        fullPath.Add(currPath.parent);
                        currPath = paths[currPath.parent];
                    }
                    fullPath.Reverse();
                    return (fullPath, fullCost);
                }
                // Get all unvisited successors
                var curSuccessors = successors(curr).Where(succ => !visited.Contains(succ.Item1)).ToList();
                // Try to reduce the cost of successors
                foreach (var (succ, newCost) in curSuccessors)
                {
                    if (!paths.ContainsKey(succ)) paths[succ] = new PathTo<T> { cost = double.PositiveInfinity };
                    var totalCost = currPath.cost + newCost;
                    if (totalCost < paths[succ].cost)
                    {
                        paths[succ] = new PathTo<T> { parent = curr, cost = totalCost, hasParent = true };
                    }
                }
                // Add the curr node as visited
                visited.Add(curr);
                // Determine the next current node
                try
                {
                    var next = paths.Where(node => !visited.Contains(node.Key)).OrderBy(node => node.Value.cost).First();
                    if (next.Value.cost == double.PositiveInfinity)
                    {
                        return null;
                    }
                    curr = next.Key;

                }
                catch
                {
                    return null;
                }
            }
        }
        public static void Main()
        {
            var grid = new[] {
                new[] { 1, 2, 3, 4 },
                new[] { 5, 6, 7, 8 },
                new[] { 9, 10, 11, 12 },
                new[] { 13, 14, 15, 16 },
            };
            var res = Run(
                (0, 0),
                index =>
                {
                    var succ = new List<((int, int), double)>();
                    if (index.Item1 > 0) succ.Add(((index.Item1 - 1, index.Item2), 1));
                    if (index.Item1 < 3) succ.Add(((index.Item1 + 1, index.Item2), 1));
                    if (index.Item2 > 0) succ.Add(((index.Item1, index.Item2 - 1), 1));
                    if (index.Item2 < 3) succ.Add(((index.Item1, index.Item2 + 1), 1));
                    return succ;
                },
                index => index == (3, 4)
            ) ?? throw new Exception("No path");
            var (path, cost) = res;
            Console.WriteLine($"cost: {cost}");
            Console.WriteLine("path:");
            foreach (var node in path)
            {
                Console.WriteLine(grid[node.Item1][node.Item2]);
            }
        }
    }
}
