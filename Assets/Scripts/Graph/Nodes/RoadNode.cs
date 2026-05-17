using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RoadNode : MonoBehaviour
{
    [Header("Spawn")]
    public bool canSpawnPlayer;
    public static bool showDebug = true;

    public List<RoadNode> outgoingNodes = new List<RoadNode>();

    public bool isExternalNode;
    public int laneIndex;
    public bool isExitNode;

    public enum Direction
    {
        None,
        North,
        South,
        East,
        West
    }

    private void OnDrawGizmos()
{
    if (!showDebug) return;

    // точка
    Gizmos.color = Color.cyan;
    Gizmos.DrawSphere(transform.position, 0.4f);

#if UNITY_EDITOR
    // стрелки связей
    Handles.color = Color.green;

    foreach (var node in outgoingNodes)
    {
        if (node == null) continue;

        Vector3 from = transform.position;
        Vector3 to = node.transform.position;

        // линия
        Handles.DrawLine(from, to);

        // стрелка
        Vector3 dir = (to - from).normalized;

        Handles.ArrowHandleCap(
            0,
            from + dir * 0f,                 // позиция стрелки
            Quaternion.LookRotation(dir),     // куда смотрит
            1.8f,                              // размер
            EventType.Repaint
        );
    }
#endif
}

        public bool CanConnectTo(RoadNode other)
        {
            if (other == null)
                return false;

            if (!isExternalNode || !other.isExternalNode)
                return false;

            if (!isExitNode)
                return false;

            if (other.isExitNode)
                return false;

            if (laneIndex != other.laneIndex)
                return false;

            return true;
        }
}