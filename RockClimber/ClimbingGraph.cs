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

    // Static method to build a graph from detected holds
    public static ClimbingGraph BuildGraph(List<System.Drawing.Rectangle> holds, double maxReach)
    {
        ClimbingGraph graph = new ClimbingGraph();
        Dictionary<System.Drawing.Rectangle, Node> nodeMap = new Dictionary<System.Drawing.Rectangle, Node>();

        // Create nodes for each hold
        foreach (var hold in holds)
        {
            Node node = new Node(hold);
            graph.AddNode(node);
            nodeMap[hold] = node;
        }

        // Create edges between reachable holds
        foreach (var nodeA in graph.Nodes)
        {
            foreach (var nodeB in graph.Nodes)
            {
                if (nodeA == nodeB) continue;

                double distance = GetDistance(nodeA.Hold, nodeB.Hold);
                if (distance <= maxReach)
                {
                    graph.AddEdge(nodeA, nodeB, distance);
                }
            }
        }

        return graph;
    }

    private static double GetDistance(System.Drawing.Rectangle holdA, System.Drawing.Rectangle holdB)
    {
        double x1 = holdA.X + holdA.Width / 2;
        double y1 = holdA.Y + holdA.Height / 2;
        double x2 = holdB.X + holdB.Width / 2;
        double y2 = holdB.Y + holdB.Height / 2;

        return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
    }

    public static List<Node> FindBestRoute(List<System.Drawing.Rectangle> holds, System.Drawing.Rectangle startHold, System.Drawing.Rectangle goalHold, double maxReach)
    {
        ClimbingGraph graph = BuildGraph(holds, maxReach);

        Node startNode = graph.Nodes.FirstOrDefault(n => n.Hold == startHold);
        Node goalNode = graph.Nodes.FirstOrDefault(n => n.Hold == goalHold);

        if (startNode == null || goalNode == null)
            throw new Exception("Start or goal hold not found in graph.");

        return Pathfinding.AStar(startNode, goalNode);
    }

    internal static List<Node> FindBestRoute(List<Rectangle> rectangles, Rectangle startHold, Rectangle secondStartHold, Rectangle endHold, double maxReach)
    {
        throw new NotImplementedException();
    }
}

public class Node
{
    public System.Drawing.Rectangle Hold { get; }
    public List<Edge> Connections { get; } = new List<Edge>();

    public Node(System.Drawing.Rectangle hold)
    {
        Hold = hold;
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


