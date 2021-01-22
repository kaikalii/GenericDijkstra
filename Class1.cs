
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
            Graph<(double, double), int> graph = new();
            var a = graph.AddNode((2, 1));
            var b = graph.AddNode((3, 4));
            var c = graph.AddNode((5, 5));
            var d = graph.AddNode((5, 2));
            var e = graph.AddNode((7, 2));
            graph.AddEdge(a, b, 0);
            graph.AddEdge(b, c, 0);
            graph.AddEdge(b, e, 0);
            graph.AddEdge(c, d, 0);
            graph.AddEdge(d, e, 0);
            var res = Run(
                a,
                ni => graph.NeighborIndices(ni).Select(nei =>
                {
                    var from = graph[ni];
                    var to = graph[nei.Node];
                    var dist = Math.Sqrt(Math.Pow(to.Item1 - from.Item1, 2) + Math.Pow(to.Item2 - from.Item2, 2));
                    return (nei.Node, dist);
                }),
                index => index == d
            ) ?? throw new Exception("No path");
            var (path, cost) = res;
            Console.WriteLine($"cost: {cost}");
            Console.WriteLine("path:");
            foreach (var ni in path)
            {
                Console.WriteLine(graph[ni]);
            }
        }
    }
    public struct NodeIndex
    {
        internal int i;
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return i == ((NodeIndex)obj).i;
        }
        public static bool operator ==(NodeIndex left, NodeIndex right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(NodeIndex left, NodeIndex right)
        {
            return !(left == right);
        }
        public override int GetHashCode()
        {
            return i.GetHashCode();
        }
    }
    public struct EdgeIndex
    {
        internal int i;
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return i == ((EdgeIndex)obj).i;
        }
        public static bool operator ==(EdgeIndex left, EdgeIndex right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(EdgeIndex left, EdgeIndex right)
        {
            return !(left == right);
        }
        public override int GetHashCode()
        {
            return i.GetHashCode();
        }
    }
    internal class NodeEntry<N>
    {
        internal N node;
        internal List<Neighbor<NodeIndex, EdgeIndex>> neighbors;
    }
    public class Neighbor<N, E>
    {
        public E Edge { get; init; }
        public N Node { get; init; }
    }
    public class Graph<N, E>
    {
        private readonly List<NodeEntry<N>> nodes = new();
        private readonly List<E> edges = new();
        public bool Directed { get; init; } = false;

        public N this[NodeIndex i]
        {
            get => nodes[i.i].node;
            set => nodes[i.i].node = value;
        }
        public E this[EdgeIndex i]
        {
            get => edges[i.i];
            set => edges[i.i] = value;
        }
        public NodeIndex AddNode(N node)
        {
            var index = new NodeIndex { i = nodes.Count };
            var entry = new NodeEntry<N> { node = node, neighbors = new() };
            nodes.Add(entry);
            return index;
        }
        public EdgeIndex AddEdge(NodeIndex a, NodeIndex b, E edge)
        {
            var existingEdgeIndex = GetEdgeConnecting(a, b);
            switch (existingEdgeIndex)
            {
                case EdgeIndex edgeIndex:
                    this[edgeIndex] = edge;
                    return edgeIndex;
                case null:
                    var ei = new EdgeIndex { i = edges.Count };
                    edges.Add(edge);
                    nodes[a.i].neighbors.Add(new Neighbor<NodeIndex, EdgeIndex> { Node = b, Edge = ei });
                    return ei;
            }
        }
        public EdgeIndex? GetEdgeConnecting(NodeIndex a, NodeIndex b)
        {
            try
            {
                return nodes[a.i].neighbors.First(nei => nei.Node.Equals(b)).Edge;
            }
            catch
            {
                if (Directed)
                {
                    return null;
                }
                try
                {
                    return nodes[a.i].neighbors.First(nei => nei.Node.Equals(b)).Edge;
                }
                catch
                {
                    return null;
                }
            }
        }
        public IEnumerable<Neighbor<NodeIndex, EdgeIndex>> NeighborIndices(NodeIndex start)
        {
            return nodes[start.i].neighbors;
        }
        public IEnumerable<Neighbor<N, E>> NeighborWeights(NodeIndex start)
        {
            return nodes[start.i].neighbors.Select(nei => new Neighbor<N, E> { Node = this[nei.Node], Edge = this[nei.Edge] });
        }
    }
}
