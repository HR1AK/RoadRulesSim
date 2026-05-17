using System.Collections.Generic;
using UnityEngine;

public class RoutePlanner : MonoBehaviour
{
    private RoadNode[] cachedNodes;

    public void RefreshNodes()
    {
        cachedNodes = FindObjectsOfType<RoadNode>();
    }

    public RoadNode FindNearestNode(Vector3 position, bool requireOutgoing = true)
    {
        if (cachedNodes == null || cachedNodes.Length == 0)
            RefreshNodes();

        RoadNode bestNode = null;
        float bestDistance = float.MaxValue;

        foreach (RoadNode node in cachedNodes)
        {
            if (node == null)
                continue;

            if (requireOutgoing && (node.outgoingNodes == null || node.outgoingNodes.Count == 0))
                continue;

            float distance = Vector3.Distance(position, node.transform.position);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestNode = node;
            }
        }

        return bestNode;
    }

    public RoadNode FindFarthestReachableNode(RoadNode startNode, float minRouteDistance = 40f)
    {
        if (startNode == null)
            return null;

        if (cachedNodes == null || cachedNodes.Length == 0)
            RefreshNodes();

        Dictionary<RoadNode, RoadNode> previous;
        Dictionary<RoadNode, float> distances = CalculateDistances(startNode, out previous);

        RoadNode bestNode = null;
        float bestDistance = 0f;

        foreach (var pair in distances)
        {
            RoadNode node = pair.Key;
            float distance = pair.Value;

            if (node == null || node == startNode)
                continue;

            if (float.IsInfinity(distance))
                continue;

            float worldDistance = Vector3.Distance(
                startNode.transform.position,
                node.transform.position
            );

            if (worldDistance < minRouteDistance)
                continue;

            if (distance > bestDistance)
            {
                bestDistance = distance;
                bestNode = node;
            }
        }

        // Если слишком далёкой точки нет — берём любую самую дальнюю достижимую
        if (bestNode == null)
        {
            foreach (var pair in distances)
            {
                RoadNode node = pair.Key;
                float distance = pair.Value;

                if (node == null || node == startNode)
                    continue;

                if (float.IsInfinity(distance))
                    continue;

                if (distance > bestDistance)
                {
                    bestDistance = distance;
                    bestNode = node;
                }
            }
        }

        return bestNode;
    }

    public List<RoadNode> FindPath(RoadNode startNode, RoadNode goalNode)
    {
        if (startNode == null || goalNode == null)
            return null;

        if (cachedNodes == null || cachedNodes.Length == 0)
            RefreshNodes();

        Dictionary<RoadNode, RoadNode> previous;
        CalculateDistances(startNode, out previous);

        if (startNode != goalNode && !previous.ContainsKey(goalNode))
        {
            Debug.LogWarning("Маршрут не найден.");
            return null;
        }

        List<RoadNode> path = new List<RoadNode>();
        RoadNode current = goalNode;

        path.Add(current);

        while (current != startNode)
        {
            if (!previous.TryGetValue(current, out RoadNode prev))
            {
                Debug.LogWarning("Не удалось восстановить маршрут.");
                return null;
            }

            current = prev;
            path.Add(current);
        }

        path.Reverse();
        return path;
    }

    private Dictionary<RoadNode, float> CalculateDistances(
        RoadNode startNode,
        out Dictionary<RoadNode, RoadNode> previous
    )
    {
        Dictionary<RoadNode, float> distances = new Dictionary<RoadNode, float>();
        previous = new Dictionary<RoadNode, RoadNode>();

        List<RoadNode> unvisited = new List<RoadNode>();

        foreach (RoadNode node in cachedNodes)
        {
            if (node == null)
                continue;

            distances[node] = float.PositiveInfinity;
            unvisited.Add(node);
        }

        distances[startNode] = 0f;

        while (unvisited.Count > 0)
        {
            RoadNode current = null;
            float bestDistance = float.PositiveInfinity;

            foreach (RoadNode node in unvisited)
            {
                if (!distances.TryGetValue(node, out float distance))
                    continue;

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    current = node;
                }
            }

            if (current == null)
                break;

            if (float.IsInfinity(bestDistance))
                break;

            unvisited.Remove(current);

            if (current.outgoingNodes == null)
                continue;

            foreach (RoadNode neighbour in current.outgoingNodes)
            {
                if (neighbour == null)
                    continue;

                float edgeCost = Vector3.Distance(
                    current.transform.position,
                    neighbour.transform.position
                );

                float newDistance = distances[current] + edgeCost;

                if (!distances.ContainsKey(neighbour))
                    distances[neighbour] = float.PositiveInfinity;

                if (newDistance < distances[neighbour])
                {
                    distances[neighbour] = newDistance;
                    previous[neighbour] = current;
                }
            }
        }

        return distances;
    }
}