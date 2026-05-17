using System.Collections.Generic;
using UnityEngine;

public class TrafficBotAgent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BotCarController botCarController;
    [SerializeField] private Rigidbody rb;

    [Header("Route")]
    [SerializeField] private float nodeReachDistance = 4f;
    [SerializeField] private float repathDistance = 8f;

    [Header("Speed")]
    [SerializeField] private float defaultSpeedLimitKmh = 60f;
    [SerializeField] private float targetSpeedMultiplier = 0.85f;
    [SerializeField] private float accelerationInput = 0.65f;
    [SerializeField] private float brakeInput = 1f;

    [Header("Steering")]
    [SerializeField] private float steerSensitivity = 1.4f;
    [SerializeField] private float slowDownAngle = 35f;

    [Header("Car detection")]
    [SerializeField] private float carLookDistance = 9f;
    [SerializeField] private float carDetectionRadius = 1.2f;
    [SerializeField] private float safeDistance = 5f;

    [Header("Traffic light detection")]
    [SerializeField] private float stopLineLookDistance = 14f;
    [SerializeField] private float stopDistance = 4f;
    [SerializeField] private float stopLineDetectionRadius = 0.7f;

    private readonly List<float> activeSpeedLimits = new();

    private List<RoadNode> route;
    private int currentNodeIndex;

    private RoutePlanner routePlanner;

    public float CurrentSpeedLimitKmh
    {
        get
        {
            if (activeSpeedLimits.Count == 0)
                return defaultSpeedLimitKmh;

            float min = defaultSpeedLimitKmh;

            foreach (float limit in activeSpeedLimits)
                min = Mathf.Min(min, limit);

            return min;
        }
    }

    private void Awake()
    {
        if (botCarController == null)
            botCarController = GetComponent<BotCarController>();

        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (route == null || route.Count < 2)
        {
            botCarController.SetInputs(0f, 0f, 1f);
            return;
        }

        FollowRoute();
    }

    public void SetRoute(List<RoadNode> newRoute)
    {
        route = newRoute;

        if (route != null && route.Count > 1)
            currentNodeIndex = 1;
        else
            currentNodeIndex = 0;
    }

    public void EnterSpeedLimit(float speedLimitKmh)
    {
        activeSpeedLimits.Add(speedLimitKmh);
    }

    public void ExitSpeedLimit(float speedLimitKmh)
    {
        activeSpeedLimits.Remove(speedLimitKmh);
    }

    private void FollowRoute()
    {
        if (botCarController == null || rb == null)
            return;

        if (route == null || route.Count < 2)
        {
            botCarController.SetInputs(0f, 0f, 1f);
            return;
        }

        if (currentNodeIndex >= route.Count)
        {
            botCarController.SetInputs(0f, 0f, 1f);
            return;
        }

        AdvanceNodeIfNeeded();

        if (currentNodeIndex >= route.Count)
        {
            botCarController.SetInputs(0f, 0f, 1f);
            return;
        }

        RoadNode targetNode = route[currentNodeIndex];

        if (targetNode == null)
        {
            botCarController.SetInputs(0f, 0f, 1f);
            return;
        }

        Vector3 targetPosition = targetNode.transform.position;
        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.01f)
        {
            botCarController.SetInputs(0.3f, 0f, 0f);
            return;
        }

        Vector3 localTarget = transform.InverseTransformPoint(targetPosition);

        float steer = localTarget.x / Mathf.Max(1f, Mathf.Abs(localTarget.z));
        steer *= steerSensitivity;
        steer = Mathf.Clamp(steer, -1f, 1f);

        float currentSpeedKmh = rb.velocity.magnitude * 3.6f;
        float normalTargetSpeedKmh = CurrentSpeedLimitKmh * targetSpeedMultiplier;

        float angleToTarget = Vector3.Angle(transform.forward, toTarget.normalized);

        bool shouldStopForCar = IsCarTooCloseAhead();
        bool shouldStopForRedLight = ShouldStopForRedLight();

        float move = accelerationInput;
        float brake = 0f;

        if (shouldStopForCar || shouldStopForRedLight)
        {
            move = 0f;
            brake = brakeInput;
        }
        else
        {
            float desiredSpeedKmh = normalTargetSpeedKmh;

            // На поворотах едем медленно, но не останавливаемся полностью
            if (angleToTarget > slowDownAngle)
            {
                desiredSpeedKmh = 18f;
            }

            if (currentSpeedKmh > desiredSpeedKmh + 5f)
            {
                move = 0f;
                brake = 0.25f;
            }
            else
            {
                move = accelerationInput;
                brake = 0f;
            }

            // Если бот почти стоит на повороте — даём ему аккуратно выехать
            if (currentSpeedKmh < 3f && angleToTarget > 20f)
            {
                move = 0.35f;
                brake = 0f;
            }
        }

        botCarController.SetInputs(move, steer, brake);
    }

    private void AdvanceNodeIfNeeded()
    {
        while (currentNodeIndex < route.Count)
        {
            RoadNode targetNode = route[currentNodeIndex];

            if (targetNode == null)
            {
                currentNodeIndex++;
                continue;
            }

            Vector3 toTarget = targetNode.transform.position - transform.position;
            toTarget.y = 0f;

            float distance = toTarget.magnitude;

            if (distance <= nodeReachDistance)
            {
                currentNodeIndex++;
                continue;
            }

            // Если нода уже осталась позади, тоже переключаемся
            float dot = Vector3.Dot(transform.forward, toTarget.normalized);

            if (dot < -0.25f && distance < nodeReachDistance * 2f)
            {
                currentNodeIndex++;
                continue;
            }

            break;
        }
    }

    private bool IsCarTooCloseAhead()
    {
        Vector3 origin = transform.position + Vector3.up * 0.7f + transform.forward * 1.5f;

        if (!Physics.SphereCast(
                origin,
                carDetectionRadius,
                transform.forward,
                out RaycastHit hit,
                carLookDistance,
                ~0,
                QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        if (hit.transform.root == transform.root)
            return false;

        bool isVehicle =
            hit.transform.root.CompareTag("Player") ||
            hit.transform.root.CompareTag("TrafficCar") ||
            hit.transform.root.GetComponent<BotCarController>() != null;

        if (!isVehicle)
            return false;

        return hit.distance <= safeDistance;
    }

    private bool ShouldStopForRedLight()
    {
        Vector3 origin = transform.position + Vector3.up * 0.7f + transform.forward * 1.5f;

        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            stopLineDetectionRadius,
            transform.forward,
            stopLineLookDistance,
            ~0,
            QueryTriggerInteraction.Collide
        );

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.root == transform.root)
                continue;

            RedLightViolationZone redLightZone =
                hit.collider.GetComponent<RedLightViolationZone>();

            if (redLightZone == null)
                redLightZone = hit.collider.GetComponentInParent<RedLightViolationZone>();

            if (redLightZone == null)
                continue;

            if (!redLightZone.IsForbiddenForTraffic())
                continue;

            if (!redLightZone.IsCorrectDirection(transform.forward))
                continue;

            return hit.distance <= stopDistance || rb.velocity.magnitude * 3.6f > 5f;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 origin = transform.position + Vector3.up * 0.7f + transform.forward * 1.5f;
        Gizmos.DrawLine(origin, origin + transform.forward * carLookDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + transform.forward * stopLineLookDistance);
    }
}