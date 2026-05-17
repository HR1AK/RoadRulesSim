using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ViolationZone : MonoBehaviour
{
    [Header("Violation")]
    [SerializeField] private ViolationType violationType;
    [SerializeField] private int penaltyPoints = 5;
    [SerializeField] private string message = "Нарушение!";
    [SerializeField] private float cooldown = 1.5f;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

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

        penaltyManager.AddPenalty(violationType, penaltyPoints, message);
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
    }
}