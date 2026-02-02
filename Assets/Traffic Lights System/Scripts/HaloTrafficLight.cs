// using UnityEngine;
// using System.Collections;

// namespace HealthbarGames
// {
//     public class HaloTrafficLight : TrafficLightBase
//     {
//         public Renderer RedRenderer;
//         public GameObject RedHalo;

//         public Renderer YellowRenderer;
//         public GameObject YellowHalo;

//         public Renderer GreenRenderer;
//         public GameObject GreenHalo;

//         public Material LightsOnMat;
//         public Material LightsOffMat;

//         private bool mInitialized = false;

//         void Awake()
//         {
//             if (    (RedRenderer != null || RedHalo != null)
//                 &&  (YellowRenderer != null || YellowHalo != null)
//                 &&  (GreenRenderer != null || GreenHalo != null)
//                 )
//             {
//                 mInitialized = true;
//             }
//             else
//             {
//                 mInitialized = false;
//                 Debug.LogError("Some variables haven't been assigned correctly for HaloTrafficLight script.", this);
//             }
//         }

//         // implementation of the callback from TrafficLight - called when lights state has changed
//         public override void OnLightStateChanged(bool redLightState, bool yellowLightState, bool greenLightState)
//         {
//             if (!mInitialized)
//                 return;
//             if (RedHalo != null)
//                 RedHalo.SetActive(redLightState);

//             if (RedRenderer != null)
//                 RedRenderer.material = (redLightState) ? LightsOnMat : LightsOffMat;

//             if (YellowHalo != null)
//                 YellowHalo.SetActive(yellowLightState);

//             if (YellowRenderer != null)
//                 YellowRenderer.material = (yellowLightState) ? LightsOnMat : LightsOffMat;

//             if (GreenHalo != null)
//                 GreenHalo.SetActive(greenLightState);

//             if (GreenRenderer != null)
//                 GreenRenderer.material = (greenLightState) ? LightsOnMat : LightsOffMat;
//         }
//     }
// }
