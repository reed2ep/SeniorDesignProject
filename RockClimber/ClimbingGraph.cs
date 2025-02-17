using Org.W3c.Dom;
using System;
using System.Collections.Generic;
using System.Drawing;

public class ClimbingGraph
{
    public List<Node> Nodes { get; set; } = new List<Node>();

    public void AddNode(Node node)
    {
        Nodes.Add(node);
    }

    public void AddEdge(Node from, Node to, double cost)
    {
        from.Connections.Add(new Edge(to, cost));
    }

    public static ClimbingGraph BuildGraph(List<Rectangle> holds, double maxReach)
    {
        ClimbingGraph graph = new ClimbingGraph();
        Dictionary<(Rectangle, Rectangle, Rectangle, Rectangle), Node> nodeMap = new();

        // Generate nodes only for reachable hold positions
        foreach (var lh in holds)
        {
            foreach (var rh in holds)
            {
                if (GetDistance(lh, rh) > maxReach) continue; // Hands must be within reach

                foreach (var lf in holds)
                {
                    if (GetDistance(lf, rh) > maxReach) continue; // Feet must be within reach of hands

                    foreach (var rf in holds)
                    {
                        if (GetDistance(rf, rh) > maxReach) continue; // Ensure reasonable positioning

                        // Create only valid node states
                        Node node = new Node(lh, rh, lf, rf);
                        graph.AddNode(node);
                        nodeMap[(lh, rh, lf, rf)] = node;
                    }
                }
            }
        }

        // Only connect nodes with a valid single-limb transition
        foreach (var nodeA in graph.Nodes)
        {
            foreach (var nodeB in graph.Nodes)
            {
                if (nodeA == nodeB) continue;

                double cost = GetTransitionCost(nodeA, nodeB, maxReach);
                if (cost >= 0)
                {
                    graph.AddEdge(nodeA, nodeB, cost);
                }
            }
        }
        foreach (var node in graph.Nodes)
        {
            Console.WriteLine($"Node Added: LH={node.LeftHand}, RH={node.RightHand}, LF={node.LeftFoot}, RF={node.RightFoot}");
        }
        return graph;
    }


    public static List<Node> FindBestRoute(
        List<Rectangle> holds,
        Rectangle leftHandStart,
        Rectangle rightHandStart,
        Rectangle leftFootStart,
        Rectangle rightFootStart,
        Rectangle leftHandEnd,
        Rectangle rightHandEnd,
        double maxReach)
    {
        ClimbingGraph graph = BuildGraph(holds, maxReach);

        Node startNode = graph.Nodes.FirstOrDefault(n =>
            n.LeftHand.Equals(leftHandStart) &&
            n.RightHand.Equals(rightHandStart) &&
            n.LeftFoot.Equals(leftFootStart) &&
            n.RightFoot.Equals(rightFootStart));


        Node goalNode = graph.Nodes.FirstOrDefault(n =>
            n.LeftHand.Equals(leftHandEnd) &&
            n.RightHand.Equals(rightHandEnd));

        if (startNode == null || goalNode == null)
            throw new Exception("Start or goal holds not found in graph.");

        return Pathfinding.AStar(startNode, goalNode);
    }


    private static double GetDistance(System.Drawing.Rectangle holdA, System.Drawing.Rectangle holdB)
    {
        double x1 = holdA.X + holdA.Width / 2;
        double y1 = holdA.Y + holdA.Height / 2;
        double x2 = holdB.X + holdB.Width / 2;
        double y2 = holdB.Y + holdB.Height / 2;

        return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
    }

    private static double GetTransitionCost(Node current, Node next, double maxReach)
    {
        int moveCount = 0;
        double cost = 0;

        if (current.LeftHand != next.LeftHand)
        {
            if (!IsReachable(current.LeftHand, next.LeftHand, maxReach)) return -1;
            moveCount++;
            cost += GetDistance(current.LeftHand, next.LeftHand);
        }

        if (current.RightHand != next.RightHand)
        {
            if (!IsReachable(current.RightHand, next.RightHand, maxReach)) return -1;
            moveCount++;
            cost += GetDistance(current.RightHand, next.RightHand);
        }

        if (current.LeftFoot != next.LeftFoot)
        {
            if (!IsReachable(current.LeftFoot, next.LeftFoot, maxReach)) return -1;
            moveCount++;
            cost += GetDistance(current.LeftFoot, next.LeftFoot);
        }

        if (current.RightFoot != next.RightFoot)
        {
            if (!IsReachable(current.RightFoot, next.RightFoot, maxReach)) return -1;
            moveCount++;
            cost += GetDistance(current.RightFoot, next.RightFoot);
        }

        if (moveCount > 1) return -1; // Only allow one limb movement at a time

        return cost;
    }

    private static bool IsReachable(Rectangle from, Rectangle to, double maxReach)
    {
        double distance = GetDistance(from, to);
        return distance <= maxReach; // Only allow movements within reach
    }
}

public class Node
{
    public Rectangle LeftHand { get; }
    public Rectangle RightHand { get; }
    public Rectangle LeftFoot { get; }
    public Rectangle RightFoot { get; }

    public List<Edge> Connections { get; } = new List<Edge>();

    public Node(Rectangle lh, Rectangle rh, Rectangle lf, Rectangle rf)
    {
        LeftHand = lh;
        RightHand = rh;
        LeftFoot = lf;
        RightFoot = rf;
    }
}

public class Edge
{
    public Node Target { get; }
    public double Cost { get; }

    public Edge(Node target, double cost)
    {
        Target = target;
        Cost = cost;
    }
}