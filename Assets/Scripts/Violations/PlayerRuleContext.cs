using System.Collections.Generic;
using UnityEngine;

public class PlayerRuleContext : MonoBehaviour
{
    [Header("Speed limit")]
    [SerializeField] private float defaultSpeedLimitKmh = 60f;

    private readonly Dictionary<int, float> activeSpeedLimitZones = new();

    public float CurrentSpeedLimitKmh
    {
        get
        {
            if (activeSpeedLimitZones.Count == 0)
                return defaultSpeedLimitKmh;

            float minLimit = float.MaxValue;

            foreach (float limit in activeSpeedLimitZones.Values)
            {
                if (limit < minLimit)
                    minLimit = limit;
            }

            return minLimit;
        }
    }

    public void EnterSpeedLimitZone(Object zone, float speedLimitKmh)
    {
        if (zone == null)
            return;

        activeSpeedLimitZones[zone.GetInstanceID()] = speedLimitKmh;

        Debug.Log($"Ограничение скорости: {CurrentSpeedLimitKmh} км/ч");
    }

    public void ExitSpeedLimitZone(Object zone)
    {
        if (zone == null)
            return;

        activeSpeedLimitZones.Remove(zone.GetInstanceID());

        Debug.Log($"Ограничение скорости: {CurrentSpeedLimitKmh} км/ч");
    }

    public void ResetContext()
    {
        activeSpeedLimitZones.Clear();
    }
}