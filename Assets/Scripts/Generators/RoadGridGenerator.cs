using System.Collections.Generic;
using UnityEngine;

using HealthbarGames;

public class RoadGridGenerator : MonoBehaviour
{
    [SerializeField] private List<RoadPiece> roadPrefabs;
    [SerializeField] private int maxSegment = 10;
    [SerializeField] private float cellSize = 10f;
    List<RealTrafficLight> spawnedLights = new List<RealTrafficLight>();
    [SerializeField] private TrafficLightManager trafficLightManager;

    private Grid grid;

    void Start()
    {
        GenerateGrid();
        trafficLightManager.AutoSetupPhases(spawnedLights);
    }

    private void GenerateGrid()
    {
        Vector2Int size = CalculateGridSize(maxSegment);

        int width = size.x;
        int height = size.y;

        grid = new Grid(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                Vector3 worldPos = new Vector3(x * cellSize, 0, y * cellSize);

                PlacedPiece placed = TryPlacePiece(cell);
                grid.Set(cell, placed);

                if (placed != null)
                {
                    Quaternion rot = Quaternion.Euler(0, placed.RotationSteps * 90f, 0);
                    RoadPiece instance = Instantiate(placed.Prefab, worldPos, rot, transform);

                    RealTrafficLight[] lights = instance.GetComponentsInChildren<RealTrafficLight>();
                    spawnedLights.AddRange(lights);
                }
            }
        }
    }

    private PlacedPiece TryPlacePiece(Vector2Int cell)
    {
        PlacedPiece left = grid.GetNeighbour(cell, Vector2Int.left);
        PlacedPiece down = grid.GetNeighbour(cell, Vector2Int.down);

        List<PlacedPiece> valid = new List<PlacedPiece>();

        foreach (var prefab in roadPrefabs)
        {
            for (int rot = 0; rot < 4; rot++)
            {
                if (IsValid(prefab, rot, left, down))
                {
                    valid.Add(new PlacedPiece(prefab, rot));
                }
            }
        }

        // Дебаг: сколько вариантов подходит для каждой клетки
        // Debug.Log($"Cell {cell} has {valid.Count} valid pieces");

        if (valid.Count == 0)
            return null;

        // weighted random
        int totalWeight = 0;
        foreach (var v in valid)
            totalWeight += v.Prefab.Weight;

        int rand = Random.Range(0, totalWeight);

        foreach (var v in valid)
        {
            rand -= v.Prefab.Weight;
            if (rand < 0)
                return v;
        }

        return valid[0];
    }

    private bool IsValid(RoadPiece piece,
                         int rotationSteps,
                         PlacedPiece left,
                         PlacedPiece down)
    {
        if (left != null)
        {
            if (GetSide(piece, rotationSteps, Vector2Int.left) !=
                GetSide(left.Prefab, left.RotationSteps, Vector2Int.right))
                return false;
        }

        if (down != null)
        {
            if (GetSide(piece, rotationSteps, Vector2Int.down) !=
                GetSide(down.Prefab, down.RotationSteps, Vector2Int.up))
                return false;
        }

        return true;
    }

    private RoadLaneType GetSide(RoadPiece piece, int rot, Vector2Int dir)
    {
        RoadLaneType n = piece.North;
        RoadLaneType e = piece.East;
        RoadLaneType s = piece.South;
        RoadLaneType w = piece.West;

        for (int i = 0; i < rot; i++)
        {
            RoadLaneType temp = n;
            n = w;
            w = s;
            s = e;
            e = temp;
        }

        if (dir == Vector2Int.up) return n;
        if (dir == Vector2Int.right) return e;
        if (dir == Vector2Int.down) return s;
        if (dir == Vector2Int.left) return w;

        return RoadLaneType.NoneLine;
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