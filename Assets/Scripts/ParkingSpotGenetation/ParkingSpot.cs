using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingSpot : MonoBehaviour
{
    public Vector3 position;
    public Quaternion rotation;
    public bool isFree = true;
    public bool isForDisabled = false;

    void Start()
    {
        position = transform.localPosition;
        rotation = transform.localRotation;
    }

    void OnTriggerEnter(Collider other)
    {
        isFree = false;
    }

    void OnTriggerExit(Collider other)
    {
        isFree = true;
    }
}
