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

        gScore[start] = 0;
        fScore[start] = GetDistance(start, goal);
        openSet.Enqueue(start, fScore[start]);

        while (openSet.Count > 0)
        {
            Node current = openSet.Dequeue();

            // Check if ALL limbs reached the goal
            if (current.LeftHand == goal.LeftHand &&
                current.RightHand == goal.RightHand &&
                current.LeftFoot == goal.LeftFoot &&
                current.RightFoot == goal.RightFoot)
            {
                return ReconstructPath(cameFrom, current);
            }

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

    // Updated to compare all four limbs
    private static double GetDistance(Node a, Node b)
    {
        double lhDist = Math.Sqrt(Math.Pow(a.LeftHand.X - b.LeftHand.X, 2) + Math.Pow(a.LeftHand.Y - b.LeftHand.Y, 2));
        double rhDist = Math.Sqrt(Math.Pow(a.RightHand.X - b.RightHand.X, 2) + Math.Pow(a.RightHand.Y - b.RightHand.Y, 2));
        double lfDist = Math.Sqrt(Math.Pow(a.LeftFoot.X - b.LeftFoot.X, 2) + Math.Pow(a.LeftFoot.Y - b.LeftFoot.Y, 2));
        double rfDist = Math.Sqrt(Math.Pow(a.RightFoot.X - b.RightFoot.X, 2) + Math.Pow(a.RightFoot.Y - b.RightFoot.Y, 2));

        return lhDist + rhDist + lfDist + rfDist; // Sum of all distances
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
