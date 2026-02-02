using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadGridGenerator : MonoBehaviour
{
    [SerializeField] private List<RoadPiece> roadPrefabs;
    [SerializeField] private int maxSegment;
    [SerializeField] private float cellSize;

    void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid() {
        Vector2Int gridSize = CalculateGridSize(maxSegment);

        int index = 0;
        for (int x = 0; x < gridSize.x; x++) {
            for (int z = 0; z < gridSize.y; z++){
                if (index >= maxSegment) {
                    return;
                }

                RoadPiece prefab = roadPrefabs[Random.Range(0, roadPrefabs.Count)];
                Vector3 pos = new Vector3(x * cellSize, 0, z * cellSize);

                Instantiate(prefab, pos, Quaternion.identity, transform);
                index++;
            }
        }
    }

    private Vector2Int CalculateGridSize(int segments) {
        int bestWidth = 1;
        int bestHeight = segments;
        int bestDiff = segments;

        for (int width = 1; width*width <= segments; width++) {
            if (segments % width != 0) {
                continue;
            }

            int height = segments / width;
            int diff = Mathf.Abs(width - height);

            if (diff < bestDiff) {
                bestDiff = diff;
                bestWidth = width;
                bestHeight = height;
            }
        }
        
        return new Vector2Int(bestWidth, bestHeight);
    }
}
