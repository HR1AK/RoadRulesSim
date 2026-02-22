using System;
using System.Collections.Generic;
using UnityEngine;
using HealthbarGames;

/// <summary>
/// Full WFC (Wave Function Collapse) generator for road-tile city.
/// Uses RoadPiece sockets (North/East/South/West) + rotations.
/// Collapses cells with minimum entropy, propagates constraints, restarts on contradictions.
/// </summary>
public class FullWfcCityGenerator : MonoBehaviour
{
    [Header("Tiles")]
    [SerializeField] private List<RoadPiece> roadPrefabs = new List<RoadPiece>();

    [Header("Grid")]
    [SerializeField] private int maxSegments = 50;          // like your maxSegment, total cells count
    [SerializeField] private float cellSize = 10f;
    [SerializeField] private bool constrainBordersToNone = false; // prevent roads pointing outside grid

    [Header("WFC Settings")]
    [SerializeField] private int maxRestarts = 20;
    [SerializeField] private int seed = 0;                 // 0 => random seed at runtime
    [SerializeField] private bool logRestarts = true;

    [Header("Traffic Lights")]
    [SerializeField] private TrafficLightManager trafficLightManager;

    // ===== Internal types =====

    private struct Option
    {
        public RoadPiece Prefab;
        public int Rot; // 0..3

        public Option(RoadPiece prefab, int rot)
        {
            Prefab = prefab;
            Rot = rot;
        }

        public int Weight => Mathf.Max(1, Prefab != null ? Prefab.Weight : 1);
        public override string ToString() => Prefab ? $"{Prefab.name}@{Rot * 90}°" : $"<null>@{Rot}";
    }

    private enum Dir { Up = 0, Right = 1, Down = 2, Left = 3 }

    private static readonly Vector2Int[] DirVec =
    {
        Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
    };

    // ===== Runtime data =====

    private int _width, _height;
    private System.Random _rng;

    private List<Option> _allOptions; // all (prefab,rot) combinations
    private Dictionary<int, HashSet<int>>[] _compat; // _compat[dir][optionIndex] = set of neighbor optionIndices allowed in that dir

    private HashSet<int>[,] _wave;    // per-cell possible option indices
    private int[,] _collapsed;         // -1 if not collapsed else chosen option index

    private readonly List<RealTrafficLight> _spawnedLights = new List<RealTrafficLight>();

    // ===== Unity =====

    private void Start()
    {
        if (roadPrefabs == null || roadPrefabs.Count == 0)
        {
            Debug.LogError("[FullWfcCityGenerator] roadPrefabs is empty.");
            return;
        }

        var size = CalculateGridSize(maxSegments);
        _width = size.x;
        _height = size.y;

        int actualSeed = seed != 0 ? seed : UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        _rng = new System.Random(actualSeed);

        BuildOptionSet();
        BuildCompatibilityRules();

        bool success = GenerateWithRestarts(out int usedRestarts);

        if (!success)
        {
            Debug.LogError($"[FullWfcCityGenerator] Failed to generate after {maxRestarts} restarts.");
            return;
        }

        SpawnResult();
        if (trafficLightManager != null)
            trafficLightManager.AutoSetupPhases(_spawnedLights);

        if (logRestarts)
            Debug.Log($"[FullWfcCityGenerator] Generation done. Grid={_width}x{_height}, options={_allOptions.Count}, restartsUsed={usedRestarts}, seed={actualSeed}");
    }

    // ===== Core WFC =====

    private bool GenerateWithRestarts(out int restartsUsed)
    {
        for (int attempt = 0; attempt <= maxRestarts; attempt++)
        {
            restartsUsed = attempt;

            InitWave();

            bool ok = RunWfc();
            if (ok) return true;

            if (logRestarts)
                Debug.LogWarning($"[FullWfcCityGenerator] Contradiction -> restart #{attempt + 1}/{maxRestarts}");
        }

        restartsUsed = maxRestarts;
        return false;
    }

