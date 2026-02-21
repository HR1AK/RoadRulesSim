using UnityEngine;

public class Grid
{
    private PlacedPiece[,] cells;

    public int Width { get; }
    public int Height { get; }

    public Grid(int width, int height)
    {
        Width = width;
        Height = height;

        cells = new PlacedPiece[width, height];
    }

    public void Set(Vector2Int pos, PlacedPiece piece)
    {
        cells[pos.x, pos.y] = piece;
    }

    public PlacedPiece Get(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= Width ||
            pos.y < 0 || pos.y >= Height)
            return null;

        return cells[pos.x, pos.y];
    }

    public PlacedPiece GetNeighbour(Vector2Int pos, Vector2Int dir)
    {
        return Get(pos + dir);
    }
}