// using UnityEngine;
// using Unity.MLAgents;
// using Unity.MLAgents.Actuators;
// using Unity.MLAgents.Sensors;

// public class ParkingAgent : Agent
// {
//     private Car2105Move carController;
//     private ParkingSpotsManager spotsManager;

//     private ParkingSpot targetSpot;

//     //------------------- Start parameters--------------------

//     private Vector3 startPosition;
//     private Quaternion startRotation;

//     public float positionThreshold = 1f; // метры
//     public float angleThreshold = 15f;     // градусы
//     //public float maxAllowedSpeed = 20f;    // км/ч
//     public float collisionPenalty = -0.5f; // штраф за столкновение
//     public float speedPenalty = -0.01f;    // штраф за превышение скорости (каждый кадр)

//     private float previousDistance = Mathf.Infinity;
//     private float previousAngle = Mathf.Infinity;
//     public float fallThresholdY = -1f; // уровень, ниже которого считаем, что упал

//     public override void Initialize()
//     {
//         startPosition = transform.position;
//         startRotation = transform.rotation;

//         carController = GetComponent<Car2105Move>();
//         if (carController == null)
//         {
//             Debug.Log("NuLL carController");
//         }
//         spotsManager = FindAnyObjectByType<ParkingSpotsManager>();
//     }

//     public override void OnEpisodeBegin()
//     {
//         resetEpisod();
//         carController.carRb.velocity = Vector3.zero;
//         carController.carRb.angularVelocity = Vector3.zero;

//         transform.position = startPosition;
//         transform.rotation = startRotation;

//         targetSpot = GetNearestFreeSpot();
//         //targetSpot = spotsManager.spots[Random.Range(0, spotsManager.spots.Count - 1)];

//         //Debug.Log(targetSpot.name);
//         //Debug.Log(spotsManager.spots.Count);
//         previousDistance = Vector3.Distance(startPosition, targetSpot.position);
//         previousAngle = Quaternion.Angle(startRotation, targetSpot.rotation);
//     }

//     public override void CollectObservations(VectorSensor sensor)
//     {
//         if (targetSpot == null) return;

//         // Положение и направление до цели в локальных координатах
//         Vector3 localTargetPos = transform.InverseTransformPoint(targetSpot.position);
//         sensor.AddObservation(localTargetPos.normalized);
//         sensor.AddObservation(localTargetPos.magnitude);

//         // Текущая скорость вперёд (в м/с)
//         float forwardSpeed = Vector3.Dot(carController.carRb.velocity, transform.forward);
//         sensor.AddObservation(forwardSpeed);

//         // Текущий угол поворота автомобиля (yaw), нормированный
//         sensor.AddObservation(transform.rotation.eulerAngles.y / 360f);
//     }

//     public override void OnActionReceived(ActionBuffers actions)
//     {
//         // 1. Применяем действия к машине (ваш исходный код)
//         int moveAction = actions.DiscreteActions[0];
//         int steerAction = actions.DiscreteActions[1];
//         int brakeAction = actions.DiscreteActions[2];

//         float move = 0f;
//         float steer = 0f;
//         float brake = 0f;

//         switch (moveAction)
//         {
//             case 0: move = -1f; break;   // назад
//             case 1: move = 0f; break;    // нейтрально
//             case 2: move = 1f; break;    // вперёд
//         }

//         switch (steerAction)
//         {
//             case 0: steer = -1f; break;  // влево
//             case 1: steer = 0f; break;   // прямо
//             case 2: steer = 1f; break;   // вправо
//         }

//         brake = (brakeAction == 1) ? 1f : 0f;
//         carController.SetInputs(move, steer, brake);

//         // 2. Расчёт расстояния и скорости
//         float dist = Vector3.Distance(transform.position, targetSpot.position);
//         float speed = carController.carRb.velocity.magnitude;

//         // 3. Награда за приближение (с проверкой скорости)
//         if (dist < previousDistance && speed > 0.1f)
//         {
//             AddReward(0.002f); // Поощрение за движение к цели
//         }
//         else if (dist > previousDistance)
//         {
//             AddReward(-0.002f); // Штраф за отдаление
//         }
//         previousDistance = dist;

//         // 4. Штраф за каждый шаг (мотивация быстрее закончить эпизод)
//         AddReward(-0.00005f);

//         // 6. Штраф за неправильный угол
//         // float angleDiff = Quaternion.Angle(transform.rotation, targetSpot.rotation);
//         // Debug.Log(angleDiff);
//         // AddReward(-0.00001f * angleDiff);
//         float angle = Quaternion.Angle(transform.rotation, targetSpot.rotation);
//         if (angle < previousAngle && speed > 0.1f)
//         {
//             AddReward(0.002f); // Поощрение за движение к цели
//         }
//         else if (previousAngle > angle)
//         {
//             AddReward(-0.002f); // Штраф за отдаление
//         }
//         previousAngle = angle;