   private void InitWave()
    {
        _wave = new HashSet<int>[_width, _height];
        _collapsed = new int[_width, _height];

        for (int x = 0; x < _width; x++)
        for (int y = 0; y < _height; y++)
        {
            _collapsed[x, y] = -1;
            _wave[x, y] = new HashSet<int>();
            for (int i = 0; i < _allOptions.Count; i++)
                _wave[x, y].Add(i);
        }

        if (constrainBordersToNone)
            ApplyBorderConstraints();
    }

    private bool RunWfc()
    {
        // Main loop: pick min-entropy cell, collapse, propagate.
        while (true)
        {
            Vector2Int cell = PickCellWithMinEntropy(out bool hasUncollapsed);
            if (!hasUncollapsed)
                return true; // all collapsed

            if (cell.x < 0 || cell.y < 0)
                return true;

            if (_wave[cell.x, cell.y] == null || _wave[cell.x, cell.y].Count == 0)
                return false;    

            if (_wave[cell.x, cell.y].Count == 0)
                return false;

            int chosen = ChooseWeighted(_wave[cell.x, cell.y]);
            CollapseCell(cell, chosen);

            bool ok = PropagateFrom(cell);
            if (!ok)
                return false;
        }
    }

    private Vector2Int PickCellWithMinEntropy(out bool hasUncollapsed)
    {
        hasUncollapsed = false;

        int bestCount = int.MaxValue;
        List<Vector2Int> bestCells = new List<Vector2Int>(16);

        for (int x = 0; x < _width; x++)
        for (int y = 0; y < _height; y++)
        {
            if (_collapsed[x, y] != -1)
                continue;

            hasUncollapsed = true;

            // _wave[x,y] theoretically should never be null, but guard anyway
            int c = (_wave[x, y] != null) ? _wave[x, y].Count : 0;

            // Contradiction: no possible tiles
            if (c == 0)
                return new Vector2Int(x, y);

            // We choose the smallest domain size (entropy proxy)
            if (c < bestCount)
            {
                bestCount = c;
                bestCells.Clear();
                bestCells.Add(new Vector2Int(x, y));
            }
            else if (c == bestCount)
            {
                bestCells.Add(new Vector2Int(x, y));
            }
        }

        if (!hasUncollapsed)
            return new Vector2Int(-1, -1);

        // Tie-break randomly among best cells
        int idx = _rng.Next(0, bestCells.Count);
        return bestCells[idx];
    }

    private void CollapseCell(Vector2Int cell, int optionIndex)
    {
        _collapsed[cell.x, cell.y] = optionIndex;

        _wave[cell.x, cell.y].Clear();
        _wave[cell.x, cell.y].Add(optionIndex);
    }

    private bool PropagateFrom(Vector2Int start)
    {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(start);

        while (q.Count > 0)
        {
            Vector2Int cur = q.Dequeue();

            for (int d = 0; d < 4; d++)
            {
                Vector2Int nb = cur + DirVec[d];
                if (!InBounds(nb)) continue;

                // если у текущей клетки нет вариантов — противоречие
                if (_wave[cur.x, cur.y].Count == 0)
                    return false;

                bool changed = ReduceNeighborByCurrent(cur, nb, (Dir)d);
                if (changed)
                {
                    if (_wave[nb.x, nb.y].Count == 0)
                        return false;

                    q.Enqueue(nb);
                }
            }
        }

        return true;
    }

