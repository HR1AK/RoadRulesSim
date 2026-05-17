using UnityEngine;

public class SpeedingViolationDetector : MonoBehaviour
{
    [Header("Penalty")]
    [SerializeField] private int penaltyPoints = 1;
    [SerializeField] private string message = "Превышена скорость!";
    [SerializeField] private float timeBeforePenalty = 2f;
    [SerializeField] private float penaltyCooldown = 5f;

    [Header("Speed settings")]
    [SerializeField] private float toleranceKmh = 5f;

    [Header("Debug")]
    [SerializeField] private bool debugSpeedCheck = false;

    private Rigidbody rb;
    private PlayerRuleContext ruleContext;
    private PenaltyManager penaltyManager;

    private float speedingTimer;
    private float lastPenaltyTime = -999f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ruleContext = FindObjectOfType<PlayerRuleContext>();
        penaltyManager = FindObjectOfType<PenaltyManager>();

        ClampSettings();
    }

    private void OnValidate()
    {
        ClampSettings();
    }

    private void ClampSettings()
    {
        toleranceKmh = Mathf.Max(0f, toleranceKmh);
        timeBeforePenalty = Mathf.Max(0.1f, timeBeforePenalty);
        penaltyCooldown = Mathf.Max(0.1f, penaltyCooldown);
        penaltyPoints = Mathf.Max(1, penaltyPoints);
    }

    private void Update()
    {
        if (rb == null)
            return;

        if (ruleContext == null)
            ruleContext = FindObjectOfType<PlayerRuleContext>();

        if (penaltyManager == null)
            penaltyManager = FindObjectOfType<PenaltyManager>();

        if (ruleContext == null || penaltyManager == null)
            return;

        if (penaltyManager.IsExamFailed)
            return;

        float currentSpeedKmh = rb.velocity.magnitude * 3.6f;
        float limitKmh = ruleContext.CurrentSpeedLimitKmh;
        float allowedSpeedKmh = limitKmh + toleranceKmh;

        bool isSpeeding = currentSpeedKmh > allowedSpeedKmh;

        if (debugSpeedCheck)
        {
            Debug.Log(
                $"Speed check: current={currentSpeedKmh:0.0}, limit={limitKmh:0.0}, tolerance={toleranceKmh:0.0}, allowed={allowedSpeedKmh:0.0}, speeding={isSpeeding}"
            );
        }

        if (!isSpeeding)
        {
            speedingTimer = 0f;
            return;
        }

        speedingTimer += Time.deltaTime;

        if (speedingTimer < timeBeforePenalty)
            return;

        if (Time.time - lastPenaltyTime < penaltyCooldown)
            return;

        // Дополнительная защита прямо перед штрафом
        currentSpeedKmh = rb.velocity.magnitude * 3.6f;
        limitKmh = ruleContext.CurrentSpeedLimitKmh;
        allowedSpeedKmh = limitKmh + toleranceKmh;

        if (currentSpeedKmh <= allowedSpeedKmh)
        {
            speedingTimer = 0f;
            return;
        }

        lastPenaltyTime = Time.time;
        speedingTimer = 0f;

        string fullMessage =
            $"{message} {currentSpeedKmh:0} км/ч при ограничении {limitKmh:0} км/ч";

        penaltyManager.AddPenalty(
            ViolationType.Speeding,
            penaltyPoints,
            fullMessage
        );
    }
}