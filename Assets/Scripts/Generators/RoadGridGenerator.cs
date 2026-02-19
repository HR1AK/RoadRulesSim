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

        List<RoadPiece> validPieces = new List<RoadPiece>();

        foreach (var prefab in roadPrefabs)
        {
            for (int i = 0; i < 4; i++)
            {
                Quaternion rotation = Quaternion.Euler(0, i * 90, 0);

                RoadPiece test = Instantiate(prefab, position, rotation, transform);

                if (IsValid(test, leftNeighbour, downNeighbour))
                {
                    validPieces.Add(test); // сохраняем подходящий
                }
                else
                {
                    Destroy(test.gameObject);
                }
            }
        }

        Debug.Log("Valid count: " + validPieces.Count);
        if (validPieces.Count > 0)
        {
            Random.InitState(System.DateTime.Now.Millisecond);
            int randomIndex = Random.Range(0, validPieces.Count);
            RoadPiece chosen = validPieces[randomIndex];

            // удаляем остальные
            for (int i = 0; i < validPieces.Count; i++)
            {
                if (i != randomIndex)
                    Destroy(validPieces[i].gameObject);
            }

            return chosen;
        }

        // fallback
        return Instantiate(roadPrefabs[0], position, Quaternion.identity, transform);
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