    private bool ReduceNeighborByCurrent(Vector2Int cur, Vector2Int nb, Dir dirToNb)
    {
        // allowed neighbor options are union over compat sets of each current option
        HashSet<int> allowed = null;

        foreach (int curOpt in _wave[cur.x, cur.y])
        {
            var set = _compat[(int)dirToNb][curOpt];
            if (allowed == null) allowed = new HashSet<int>(set);
            else allowed.UnionWith(set);
        }

        if (allowed == null) allowed = new HashSet<int>();

        int before = _wave[nb.x, nb.y].Count;
        _wave[nb.x, nb.y].IntersectWith(allowed);
        return _wave[nb.x, nb.y].Count != before;
    }
    /// <summary>
    /// Reduces neighbor options based on current cell possibilities.
    /// Returns true if neighbor changed (or false if unchanged).
    /// Returns false as contradiction via caller when neighbor becomes empty.
    /// </summary>
    private bool EnforceNeighborConstraint(Vector2Int cur, Vector2Int nb, Dir dirToNb, bool enqueueOnChange, Queue<Vector2Int> q)
    {
        // Neighbor options must be compatible with at least one option in current cell.
        HashSet<int> allowedForNeighbor = null;

        foreach (int curOpt in _wave[cur.x, cur.y])
        {
            var compatSet = _compat[(int)dirToNb][curOpt]; // allowed neighbor indices for this curOpt in this direction
            if (allowedForNeighbor == null)
                allowedForNeighbor = new HashSet<int>(compatSet);
            else
                allowedForNeighbor.UnionWith(compatSet);
        }

        if (allowedForNeighbor == null)
            allowedForNeighbor = new HashSet<int>(); // should not happen

        int before = _wave[nb.x, nb.y].Count;
        _wave[nb.x, nb.y].IntersectWith(allowedForNeighbor);
        int after = _wave[nb.x, nb.y].Count;

        if (after < before && enqueueOnChange)
            q.Enqueue(nb);

        return after != before;
    }

    // ===== Options + Compatibility =====

    private void BuildOptionSet()
    {
        _allOptions = new List<Option>(roadPrefabs.Count * 4);

        foreach (var prefab in roadPrefabs)
        {
            if (prefab == null) continue;

            for (int rot = 0; rot < 4; rot++)
                _allOptions.Add(new Option(prefab, rot));
        }
    }

    private void BuildCompatibilityRules()
    {
        // For each direction and each optionIndex, store which neighbor optionIndices are allowed.
        _compat = new Dictionary<int, HashSet<int>>[4];
        for (int d = 0; d < 4; d++)
            _compat[d] = new Dictionary<int, HashSet<int>>(_allOptions.Count);

        for (int i = 0; i < _allOptions.Count; i++)
        {
            for (int d = 0; d < 4; d++)
                _compat[d][i] = new HashSet<int>();
        }

        // Rule: side types must match.
        // If current cell has side X in direction dir, neighbor must have side X on opposite side.
        for (int i = 0; i < _allOptions.Count; i++)
        {
            Option a = _allOptions[i];

            RoadLaneType aUp = GetSide(a, Dir.Up);
            RoadLaneType aRight = GetSide(a, Dir.Right);
            RoadLaneType aDown = GetSide(a, Dir.Down);
            RoadLaneType aLeft = GetSide(a, Dir.Left);

            for (int j = 0; j < _allOptions.Count; j++)
            {
                Option b = _allOptions[j];

                // b is neighbor of a in each direction
                if (aUp == GetSide(b, Dir.Down))   _compat[(int)Dir.Up][i].Add(j);
                if (aRight == GetSide(b, Dir.Left)) _compat[(int)Dir.Right][i].Add(j);
                if (aDown == GetSide(b, Dir.Up))   _compat[(int)Dir.Down][i].Add(j);
                if (aLeft == GetSide(b, Dir.Right)) _compat[(int)Dir.Left][i].Add(j);
            }
        }
    }

    // ===== Border constraints =====

