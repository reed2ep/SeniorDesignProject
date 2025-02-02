using System;
using System.Collections.Generic;
using System.Linq;

public static class Pathfinding
{
    public static List<Node> AStar(Node start, Node goal)
    {
        var openSet = new PriorityQueue<Node, double>();
        var cameFrom = new Dictionary<Node, Node>();
        var gScore = new Dictionary<Node, double>();
        var fScore = new Dictionary<Node, double>();

        foreach (var node in start.Connections.Select(e => e.Target))
        {
            gScore[node] = double.MaxValue;
            fScore[node] = double.MaxValue;
        }

        gScore[start] = 0;
        fScore[start] = GetDistance(start, goal);
        openSet.Enqueue(start, fScore[start]);

        while (openSet.Count > 0)
        {
            Node current = openSet.Dequeue();

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            foreach (var edge in current.Connections)
            {
                Node neighbor = edge.Target;
                double tentativeGScore = gScore[current] + edge.Cost;

                if (tentativeGScore < gScore.GetValueOrDefault(neighbor, double.MaxValue))
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + GetDistance(neighbor, goal);
                    openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }
        return null;
    }

    private static double GetDistance(Node a, Node b)
    {
        return Math.Sqrt(Math.Pow(a.Hold.X - b.Hold.X, 2) + Math.Pow(a.Hold.Y - b.Hold.Y, 2));
    }

    private static List<Node> ReconstructPath(Dictionary<Node, Node> cameFrom, Node current)
    {
        List<Node> path = new List<Node> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }
}
