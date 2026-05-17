using System;
using System.Collections.Generic;
using UnityEngine;

public class PenaltyManager : MonoBehaviour
{
    [SerializeField] private int maxPenaltyPoints = 5;

    public int TotalPoints { get; private set; }
    public bool IsExamFailed { get; private set; }
    public int MaxPenaltyPoints => maxPenaltyPoints;

    private readonly List<ViolationType> violations = new();

    public event Action<ViolationType, int, int, string> OnPenaltyAdded;
    public event Action OnExamFailed;

    public void AddPenalty(ViolationType type, int points, string message)
    {
        if (IsExamFailed)
            return;

        TotalPoints += points;
        violations.Add(type);

        Debug.Log($"{message} +{points} штрафных баллов. Всего: {TotalPoints}");

        OnPenaltyAdded?.Invoke(type, points, TotalPoints, message);

        if (TotalPoints >= maxPenaltyPoints)
        {
            IsExamFailed = true;
            Debug.Log("Экзамен завершён: набрано максимальное количество штрафных баллов.");
            OnExamFailed?.Invoke();
        }
    }

    public IReadOnlyList<ViolationType> GetViolations()
    {
        return violations;
    }

    public void ResetPenalties()
    {
        TotalPoints = 0;
        IsExamFailed = false;
        violations.Clear();
    }
}