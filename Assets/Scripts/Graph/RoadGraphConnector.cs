using System.Collections.Generic;
using UnityEngine;

public class RoadGraphConnector : MonoBehaviour
{
    [SerializeField]
    private float connectionDistance = 2f;

    public void BuildConnections()
    {
        RoadNode[] allNodes =
            FindObjectsOfType<RoadNode>();

        List<RoadNode> externalNodes = new();

        foreach (var node in allNodes)
        {
            if (node.isExternalNode)
            {
                externalNodes.Add(node);
            }
        }

        ConnectNodes(externalNodes);

        Debug.Log("Road graph connected!");
    }

    private void ConnectNodes(List<RoadNode> nodes)
    {
        foreach (var nodeA in nodes)
        {
            if (!nodeA.isExitNode)
                continue;

            foreach (var nodeB in nodes)
            {
                if (nodeA == nodeB)
                    continue;

                if (!nodeA.CanConnectTo(nodeB))
                    continue;

                float dist =
                    Vector3.Distance(
                        nodeA.transform.position,
                        nodeB.transform.position);

                if (dist > connectionDistance)
                    continue;

                // избегаем дубликатов
                if (!nodeA.outgoingNodes.Contains(nodeB))
                {
                    nodeA.outgoingNodes.Add(nodeB);
                }
            }
        }
    }
}