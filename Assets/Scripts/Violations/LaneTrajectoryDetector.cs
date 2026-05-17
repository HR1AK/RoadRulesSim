using System.Collections.Generic;
using UnityEngine;

public class LaneTrajectoryDetector : MonoBehaviour
{
    [Header("Penalty")]
    [SerializeField] private int penaltyPoints = 5;
    [SerializeField] private string wrongDirectionMessage = "Движение по встречной полосе!";
    [SerializeField] private string wrongTrajectoryMessage = "Неверная траектория движения!";
    [SerializeField] private float penaltyCooldown = 2f;

    [Header("Trajectory check")]
    [SerializeField] private float allowedDistanceFromTrajectory = 2.2f;
    [SerializeField] private float wrongDirectionDotThreshold = -0.25f;
    [SerializeField] private float minSpeedToCheckKmh = 5f;
    [SerializeField] private float checkInterval = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;
    [SerializeField] private Color validColor = Color.green;
    [SerializeField] private Color invalidColor = Color.red;

    private Rigidbody rb;
    private PenaltyManager penaltyManager;

    private readonly List<TrajectoryEdge> edges = new();

    private float checkTimer;
    private float lastPenaltyTime = -999f;

    private TrajectoryEdge nearestEdge;
    private float nearestDistance;

    private struct TrajectoryEdge
    {
        public RoadNode From;
        public RoadNode To;

        public Vector3 Start => From.transform.position;
        public Vector3 End => To.transform.position;

        public Vector3 Direction
        {
            get
            {
                Vector3 dir = End - Start;
                dir.y = 0f;
                return dir.sqrMagnitude > 0.001f ? dir.normalized : Vector3.forward;
            }
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        penaltyManager = FindObjectOfType<PenaltyManager>();
    }

    private void Start()
    {
        BuildTrajectoryEdges();
    }

    private void Update()
    {
        checkTimer += Time.deltaTime;

        if (checkTimer < checkInterval)
            return;

        checkTimer = 0f;

        CheckTrajectory();
    }

    private void BuildTrajectoryEdges()
    {
        edges.Clear();

        RoadNode[] nodes = FindObjectsOfType<RoadNode>();

        foreach (RoadNode node in nodes)
        {
            if (node == null || node.outgoingNodes == null)
                continue;

            foreach (RoadNode next in node.outgoingNodes)
            {
                if (next == null)
                    continue;

                edges.Add(new TrajectoryEdge
                {
                    From = node,
                    To = next
                });
            }
        }

        Debug.Log($"LaneTrajectoryDetector: найдено траекторий: {edges.Count}");
    }

    private void CheckTrajectory()
    {
        if (rb == null)
            return;

        if (penaltyManager == null)
            penaltyManager = FindObjectOfType<PenaltyManager>();

        if (penaltyManager == null || penaltyManager.IsExamFailed)
            return;

        float speedKmh = rb.velocity.magnitude * 3.6f;

        if (speedKmh < minSpeedToCheckKmh)
            return;

        if (edges.Count == 0)
        {
            BuildTrajectoryEdges();
            return;
        }

        Vector3 carPosition = transform.position;
        nearestEdge = FindNearestEdge(carPosition, out nearestDistance);

        if (nearestEdge.From == null || nearestEdge.To == null)
            return;

        if (nearestDistance > allowedDistanceFromTrajectory)
        {
            AddPenaltyOnce(ViolationType.WrongLane, wrongTrajectoryMessage);
            return;
        }

        Vector3 movementDirection = rb.velocity;
        movementDirection.y = 0f;

        if (movementDirection.sqrMagnitude < 0.001f)
            movementDirection = transform.forward;

        movementDirection.Normalize();

        float dot = Vector3.Dot(movementDirection, nearestEdge.Direction);

        if (dot < wrongDirectionDotThreshold)
        {
            AddPenaltyOnce(ViolationType.WrongLane, wrongDirectionMessage);
        }
    }

    private TrajectoryEdge FindNearestEdge(Vector3 point, out float distance)
    {
        TrajectoryEdge bestEdge = default;
        distance = float.MaxValue;

        foreach (TrajectoryEdge edge in edges)
        {
            Vector3 closestPoint = GetClosestPointOnSegment(
                point,
                edge.Start,
                edge.End
            );

            float currentDistance = Vector3.Distance(
                Flatten(point),
                Flatten(closestPoint)
            );

            if (currentDistance < distance)
            {
                distance = currentDistance;
                bestEdge = edge;
            }
        }

        return bestEdge;
    }

    private Vector3 GetClosestPointOnSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        Vector3 flatPoint = Flatten(point);
        Vector3 flatA = Flatten(a);
        Vector3 flatB = Flatten(b);

        Vector3 ab = flatB - flatA;

        float abLengthSqr = ab.sqrMagnitude;

        if (abLengthSqr < 0.001f)
            return flatA;

        float t = Vector3.Dot(flatPoint - flatA, ab) / abLengthSqr;
        t = Mathf.Clamp01(t);

        return flatA + ab * t;
    }

    private Vector3 Flatten(Vector3 v)
    {
        return new Vector3(v.x, 0f, v.z);
    }

    private void AddPenaltyOnce(ViolationType type, string message)
    {
        if (Time.time - lastPenaltyTime < penaltyCooldown)
            return;

        lastPenaltyTime = Time.time;

        penaltyManager.AddPenalty(
            type,
            penaltyPoints,
            message
        );
    }

    private void OnDrawGizmos()
    {
        if (!showDebug)
            return;

        if (nearestEdge.From == null || nearestEdge.To == null)
            return;

        Gizmos.color = nearestDistance <= allowedDistanceFromTrajectory
            ? validColor
            : invalidColor;

        Gizmos.DrawLine(
            nearestEdge.Start + Vector3.up * 0.2f,
            nearestEdge.End + Vector3.up * 0.2f
        );

        Gizmos.DrawSphere(
            nearestEdge.Start + Vector3.up * 0.2f,
            0.25f
        );

        Gizmos.DrawSphere(
            nearestEdge.End + Vector3.up * 0.2f,
            0.25f
        );
    }
}