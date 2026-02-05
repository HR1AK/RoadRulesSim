using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoadLaneType
{
    NoneLine = 0,
    OneLine = 1,
    TwoLine = 2,
    TwoWideLine = 4,
    FourLine = 8
}

public class RoadSocket : MonoBehaviour
{
    [Header("Socket settings")]
    [SerializeField] private RoadLaneType laneType;

    [SerializeField] private bool isOccupied = false;

    // Удобное свойство — направление сокета
    public Vector3 Forward => transform.forward;
    public RoadLaneType LaneType => laneType;
    public bool IsOccupied => isOccupied;

    
    public void SetOccupied(bool value)
    {
        isOccupied = value;
    }

    #if UNITY_EDITOR
    // Визуализация в сцене
    private void OnDrawGizmos()
    {
        Gizmos.color = isOccupied ? Color.red : Color.green;
        Gizmos.DrawSphere(transform.position, 0.3f);
        Gizmos.DrawLine(
            transform.position,
            transform.position + transform.forward * 1.5f
        );
    }
    #endif
}
