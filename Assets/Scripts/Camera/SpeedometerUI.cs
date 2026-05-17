using TMPro;
using UnityEngine;

public class SpeedometerUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text speedText;

    [Header("Target")]
    [SerializeField] private Rigidbody targetRigidbody;
    [SerializeField] private string playerTag = "Player";

    [Header("Settings")]
    [SerializeField] private bool autoFindPlayer = true;
    [SerializeField] private string speedFormat = "{0:0} км/ч";

    private void Awake()
    {
        if (speedText == null)
            speedText = GetComponent<TMP_Text>();
    }

    private void LateUpdate()
    {
        if (targetRigidbody == null && autoFindPlayer)
            TryFindPlayer();

        if (targetRigidbody == null)
        {
            SetSpeedText(0f);
            return;
        }

        float speedKmh = targetRigidbody.velocity.magnitude * 3.6f;
        SetSpeedText(speedKmh);
    }

    private void TryFindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player == null)
            return;

        targetRigidbody = player.GetComponent<Rigidbody>();
    }

    private void SetSpeedText(float speedKmh)
    {
        if (speedText == null)
            return;

        speedText.text = string.Format(speedFormat, speedKmh);
    }

    public void SetTarget(Rigidbody newTarget)
    {
        targetRigidbody = newTarget;
    }
}