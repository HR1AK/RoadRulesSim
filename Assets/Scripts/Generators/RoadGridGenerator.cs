using System.Collections;
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

    private void GenerateGrid() {
        grid = new Grid(maxSegment);

        int index = 0;
        for (int x = 0; x < grid.x; x++) {
            for (int z = 0; z < grid.y; z++){
                RoadPiece prefab = roadPrefabs[Random.Range(0, roadPrefabs.Count)];
                Vector3 pos = new Vector3(x * cellSize, 0, z * cellSize);

                Instantiate(prefab, pos, Quaternion.identity, transform);

                index++;
            }
        }
    }

    private void TurnRoadPiece(RoadSocket currentSocket, RoadSocket nextSocket){
        Quaternion rotation = Quaternion.FromToRotation(
            nextSocket.Forward,
            -currentSocket.Forward
        );

        RoadPiece nextPiece = nextSocket.GetComponentInParent<RoadPiece>();
        nextPiece.transform.rotation = rotation * nextPiece.transform.rotation;
    }
}
