using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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

    // Build a graph from detected holds.
    public static ClimbingGraph BuildGraph(List<Rectangle> holds, double maxReach)
    {
        ClimbingGraph graph = new ClimbingGraph();
        Dictionary<Rectangle, Node> nodeMap = new Dictionary<Rectangle, Node>();

        // Create a node for each hold.
        foreach (var hold in holds)
        {
            Node node = new Node(hold);
            graph.AddNode(node);
            nodeMap[hold] = node;
        }

        // Create edges between holds that are within reach.
        foreach (var nodeA in graph.Nodes)
        {
            foreach (var nodeB in graph.Nodes)
            {
                if (nodeA == nodeB) continue;

                double distance = GetDistance(nodeA.Hold, nodeB.Hold);
                if (distance <= maxReach)
                {
                    double cost = ComputeEdgeCost(distance, maxReach);
                    graph.AddEdge(nodeA, nodeB, cost);
                }
            }
        }

        return graph;
    }


    private static double GetDistance(Rectangle holdA, Rectangle holdB)
    {
        double x1 = holdA.X + holdA.Width / 2;
        double y1 = holdA.Y + holdA.Height / 2;
        double x2 = holdB.X + holdB.Width / 2;
        double y2 = holdB.Y + holdB.Height / 2;
        return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
    }

    /// <summary>
    /// Computes climbing paths for both hands and legs.
    /// Hands: from their start holds to finish holds.
    /// Legs: from their start holds to the same finish holds as their corresponding hand.
    /// If left-hand finish is not provided, both hands (and legs) use the right-hand finish.
    /// </summary>
    public static Dictionary<string, List<Node>> FindClimbingPaths(
        List<Rectangle> holds,
        Rectangle rightHandStart,
        Rectangle leftHandStart,
        Rectangle rightLegStart,
        Rectangle leftLegStart,
        Rectangle rightHandFinish,
        Rectangle? leftHandFinish,
        double maxReach)
    {
        ClimbingGraph graph = BuildGraph(holds, maxReach);

        Node rightHandStartNode = graph.Nodes.FirstOrDefault(n => n.Hold == rightHandStart);
        Node leftHandStartNode = graph.Nodes.FirstOrDefault(n => n.Hold == leftHandStart);
        Node rightLegStartNode = graph.Nodes.FirstOrDefault(n => n.Hold == rightLegStart);
        Node leftLegStartNode = graph.Nodes.FirstOrDefault(n => n.Hold == leftLegStart);
        Node rightHandFinishNode = graph.Nodes.FirstOrDefault(n => n.Hold == rightHandFinish);
        Node leftHandFinishNode = leftHandFinish.HasValue
            ? graph.Nodes.FirstOrDefault(n => n.Hold == leftHandFinish.Value)
            : rightHandFinishNode;

        if (rightHandStartNode == null || leftHandStartNode == null ||
            rightLegStartNode == null || leftLegStartNode == null ||
            rightHandFinishNode == null || leftHandFinishNode == null)
        {
            throw new Exception("One or more holds not found in graph.");
        }

        var paths = new Dictionary<string, List<Node>>();
        paths["RightHand"] = Pathfinding.AStar(rightHandStartNode, rightHandFinishNode);
        paths["LeftHand"] = Pathfinding.AStar(leftHandStartNode, leftHandFinishNode);
        paths["RightLeg"] = Pathfinding.AStar(rightLegStartNode, rightHandFinishNode);
        paths["LeftLeg"] = Pathfinding.AStar(leftLegStartNode, leftHandFinishNode);

        return paths;
    }

    private static double ComputeEdgeCost(double distance, double maxReach)
    {
        // Compute a penalty factor: if the move is near the maximum reach, add extra cost.
        double fraction = distance / maxReach;
        double penalty = 0.0;

        // If the move is more than 75% of max reach, add a penalty that grows as you get closer to maxReach.
        if (fraction > 0.75)
        {
            // You can adjust the multiplier to fine-tune how harsh the penalty is.
            penalty = (fraction - 0.75) * maxReach;
        }
        return distance + penalty;
    }
}

public class Node
{
    public Rectangle Hold { get; }
    public List<Edge> Connections { get; } = new List<Edge>();

    public Node(Rectangle hold)
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
