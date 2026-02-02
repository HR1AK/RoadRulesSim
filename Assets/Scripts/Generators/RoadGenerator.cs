using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RoadGenerator : MonoBehaviour
{
    [SerializeField] private List<RoadPiece> roadSegmentPrefabs;
    [SerializeField] private int maxSegment = 20;
    [SerializeField] private List<RoadPiece> spawnedRoadSegmentPrefabs;

    private void Start() {
        SpawnRootSegment();

        for (int i = 0; i < maxSegment - 1; i++) {
            if (!TrySpawnNextSegment()) {
                break;
            }
        }
    }

    private RoadPiece GetRandomRoadSegment() {
        int randomIndex = Random.Range(0, roadSegmentPrefabs.Count);
        return roadSegmentPrefabs[randomIndex];
    }

    private void SpawnRootSegment() {
        RoadPiece roadSegment = GetRandomRoadSegment();
        RoadPiece spawnedSegment = Instantiate(roadSegment, Vector3.zero, Quaternion.identity);

        spawnedRoadSegmentPrefabs.Add(spawnedSegment);
    }

    // private RoadSocket GetRandomFreeSocket(RoadPiece piece){
    //     var freeSocket = piece.GetFreeSockets();
    //     if (freeSocket.Count == 0) {
    //         return null;
    //     }
    //     return freeSocket[Random.Range(0, freeSocket.Count)];
    // }

    private RoadSocket GetCompatibleSocket(RoadPiece piece, RoadLaneType laneType){
        foreach (var socket in piece.GetFreeSockets()) {
            if (socket.LaneType == laneType) {
                return socket;
            }
        }

        return null;
    }

    private void TurnRoadPiece(RoadSocket currentSocket, RoadSocket nextSocket){
        Quaternion rotation = Quaternion.FromToRotation(
            nextSocket.Forward,
            -currentSocket.Forward
        );

        RoadPiece nextPiece = nextSocket.GetComponentInParent<RoadPiece>();
        nextPiece.transform.rotation = rotation * nextPiece.transform.rotation;
    }

    private bool TrySpawnNextSegment(){
        RoadPiece currentPiece = spawnedRoadSegmentPrefabs[Random.Range(0, spawnedRoadSegmentPrefabs.Count)];

        var freeSockets = currentPiece.GetFreeSockets();
        if (freeSockets.Count == 0) {
            return false;
        }

        RoadSocket currentSocket = freeSockets[Random.Range(0, freeSockets.Count)];
        RoadPiece nextPrefab = GetRandomRoadSegment();
        RoadPiece nextPiece = Instantiate(nextPrefab);

        RoadSocket nextSocket = GetCompatibleSocket(nextPiece, currentSocket.LaneType);
        if (nextSocket == null) {
            Destroy(nextPiece.gameObject);
            return false;
        } 

        
        TurnRoadPiece(currentSocket, nextSocket);

        Vector3 offset = currentSocket.transform.position - nextSocket.transform.position;
        nextPiece.transform.position += offset;

        // 7. помечаем соккеты
        currentSocket.SetOccupied(true);
        nextSocket.SetOccupied(true);

        // 8. сохраняем сегмент
        spawnedRoadSegmentPrefabs.Add(nextPiece);
        
        return true;
    }
}
