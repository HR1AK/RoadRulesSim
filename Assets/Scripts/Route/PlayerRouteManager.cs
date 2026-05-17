using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRouteManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoutePlanner routePlanner;
    [SerializeField] private RouteVisualizer routeVisualizer;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";

    [Header("Route settings")]
    [SerializeField] private float minRouteDistance = 40f;
    [SerializeField] private bool buildRouteAutomatically = true;
    [SerializeField] private float startDelay = 0.5f;

    [Header("Destination marker")]
    [SerializeField] private GameObject destinationMarkerPrefab;
    [SerializeField] private float destinationMarkerHeightOffset = 0.3f;

    [Header("Exam finish")]
    [SerializeField] private float finishDistance = 4f;
    [SerializeField] private bool clearRouteOnFinish = true;

    private PenaltyManager penaltyManager;
    private bool routeCompleted;

    private RoadNode currentStartNode;
    private RoadNode currentGoalNode;
    private List<RoadNode> currentRoute;
    private GameObject currentDestinationMarker;

    private IEnumerator Start()
    {
        if (!buildRouteAutomatically)
            yield break;

        yield return new WaitForSeconds(startDelay);

        while (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

            if (playerObject != null)
                player = playerObject.transform;

            yield return null;
        }

        BuildRouteForPlayer(player);
    }

    private void Update()
    {
        CheckRouteFinish();
    }

    private void CheckRouteFinish()
    {
        if (routeCompleted)
            return;

        if (player == null)
            return;

        if (currentGoalNode == null)
            return;

        if (penaltyManager == null)
            penaltyManager = FindObjectOfType<PenaltyManager>();

        if (penaltyManager == null)
            return;

        float distanceToFinish = Vector3.Distance(
            player.position,
            currentGoalNode.transform.position
        );

        if (distanceToFinish > finishDistance)
            return;

        routeCompleted = true;

        if (!penaltyManager.IsExamFailed &&
            penaltyManager.TotalPoints < penaltyManager.MaxPenaltyPoints)
        {
            Debug.Log("Поздравляем! Экзамен пройден!");

            if (clearRouteOnFinish && routeVisualizer != null)
            {
                routeVisualizer.ClearRoute();
            }
        }
        else
        {
            Debug.Log("Маршрут завершён, но экзамен не пройден из-за штрафных баллов.");
        }
    }

    public void BuildRouteForPlayer(Transform playerTransform)
    {
        routeCompleted = false;
        if (playerTransform != null)
            player = playerTransform;

        if (player == null)
        {
            Debug.LogWarning("PlayerRouteManager: игрок не найден.");
            return;
        }

        if (routePlanner == null)
            routePlanner = FindObjectOfType<RoutePlanner>();

        if (routeVisualizer == null)
            routeVisualizer = FindObjectOfType<RouteVisualizer>();

        if (routePlanner == null || routeVisualizer == null)
        {
            Debug.LogWarning("PlayerRouteManager: не найден RoutePlanner или RouteVisualizer.");
            return;
        }

        routePlanner.RefreshNodes();

        currentStartNode = routePlanner.FindNearestNode(player.position, true);

        if (currentStartNode == null)
        {
            Debug.LogWarning("PlayerRouteManager: стартовая RoadNode не найдена.");
            return;
        }

        currentGoalNode = routePlanner.FindFarthestReachableNode(
            currentStartNode,
            minRouteDistance
        );

        if (currentGoalNode == null)
        {
            Debug.LogWarning("PlayerRouteManager: конечная RoadNode не найдена.");
            return;
        }

        currentRoute = routePlanner.FindPath(currentStartNode, currentGoalNode);

        if (currentRoute == null || currentRoute.Count < 2)
        {
            Debug.LogWarning("PlayerRouteManager: маршрут не построен.");
            return;
        }

        routeVisualizer.DrawRoute(currentRoute);
        SpawnDestinationMarker();

        Debug.Log($"Маршрут построен. Узлов: {currentRoute.Count}");
    }

    private void SpawnDestinationMarker()
    {
        if (currentDestinationMarker != null)
            Destroy(currentDestinationMarker);

        if (destinationMarkerPrefab == null || currentGoalNode == null)
            return;

        Vector3 position =
            currentGoalNode.transform.position +
            Vector3.up * destinationMarkerHeightOffset;

        currentDestinationMarker = Instantiate(
            destinationMarkerPrefab,
            position,
            Quaternion.identity
        );
    }

    public RoadNode GetGoalNode()
    {
        return currentGoalNode;
    }

    public List<RoadNode> GetCurrentRoute()
    {
        return currentRoute;
    }
}