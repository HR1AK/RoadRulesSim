// using UnityEngine;
// using System.Collections;

// public class GridGenExperimentRunner : MonoBehaviour
// {
//     [SerializeField] private FullWfcCityGenerator generator;
//     [SerializeField] private int runs = 100;
//     [SerializeField] private int seedStart = 1;
//     [SerializeField] private bool logEachRun = false;


//     private IEnumerator Start()
//     {
//         if (generator == null)
//         {
//             Debug.LogError("[Experiment] Generator is not assigned.");
//             yield break;
//         }

//         int sumEmpty = 0;
//         int totalCells = 0;

//         for (int i = 0; i < runs; i++)
//         {
//             int seed = seedStart + i;

//             generator.ClearGenerated();
//             int empty = generator.GenerateOnce(seed);

//             sumEmpty += empty;
//             totalCells = generator.lastTotalCells;

//             if (logEachRun)
//                 Debug.Log($"[Experiment] Run {i + 1}/{runs}, seed={seed}, empty={empty}/{totalCells}");

//             // дать Unity кадр, чтобы не подвесить редактор (если надо)
//             yield return null;
//         }

//         float avgEmpty = (float)sumEmpty / runs;
//         float avgFillPct = totalCells > 0 ? (1f - avgEmpty / totalCells) * 100f : 0f;

//         Debug.Log($"[Experiment] DONE. runs={runs}, avg empty cells={avgEmpty:F2} of {totalCells}, avg filled={avgFillPct:F2}%");
//     }
// }