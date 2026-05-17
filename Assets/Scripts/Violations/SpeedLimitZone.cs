using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SpeedLimitZone : MonoBehaviour
{
    [Header("Speed limit")]
    [SerializeField] private float speedLimitKmh = 40f;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;
    [SerializeField] private Color debugColor = new Color(0f, 0.6f, 1f, 0.25f);

    private PlayerRuleContext ruleContext;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        ruleContext = FindObjectOfType<PlayerRuleContext>();
    }

    private void OnValidate()
    {
        Collider col = GetComponent<Collider>();

        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Transform root = other.transform.root;

        TrafficBotAgent bot = root.GetComponent<TrafficBotAgent>();

        if (bot != null)
        {
            bot.EnterSpeedLimit(speedLimitKmh);
            return;
        }

        if (!root.CompareTag(playerTag))
            return;

        if (ruleContext == null)
            ruleContext = FindObjectOfType<PlayerRuleContext>();

        if (ruleContext == null)
        {
            Debug.LogWarning("PlayerRuleContext не найден на сцене.");
            return;
        }

        ruleContext.EnterSpeedLimitZone(this, speedLimitKmh);
    }

    private void OnTriggerExit(Collider other)
    {
        Transform root = other.transform.root;

        TrafficBotAgent bot = root.GetComponent<TrafficBotAgent>();

        if (bot != null)
        {
            bot.ExitSpeedLimit(speedLimitKmh);
            return;
        }

        if (!root.CompareTag(playerTag))
            return;

        if (ruleContext == null)
            ruleContext = FindObjectOfType<PlayerRuleContext>();

        if (ruleContext == null)
            return;

        ruleContext.ExitSpeedLimitZone(this);
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

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(box.center, box.size);

            Gizmos.matrix = oldMatrix;
        }
    }
}