//         // 7. Проверка успешной парковки (ЯВНЫЙ ВЫЗОВ!) 
//         CheckParkingSuccess();

//         // 8. Штраф за падение (ваш код)
//         if (transform.position.y < fallThresholdY)
//         {
//             AddReward(-2f);
//             EndEpisode();
//         }
//     }

//     // public override void OnActionReceived(ActionBuffers actions)
//     // {
//     //     int moveAction = actions.DiscreteActions[0];
//     //     int steerAction = actions.DiscreteActions[1];
//     //     int brakeAction = actions.DiscreteActions[2];

//     //     float move = 0f;
//     //     float steer = 0f;
//     //     float brake = 0f;

//     //     // Первая ветка — движение
//     //     switch (moveAction)
//     //     {
//     //         case 0: move = -1f; break;   // назад
//     //         case 1: move = 0f; break;    // нейтрально
//     //         case 2: move = 1f; break;    // вперёд
//     //     }

//     //     // Вторая ветка — руление
//     //     switch (steerAction)
//     //     {
//     //         case 0: steer = -1f; break;  // влево
//     //         case 1: steer = 0f; break;   // прямо
//     //         case 2: steer = 1f; break;   // вправо
//     //     }

//     //     // Третья ветка — тормоз
//     //     brake = (brakeAction == 1) ? 1f : 0f;


//     //     carController.SetInputs(move, steer, brake);

//     //     float dist = Vector3.Distance(transform.position, targetSpot.position);
//     //     if (dist < previousDistance)
//     //     {
//     //         AddReward(0.005f); // награда за приближение
//     //     }
//     //     else if (dist > previousDistance)
//     //     {
//     //         AddReward(-0.01f); // штраф за отдаление
//     //     }
//     //     previousDistance = dist;

//     //     if (transform.position.y < fallThresholdY)
//     //     {
//     //         AddReward(-2f);  // большой штраф за падение
//     //         //resetEpisod();
//     //         EndEpisode();    // заканчиваем эпизод
//     //     }

//     //     CheckParkingSuccess();
//     // }

//     public override void Heuristic(in ActionBuffers actionsOut)
//     {
//         var discreteActionsOut = actionsOut.DiscreteActions;

//         // Вертикальное движение
//         float v = Input.GetAxis("Vertical");
//         if (v > 0) discreteActionsOut[0] = 2;
//         else if (v < 0) discreteActionsOut[0] = 0;
//         else discreteActionsOut[0] = 1;

//         // Горизонтальное движение (руление)
//         float h = Input.GetAxis("Horizontal");
//         if (h > 0) discreteActionsOut[1] = 2;
//         else if (h < 0) discreteActionsOut[1] = 0;
//         else discreteActionsOut[1] = 1;

//         // Тормоз
//         discreteActionsOut[2] = Input.GetKey(KeyCode.Space) ? 1 : 0;
//     }

//     private ParkingSpot GetNearestFreeSpot()
//     {
//         ParkingSpot nearest = null;
//         float minDist = Mathf.Infinity;

//         foreach (var spot in spotsManager.freeSpots)
//         {
//             float dist = Vector3.Distance(transform.position, spot.position);
//             if (dist < minDist)
//             {
//                 minDist = dist;
//                 nearest = spot;
//             }
//         }
//         return nearest;
//     }

//     private void CheckParkingSuccess()
//     {
//         if (targetSpot == null) return;

//         float dist = Vector3.Distance(transform.position, targetSpot.position);
//         float angleDiff = Quaternion.Angle(transform.rotation, targetSpot.rotation);
//         float angleDiffReverse = Mathf.Abs(angleDiff - 180f);
    
//         if (dist > positionThreshold)
//             return;

//         if (angleDiff <= angleThreshold || angleDiffReverse <= angleThreshold)
//         {
//             Debug.Log("УРАААААААААААА");
//             AddReward(2.0f); // награда за успешную парковку
//             //resetEpisod();
//             EndEpisode();
//         }
//     }

//     private void OnCollisionEnter(Collision collision)
//     {
//         // Штрафуем за столкновение с машинами и препятствиями
//         if (!collision.gameObject.CompareTag("Road"))
//         {
//             //Debug.Log("Agents Touch something!!!");
//             AddReward(-2);
//             //resetEpisod();
//             EndEpisode();
//         }
//     }

//     private void resetEpisod()
//     {
//         spotsManager.deleteCars();
//         spotsManager.spawnCars();
//     }
// }
