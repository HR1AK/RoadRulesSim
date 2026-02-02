using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class CarController : MonoBehaviour
{
    public enum Axel
    {
        Front,
        Rear,
    }

    [Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public Axel axel;
    }

    public float maxAcceleration = 800.0f; 
    public float brakeAcceleration = 50.0f;
    public float turnSensetivity = 1.0f;
    public float maxSteerAngle = 25.0f;

    //============== SPEEDs ========================================================
    static public float maxSpeed = 150.0f; // Максимальная скорость в км/ч (150 км/ч)
    static public float maxReverseSpeed = 50.0f; // Максимальная скорость заднего хода в км/ч
    public float maxSpeedInMetersPerSecond = maxSpeed * 1000 / 3600; // 150 км/ч -> 41.67 м/с
    public float maxReverseSpeedInMetersPerSecond = maxReverseSpeed * 1000 / 3600; // 50 км/ч -> 13.89 м/с

    //============== OTHER ========================================================
    protected UnityEngine.Vector3 centerOfMass = new UnityEngine.Vector3(0, -0.5f, 0f);
    public List<Wheel> wheels;

    protected float moveInput;
    protected float steerInput;
    protected float brakeInput;
    public Rigidbody carRb;
    protected float currentSpeed;

    protected void Start()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = centerOfMass;  // Центр масс, чтобы машина не переворачивалась при ускорении
    }

    protected void Update()
    {
        GetInputs();
        AnimateWheels();
    }

    protected void LateUpdate()
    {
        Move();
        Steer();
        Brake();
    }

    // ------------------- РУЧНОЕ УПРАВЛЕНИЕ -------------------------
    protected void GetInputs()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }

    // ------------------ УПРАВЛЕНИЕ ЧЕРЕЗ АГЕНТА --------------------
    // public void SetInputs(float move, float steer, float brake)
    // {
    //         moveInput = Mathf.Clamp(move, -1f, 1f);
    //         steerInput = Mathf.Clamp(steer, -1f, 1f);
    //         brakeInput = Mathf.Clamp(brake, 0f, 1f);
    // }

    protected void Move()
    {
        float torque = moveInput * maxAcceleration;

        currentSpeed = carRb.velocity.magnitude;
        if (moveInput > 0 && currentSpeed > maxSpeedInMetersPerSecond)
        {
            torque = 0;
        }

        else if (moveInput < 0 && currentSpeed < -maxReverseSpeedInMetersPerSecond)
        {
            torque = 0; 
            
        }

        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Rear){
                wheel.wheelCollider.motorTorque = torque;
            }
            //wheel.wheelCollider.motorTorque = torque;
        }
    }

    protected void Steer()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                var steerAngle = steerInput * turnSensetivity * maxSteerAngle;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, steerAngle, 0.6f);
            }
        }
    }

    // -------------- РУЧНОЕ ТОРМОЖЕНИЕ ЧЕРЕЗ ПРОБЕЛ -----------------------------
    // protected void Brake()
    // {
    //     if (Input.GetKey(KeyCode.Space))
    //     {
    //         foreach (var wheel in wheels)
    //         {
    //             wheel.wheelCollider.brakeTorque = 300 * brakeAcceleration * Time.deltaTime;
    //         }
    //     }
    //     else
    //     {
    //         foreach (var wheel in wheels)
    //         {
    //             wheel.wheelCollider.brakeTorque = 0;
    //         }
    //     }
    // }


    //------------------ УПРАВЛЕНИЕ ЧЕРЕЗ АГЕНТА --------------------
    protected void Brake()
    {
        float brakeTorqueValue = brakeInput * 300 * brakeAcceleration * Time.deltaTime;

        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.brakeTorque = brakeTorqueValue;
        }
    }

    protected void AnimateWheels()
    {
        foreach (var wheel in wheels)
        {
            UnityEngine.Quaternion rot;
            UnityEngine.Vector3 pos;

            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;
        }
    }
}