    private void ApplyBorderConstraints()
    {
        for (int x = 0; x < _width; x++)
        for (int y = 0; y < _height; y++)
        {
            bool isTop = (y == _height - 1);
            bool isBottom = (y == 0);
            bool isLeft = (x == 0);
            bool isRight = (x == _width - 1);

            if (!isTop && !isBottom && !isLeft && !isRight) 
                continue;

            var cellSet = _wave[x, y];
            var toRemove = new List<int>();

            foreach (int optIndex in cellSet)
            {
                Option opt = _allOptions[optIndex];

                if (isTop && GetSide(opt, Dir.Up) != RoadLaneType.NoneLine) toRemove.Add(optIndex);
                if (isBottom && GetSide(opt, Dir.Down) != RoadLaneType.NoneLine) toRemove.Add(optIndex);
                if (isLeft && GetSide(opt, Dir.Left) != RoadLaneType.NoneLine) toRemove.Add(optIndex);
                if (isRight && GetSide(opt, Dir.Right) != RoadLaneType.NoneLine) toRemove.Add(optIndex);
            }

            for (int i = 0; i < toRemove.Count; i++)
                cellSet.Remove(toRemove[i]);

            // ВАЖНО: если клетка стала пустой — это сразу противоречие
            if (cellSet.Count == 0)
            {
                Debug.LogError($"[WFC] Border constraint emptied cell ({x},{y}). Check tileset/border rules.");
            }
        }
    }
    // ===== Spawn =====

    private void SpawnResult()
    {
        _spawnedLights.Clear();

        // Optional: clear previous children
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        for (int x = 0; x < _width; x++)
        for (int y = 0; y < _height; y++)
        {
            int collapsed = _collapsed[x, y];
            if (collapsed == -1)
            {
                // In theory shouldn't happen if WFC succeeded.
                continue;
            }

            Option opt = _allOptions[collapsed];
            if (opt.Prefab == null) continue;

            Vector3 worldPos = new Vector3(x * cellSize, 0f, y * cellSize);
            Quaternion rot = Quaternion.Euler(0f, opt.Rot * 90f, 0f);

            RoadPiece instance = Instantiate(opt.Prefab, worldPos, rot, transform);

            RealTrafficLight[] lights = instance.GetComponentsInChildren<RealTrafficLight>(true);
            if (lights != null && lights.Length > 0)
                _spawnedLights.AddRange(lights);
        }
    }

    // ===== Helpers =====

    private bool InBounds(Vector2Int p) =>
        p.x >= 0 && p.x < _width && p.y >= 0 && p.y < _height;

    private int ChooseWeighted(HashSet<int> options)
    {
        // Weighted random by prefab weight
        int total = 0;
        foreach (int i in options)
            total += _allOptions[i].Weight;

        int r = _rng.Next(0, Mathf.Max(1, total));
        foreach (int i in options)
        {
            r -= _allOptions[i].Weight;
            if (r < 0)
                return i;
        }

        // fallback
        foreach (int i in options) return i;
        return 0;
    }

    private RoadLaneType GetSide(Option opt, Dir dir)
    {
        RoadLaneType n = opt.Prefab.North;
        RoadLaneType e = opt.Prefab.East;
        RoadLaneType s = opt.Prefab.South;
        RoadLaneType w = opt.Prefab.West;

        // rotate 90° steps clockwise
        for (int i = 0; i < opt.Rot; i++)
        {
            RoadLaneType temp = n;
            n = w;
            w = s;
            s = e;
            e = temp;
        }

        switch (dir)
        {
            case Dir.Up: return n;
            case Dir.Right: return e;
            case Dir.Down: return s;
            case Dir.Left: return w;
            default: return RoadLaneType.NoneLine;
        }
    }

    private Vector2Int CalculateGridSize(int segmentsCount)
    {
        int bestWidth = 1;
        int bestHeight = segmentsCount;
        int bestDiff = segmentsCount;

        for (int w = 1; w * w <= segmentsCount; w++)
        {
            if (segmentsCount % w != 0)
                continue;

            int h = segmentsCount / w;
            int diff = Mathf.Abs(w - h);

            if (diff < bestDiff)
            {
                bestDiff = diff;
                bestWidth = w;
                bestHeight = h;
            }
        }

        return new Vector2Int(bestWidth, bestHeight);
    }
}