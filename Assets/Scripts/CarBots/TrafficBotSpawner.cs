using System.Collections.Generic;
using UnityEngine;

public class TrafficBotSpawner : MonoBehaviour
{
    [Header("Bot")]
    [SerializeField] private GameObject botPrefab;
    [SerializeField] private int botCount = 5;
    [SerializeField] private float spawnHeightOffset = 0.4f;

    [Header("Spawn")]
    [SerializeField] private float minDistanceFromPlayer = 20f;
    [SerializeField] private float minDistanceBetweenBots = 12f;
    [SerializeField] private int maxSpawnAttempts = 100;

    [Header("Route")]
    [SerializeField] private RoutePlanner routePlanner;
    [SerializeField] private float minRouteDistance = 30f;

    private readonly List<GameObject> spawnedBots = new();

    public void SpawnBots()
    {
        if (botPrefab == null)
        {
            Debug.LogWarning("TrafficBotSpawner: botPrefab не назначен.");
            return;
        }

        if (routePlanner == null)
            routePlanner = FindObjectOfType<RoutePlanner>();

        if (routePlanner == null)
        {
            Debug.LogWarning("TrafficBotSpawner: RoutePlanner не найден.");
            return;
        }

        routePlanner.RefreshNodes();

        RoadNode[] allNodes = FindObjectsOfType<RoadNode>();

        List<RoadNode> spawnCandidates = new();

        foreach (RoadNode node in allNodes)
        {
            if (node == null)
                continue;

            if (node.outgoingNodes == null || node.outgoingNodes.Count == 0)
                continue;

            if (node.isExternalNode)
                continue;

            spawnCandidates.Add(node);
        }

        if (spawnCandidates.Count == 0)
        {
            Debug.LogWarning("TrafficBotSpawner: нет подходящих RoadNode для спавна.");
            return;
        }

        for (int i = 0; i < botCount; i++)
        {
            TrySpawnOneBot(spawnCandidates);
        }

        Debug.Log($"TrafficBotSpawner: заспавнено ботов: {spawnedBots.Count}");
    }

    private void TrySpawnOneBot(List<RoadNode> spawnCandidates)
    {
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            RoadNode spawnNode = spawnCandidates[Random.Range(0, spawnCandidates.Count)];

            if (!IsSpawnPositionFree(spawnNode.transform.position))
                continue;

            RoadNode goalNode = routePlanner.FindFarthestReachableNode(
                spawnNode,
                minRouteDistance
            );

            if (goalNode == null)
                continue;

            List<RoadNode> route = routePlanner.FindPath(spawnNode, goalNode);

            if (route == null || route.Count < 2)
                continue;

            Vector3 spawnPosition =
                spawnNode.transform.position +
                Vector3.up * spawnHeightOffset;

            Quaternion spawnRotation = GetSpawnRotation(spawnNode);

            GameObject bot = Instantiate(
                botPrefab,
                spawnPosition,
                spawnRotation
            );

            bot.tag = "TrafficCar";

            TrafficBotAgent botAgent = bot.GetComponent<TrafficBotAgent>();

            if (botAgent != null)
                botAgent.SetRoute(route);

            spawnedBots.Add(bot);
            return;
        }

        Debug.LogWarning("TrafficBotSpawner: не удалось найти место для бота.");
    }

    private bool IsSpawnPositionFree(Vector3 position)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(
                position,
                player.transform.position
            );

            if (distanceToPlayer < minDistanceFromPlayer)
                return false;
        }

        foreach (GameObject bot in spawnedBots)
        {
            if (bot == null)
                continue;

            float distanceToBot = Vector3.Distance(
                position,
                bot.transform.position
            );

            if (distanceToBot < minDistanceBetweenBots)
                return false;
        }

        return true;
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

    public void ClearBots()
    {
        foreach (GameObject bot in spawnedBots)
        {
            if (bot != null)
                Destroy(bot);
        }

        spawnedBots.Clear();
    }
}