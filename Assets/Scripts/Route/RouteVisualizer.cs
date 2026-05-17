using System.Collections.Generic;
using UnityEngine;

public class RouteVisualizer : MonoBehaviour
{
    [Header("Arrow prefab")]
    [SerializeField] private GameObject arrowPrefab;

    [Header("Arrow settings")]
    [SerializeField] private float arrowSpacing = 4f;
    [SerializeField] private float heightOffset = 0.08f;
    [SerializeField] private float startEndPadding = 1f;
    [SerializeField] private float arrowScale = 1f;

    [Header("Optional line")]
    [SerializeField] private bool drawLine = true;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float lineHeightOffset = 0.05f;
    [SerializeField] private float lineWidth = 0.25f;

    private Transform arrowsRoot;

    private void Awake()
    {
        CreateRootIfNeeded();
    }

    public void DrawRoute(List<RoadNode> path)
    {
        ClearRoute();

        if (path == null || path.Count < 2)
        {
            Debug.LogWarning("RouteVisualizer: маршрут пустой или слишком короткий.");
            return;
        }

        CreateRootIfNeeded();

        if (drawLine)
            DrawRouteLine(path);

        for (int i = 0; i < path.Count - 1; i++)
        {
            RoadNode from = path[i];
            RoadNode to = path[i + 1];

            if (from == null || to == null)
                continue;

            SpawnArrowsBetween(
                from.transform.position,
                to.transform.position
            );
        }
    }

    public void ClearRoute()
    {
        if (arrowsRoot != null)
        {
            for (int i = arrowsRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(arrowsRoot.GetChild(i).gameObject);
            }
        }

        if (lineRenderer != null)
            lineRenderer.positionCount = 0;
    }

    private void SpawnArrowsBetween(Vector3 start, Vector3 end)
    {
        if (arrowPrefab == null)
        {
            Debug.LogWarning("RouteVisualizer: не назначен Arrow Prefab.");
            return;
        }

        Vector3 flatStart = Flatten(start);
        Vector3 flatEnd = Flatten(end);

        Vector3 direction = flatEnd - flatStart;
        float length = direction.magnitude;

        if (length < 0.5f)
            return;

        direction.Normalize();

        float usableLength = Mathf.Max(0f, length - startEndPadding * 2f);

        if (usableLength <= 0f)
            return;

        for (float distance = startEndPadding; distance < length - startEndPadding; distance += arrowSpacing)
        {
            Vector3 position =
                flatStart +
                direction * distance +
                Vector3.up * heightOffset;

            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

            GameObject arrow = Instantiate(
                arrowPrefab,
                position,
                rotation,
                arrowsRoot
            );

            arrow.transform.localScale *= arrowScale;

            // Чтобы стрелки случайно не мешали физике
            Collider[] colliders = arrow.GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
                col.enabled = false;
        }
    }

    private void DrawRouteLine(List<RoadNode> path)
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.positionCount = path.Count;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.useWorldSpace = true;

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 position = path[i].transform.position + Vector3.up * lineHeightOffset;
            lineRenderer.SetPosition(i, position);
        }
    }

    private void CreateRootIfNeeded()
    {
        if (arrowsRoot != null)
            return;

        GameObject root = new GameObject("RouteArrows");
        root.transform.SetParent(transform);
        root.transform.localPosition = Vector3.zero;
        arrowsRoot = root.transform;
    }

    private Vector3 Flatten(Vector3 value)
    {
        return new Vector3(value.x, 0f, value.z);
    }
}