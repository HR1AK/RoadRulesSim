using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadPiece : MonoBehaviour
{
    [Header("Sockets")]
    public RoadLaneType North;
    public RoadLaneType South;
    public RoadLaneType East;
    public RoadLaneType West;

    [SerializeField] private int weight = 1;
    public int Weight => weight;
}
