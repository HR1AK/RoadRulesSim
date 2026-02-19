using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    private int width;
    private int height;

    private Dictionary<Vector2Int, RoadPiece> cells = new();

    public int Width => width;
    public int Height => height;

    public Grid(int segmentsCount)
    {
        Vector2Int size = CalculateGridSize(segmentsCount);
        width = size.x;
        height = size.y;
    }

    public void Set(Vector2Int cell, RoadPiece piece)
    {
        cells[cell] = piece;
    }

    public bool TryGet(Vector2Int cell, out RoadPiece piece)
    {
        return cells.TryGetValue(cell, out piece);
    }

    public RoadPiece GetNeighbour(Vector2Int cell, Vector2Int direction)
    {
        cells.TryGetValue(cell + direction, out RoadPiece neighbour);
        return neighbour;
    }

    private Vector2Int CalculateGridSize(int segmentsCount)
    {
        int bestWidth = 1;
        int bestHeight = segmentsCount;
        int bestDiff = segmentsCount;

        for (int w = 1; w * w <= segmentsCount; w++)
        {
            if (segmentsCount % w != 0)
                continue;

            int h = segmentsCount / w;
            int diff = Mathf.Abs(w - h);

            if (diff < bestDiff)
            {
                bestDiff = diff;
                bestWidth = w;
                bestHeight = h;
            }
        }

        return new Vector2Int(bestWidth, bestHeight);
    }
}
