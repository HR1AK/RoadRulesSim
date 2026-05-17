using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject playerCarPrefab;
    [SerializeField] private float spawnHeightOffset = 0.4f;

    [Header("Spawn settings")]
    [SerializeField] private bool useRandomSpawnNode = true;

    private GameObject spawnedPlayer;

    public GameObject SpawnPlayer()
    {
        RoadNode spawnNode = FindSpawnNode();

        if (spawnNode == null)
        {
            Debug.LogError("Не найден RoadNode для спавна игрока.");
            return null;
        }

        Vector3 spawnPosition = spawnNode.transform.position + Vector3.up * spawnHeightOffset;
        Quaternion spawnRotation = GetSpawnRotation(spawnNode);

        spawnedPlayer = Instantiate(playerCarPrefab, spawnPosition, spawnRotation);

        if (!spawnedPlayer.CompareTag("Player"))
            spawnedPlayer.tag = "Player";

        SetupCamera(spawnedPlayer.transform);

        // ВОТ СЮДА
        PlayerRouteManager routeManager = FindObjectOfType<PlayerRouteManager>();

        if (routeManager != null)
        {
            routeManager.BuildRouteForPlayer(spawnedPlayer.transform);
        }

        //Debug.Log($"Игрок заспавнен на ноде: {spawnNode.name}");

        return spawnedPlayer;
    }

    private RoadNode FindSpawnNode()
    {
        RoadNode[] allNodes = FindObjectsOfType<RoadNode>();

        List<RoadNode> spawnNodes = new();

        foreach (RoadNode node in allNodes)
        {
            if (node == null)
                continue;

            if (!node.canSpawnPlayer)
                continue;

            if (node.outgoingNodes == null || node.outgoingNodes.Count == 0)
                continue;

            spawnNodes.Add(node);
        }

        if (spawnNodes.Count > 0)
        {
            if (useRandomSpawnNode)
                return spawnNodes[Random.Range(0, spawnNodes.Count)];

            return spawnNodes[0];
        }

        Debug.LogWarning("Не найдено нод с canSpawnPlayer = true. Используется любая подходящая внутренняя нода.");

        foreach (RoadNode node in allNodes)
        {
            if (node == null)
                continue;

            if (node.isExternalNode)
                continue;

            if (node.outgoingNodes == null || node.outgoingNodes.Count == 0)
                continue;

            return node;
        }

        Debug.LogWarning("Не найдено внутренних нод. Используется любая нода с outgoingNodes.");

        foreach (RoadNode node in allNodes)
        {
            if (node == null)
                continue;

            if (node.outgoingNodes == null || node.outgoingNodes.Count == 0)
                continue;

            return node;
        }

        return null;
    }

    private Quaternion GetSpawnRotation(RoadNode spawnNode)
    {
        if (spawnNode.outgoingNodes == null || spawnNode.outgoingNodes.Count == 0)
            return spawnNode.transform.rotation;

        RoadNode nextNode = spawnNode.outgoingNodes[0];

        if (nextNode == null)
            return spawnNode.transform.rotation;

        Vector3 direction = nextNode.transform.position - spawnNode.transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return spawnNode.transform.rotation;

        return Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    private void SetupCamera(Transform playerTransform)
    {
        ThirdPersonCameraFollow cameraFollow = FindObjectOfType<ThirdPersonCameraFollow>();

        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(playerTransform);
        }
    }

    private void SetupRoute(Transform playerTransform)
    {
        PlayerRouteManager routeManager = FindObjectOfType<PlayerRouteManager>();

        if (routeManager != null)
        {
            routeManager.BuildRouteForPlayer(playerTransform);
        }
        else
        {
            Debug.LogWarning("PlayerRouteManager не найден на сцене.");
        }
    }

    public GameObject GetSpawnedPlayer()
    {
        return spawnedPlayer;
    }
}