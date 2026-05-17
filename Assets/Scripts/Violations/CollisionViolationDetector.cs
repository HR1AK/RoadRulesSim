using UnityEngine;

public class CollisionViolationDetector : MonoBehaviour
{
    [Header("Penalty")]
    [SerializeField] private int penaltyPoints = 5;
    [SerializeField] private string message = "Совершено столкновение!";
    [SerializeField] private float cooldown = 1.5f;

    [Header("Collision settings")]
    [SerializeField] private float minCollisionRelativeVelocity = 2f;

    private PenaltyManager penaltyManager;
    private float lastViolationTime = -999f;

    private void Awake()
    {
        penaltyManager = FindObjectOfType<PenaltyManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time - lastViolationTime < cooldown)
            return;

        if (penaltyManager == null)
            penaltyManager = FindObjectOfType<PenaltyManager>();

        if (penaltyManager == null || penaltyManager.IsExamFailed)
            return;

        if (collision.relativeVelocity.magnitude < minCollisionRelativeVelocity)
            return;

        lastViolationTime = Time.time;

        penaltyManager.AddPenalty(
            ViolationType.Collision,
            penaltyPoints,
            message
        );
    }
}