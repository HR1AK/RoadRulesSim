using System.Collections.Generic;
using UnityEngine;
using HealthbarGames;

[RequireComponent(typeof(Collider))]
public class RedLightViolationZone : MonoBehaviour
{
    [Header("Traffic light")]
    [SerializeField] private TrafficLightBase controlledLight;

    [Header("Violation")]
    [SerializeField] private int penaltyPoints = 5;
    [SerializeField] private string message = "Проезд на красный свет!";
    [SerializeField] private float cooldown = 1.5f;

    [Header("Yellow signal")]
    [SerializeField] private bool penalizeYellow = false;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Direction check")]
    [SerializeField] private bool checkDirection = true;
    [SerializeField] private float directionDotThreshold = 0.25f;
    [SerializeField] private float minSpeedToCheckDirection = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;
    [SerializeField] private Color debugColor = new Color(1f, 0f, 0f, 0.35f);

    private PenaltyManager penaltyManager;

    private readonly Dictionary<Transform, float> lastViolationTimeByCar = new();

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        penaltyManager = FindObjectOfType<PenaltyManager>();
    }

    private void OnValidate()
    {
        Collider col = GetComponent<Collider>();

        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Transform carRoot = other.transform.root;

        if (!carRoot.CompareTag(playerTag))
            return;

        if (!IsMovingThroughStopLine(carRoot))
            return;

        if (!IsForbiddenSignal())
            return;

        if (penaltyManager == null)
            penaltyManager = FindObjectOfType<PenaltyManager>();

        if (penaltyManager == null)
        {
            Debug.LogWarning("PenaltyManager не найден на сцене.");
            return;
        }

        if (penaltyManager.IsExamFailed)
            return;

        if (IsOnCooldown(carRoot))
            return;

        lastViolationTimeByCar[carRoot] = Time.time;

        penaltyManager.AddPenalty(
            ViolationType.RedLightCrossing,
            penaltyPoints,
            message
        );
    }

    private bool IsForbiddenSignal()
    {
        if (controlledLight == null)
        {
            Debug.LogWarning($"У стоп-линии {name} не назначен светофор.");
            return false;
        }

        TrafficLightBase.State state = controlledLight.GetState();

        if (state == TrafficLightBase.State.Stop)
            return true;

        if (state == TrafficLightBase.State.PrepareToGo)
            return true;

        if (penalizeYellow && state == TrafficLightBase.State.PrepareToStop)
            return true;

        return false;
    }

    private bool IsMovingThroughStopLine(Transform carRoot)
    {
        if (!checkDirection)
            return true;

        Rigidbody rb = carRoot.GetComponent<Rigidbody>();

        Vector3 movementDirection = carRoot.forward;

        if (rb != null && rb.velocity.magnitude >= minSpeedToCheckDirection)
            movementDirection = rb.velocity.normalized;

        movementDirection.y = 0f;

        Vector3 stopLineDirection = transform.forward;
        stopLineDirection.y = 0f;

        if (movementDirection.sqrMagnitude < 0.001f)
            return true;

        if (stopLineDirection.sqrMagnitude < 0.001f)
            return true;

        float dot = Vector3.Dot(
            movementDirection.normalized,
            stopLineDirection.normalized
        );

        return dot >= directionDotThreshold;
    }

    private bool IsOnCooldown(Transform carRoot)
    {
        if (!lastViolationTimeByCar.TryGetValue(carRoot, out float lastTime))
            return false;

        return Time.time - lastTime < cooldown;
    }

    private void OnDrawGizmos()
    {
        if (!showDebug)
            return;

        Collider col = GetComponent<Collider>();

        if (col == null)
            return;

        if (col is BoxCollider box)
        {
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.color = debugColor;
            Gizmos.DrawCube(box.center, box.size);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(box.center, box.size);

            Gizmos.matrix = oldMatrix;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            transform.position,
            transform.position + transform.forward * 2f
        );
    }

    public bool IsForbiddenForTraffic()
    {
        return IsForbiddenSignal();
    }

    public bool IsCorrectDirection(Vector3 movementDirection)
    {
        if (!checkDirection)
            return true;

        movementDirection.y = 0f;

        Vector3 stopLineDirection = transform.forward;
        stopLineDirection.y = 0f;

        if (movementDirection.sqrMagnitude < 0.001f)
            return true;

        if (stopLineDirection.sqrMagnitude < 0.001f)
            return true;

        float dot = Vector3.Dot(
            movementDirection.normalized,
            stopLineDirection.normalized
        );

        return dot >= directionDotThreshold;
    }
}