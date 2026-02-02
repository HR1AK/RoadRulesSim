using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadPiece : MonoBehaviour
{
    [Header("Sockets")]
    [SerializeField] private List<RoadSocket> sockets = new List<RoadSocket>();

    private void Awake()
    {
        // Автоматически собираем все соккеты внутри префаба
        sockets = GetComponentsInChildren<RoadSocket>().ToList();
    }

    /// <summary>
    /// Все соккеты этого куска
    /// </summary>
    public IReadOnlyList<RoadSocket> Sockets => sockets;

    /// <summary>
    /// Свободные соккеты
    /// </summary>
    public List<RoadSocket> GetFreeSockets()
    {
        return sockets.Where(s => !s.IsOccupied).ToList();
    }

    /// <summary>
    /// Свободные соккеты определённого типа
    /// </summary>
    public List<RoadSocket> GetFreeSocketsByLane(RoadLaneType laneType)
    {
        return sockets
            .Where(s => !s.IsOccupied && s.LaneType == laneType)
            .ToList();
    }

    /// <summary>
    /// Есть ли вообще свободные соккеты
    /// </summary>
    public bool HasFreeSockets()
    {
        return sockets.Any(s => !s.IsOccupied);
    }
}
