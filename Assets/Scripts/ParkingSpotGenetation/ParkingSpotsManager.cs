using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class ParkingSpotsManager : MonoBehaviour
{
    public List<ParkingSpot> spots; //все места
    public List<ParkingSpot> freeSpots = new List<ParkingSpot>(); //свободные для агента
    List<ParkingSpot> blockedSpots = new List<ParkingSpot>(); //занятые места (спавним там машины)
    public List<GameObject> cars;
    void Start()
    {

        //spots = new List<ParkingSpot>(FindObjectsOfType<ParkingSpot>());
        //spawnCars();
    }

    public void spawnCars()
    {
            // Фиксируем количество свободных мест (3-7)
        int countFreeSpot = Random.Range(3, 7); 

        // 1. Помечаем ВСЕ места как свободные (сброс)
        foreach (var spot in spots)
        {
            spot.isFree = true;
        }

        // 2. Занятые места выбираем без повторений
        List<ParkingSpot> availableSpots = new List<ParkingSpot>(spots);
        for (int i = 0; i < 28 - countFreeSpot; i++)
        {
            if (availableSpots.Count == 0) break;

            int randIndex = Random.Range(0, availableSpots.Count);
            ParkingSpot spot = availableSpots[randIndex];
            spot.isFree = false;
            availableSpots.RemoveAt(randIndex); // Убираем место из доступных
        }

        // 3. Заполняем blockedSpots (занятые) и freeSpots (только НЕ инвалидные + свободные)
        foreach (var spot in spots)
        {
            if (!spot.isFree)
            {
                blockedSpots.Add(spot);
                int randomIndex = Random.Range(0, cars.Count);
                GameObject randomCarPrefab = cars[randomIndex];
                Vector3 carPosition = new Vector3(spot.position.x, 1.2f, spot.position.z);
                Instantiate(randomCarPrefab, carPosition, spot.rotation);
            }
            else if (!spot.isForDisabled) // Только свободные + НЕ инвалидные
            {
                freeSpots.Add(spot);
            }
        }

    }

    public void deleteCars()
    {
        GameObject[] cars = GameObject.FindGameObjectsWithTag("emptyCar");
        foreach (GameObject car in cars)
        {
            Destroy(car);
        }

        freeSpots.Clear();
        blockedSpots.Clear();

        foreach (var spot in spots)
        {
            spot.isFree = true; 
        }
    }

}
