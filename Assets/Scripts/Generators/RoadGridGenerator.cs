using System.Collections.Generic;
using UnityEngine;

public class RoadGridGenerator : MonoBehaviour
{
    [SerializeField] private List<RoadPiece> roadPrefabs;
    [SerializeField] private int maxSegment;
    [SerializeField] private float cellSize;

    private Grid grid;

    void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        grid = new Grid(maxSegment);

        for (int x = 0; x < grid.Width; x++)
        {
            for (int z = 0; z < grid.Height; z++)
            {
                Vector2Int cell = new Vector2Int(x, z);
                Vector3 pos = new Vector3(x * cellSize, 0, z * cellSize);

                RoadPiece placed = TryPlacePiece(cell, pos);
                grid.Set(cell, placed);
            }
        }
    }

private RoadPiece TryPlacePiece(Vector2Int cell, Vector3 position)
{
    RoadPiece leftNeighbour = grid.GetNeighbour(cell, Vector2Int.left);
    RoadPiece downNeighbour = grid.GetNeighbour(cell, Vector2Int.down);

    Dictionary<RoadPiece, List<Quaternion>> validByPrefab =
        new Dictionary<RoadPiece, List<Quaternion>>();

    foreach (var prefab in roadPrefabs)
    {
        for (int i = 0; i < 4; i++)
        {
            Quaternion rotation = Quaternion.Euler(0, i * 90, 0);
            RoadPiece test = Instantiate(prefab, position, rotation, transform);

            if (IsValid(test, leftNeighbour, downNeighbour))
            {
                if (!validByPrefab.ContainsKey(prefab))
                    validByPrefab[prefab] = new List<Quaternion>();

                validByPrefab[prefab].Add(rotation);
            }

            Destroy(test.gameObject);
        }
    }

    if (validByPrefab.Count == 0)
        return null;

    // --- WEIGHTED RANDOM ---
    int totalWeight = 0;
    foreach (var pair in validByPrefab)
        totalWeight += pair.Key.Weight;

    int randomValue = Random.Range(0, totalWeight);
    RoadPiece chosenPrefab = null;

    foreach (var pair in validByPrefab)
    {
        randomValue -= pair.Key.Weight;
        if (randomValue < 0)
        {
            chosenPrefab = pair.Key;
            break;
        }
    }

    // случайный поворот выбранного prefab
    List<Quaternion> rotations = validByPrefab[chosenPrefab];
    Quaternion chosenRotation = rotations[Random.Range(0, rotations.Count)];

    return Instantiate(chosenPrefab, position, chosenRotation, transform);
}


    private bool IsValid(RoadPiece piece,
                         RoadPiece leftNeighbour,
                         RoadPiece downNeighbour)
    {
        if (leftNeighbour != null)
        {
            if (!Match(piece, Vector3.left,
                       leftNeighbour, Vector3.right))
                return false;
        }

        if (downNeighbour != null)
        {
            if (!Match(piece, Vector3.back,
                       downNeighbour, Vector3.forward))
                return false;
        }

        return true;
    }

    private bool Match(RoadPiece piece, Vector3 myDirection,
                       RoadPiece neighbour, Vector3 neighbourDirection)
    {
        RoadSocket mySocket = GetSocketFacing(piece, myDirection);
        RoadSocket neighbourSocket = GetSocketFacing(neighbour, neighbourDirection);

        if (mySocket == null || neighbourSocket == null)
            return false;

        return mySocket.LaneType == neighbourSocket.LaneType;
    }

    private RoadSocket GetSocketFacing(RoadPiece piece, Vector3 direction)
    {
        foreach (var socket in piece.GetSockets())
        {
            if (Vector3.Dot(socket.transform.forward.normalized,
                            direction.normalized) > 0.95f)
            {
                return socket;
            }
        }

        return null;
    }
}
