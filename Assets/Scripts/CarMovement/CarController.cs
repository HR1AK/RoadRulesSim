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

    [Header("Motor")]
    public float maxAcceleration = 900f;
    public float reverseAcceleration = 500f;

    [Header("Brakes")]
    public float brakeTorque = 3000f;
    public float handbrakeTorque = 8000f;
    public float idleBrakeTorque = 150f;

    [Header("Steering")]
    public float turnSensetivity = 1.0f;
    public float maxSteerAngle = 28.0f;
    public float steerSmoothSpeed = 8f;
    public float minSteerMultiplierAtMaxSpeed = 0.35f;

    [Header("Speed limits")]
    public static float maxSpeed = 150.0f;        // км/ч
    public static float maxReverseSpeed = 40.0f;  // км/ч

    [Header("Stability")]
    public Vector3 centerOfMass = new Vector3(0f, -0.6f, 0f);
    public float downforce = 50f;

    [Header("Wheels")]
    public List<Wheel> wheels;

    protected float moveInput;
    protected float steerInput;
    protected float brakeInput;
    protected float handbrakeInput;

    public Rigidbody carRb;

    protected float currentSpeed;
    protected float forwardSpeed;

    public float CurrentSpeedKmh => currentSpeed * 3.6f;
    public float ForwardSpeedKmh => forwardSpeed * 3.6f;

    protected virtual void Start()
    {
        carRb = GetComponent<Rigidbody>();

        if (carRb != null)
        {
            carRb.centerOfMass = centerOfMass;
        }
    }

    protected virtual void Update()
    {
        GetInputs();
        AnimateWheels();
    }

    protected virtual void FixedUpdate()
    {
        UpdateSpeedValues();

        ApplyDownforce();
        Move();
        Steer();
        Brake();
    }

    protected virtual void GetInputs()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");

        // Ручник / резкое торможение
        handbrakeInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;
    }

    // В будущем пригодится для ИИ-автомобилей
    public void SetInputs(float move, float steer, float brake, float handbrake = 0f)
    {
        moveInput = Mathf.Clamp(move, -1f, 1f);
        steerInput = Mathf.Clamp(steer, -1f, 1f);
        brakeInput = Mathf.Clamp01(brake);
        handbrakeInput = Mathf.Clamp01(handbrake);
    }

    protected void UpdateSpeedValues()
    {
        currentSpeed = carRb.velocity.magnitude;

        // Скорость именно вдоль направления машины.
        // Если значение положительное — машина едет вперёд.
        // Если отрицательное — назад.
        forwardSpeed = Vector3.Dot(transform.forward, carRb.velocity);
    }

    protected void Move()
    {
        float maxSpeedMps = maxSpeed / 3.6f;
        float maxReverseSpeedMps = maxReverseSpeed / 3.6f;

        float torque = 0f;

        bool wantsForward = moveInput > 0.05f;
        bool wantsReverse = moveInput < -0.05f;

        if (handbrakeInput > 0.1f)
        {
            torque = 0f;
        }
        else if (wantsForward)
        {
            if (forwardSpeed < maxSpeedMps)
            {
                torque = moveInput * maxAcceleration;
            }
        }
        else if (wantsReverse)
        {
            // Если машина ещё едет вперёд, сначала тормозим, а не сразу включаем задний ход
            if (forwardSpeed > 0.5f)
            {
                torque = 0f;
            }
            else if (Mathf.Abs(forwardSpeed) < maxReverseSpeedMps)
            {
                torque = moveInput * reverseAcceleration;
            }
        }

        foreach (var wheel in wheels)
        {
            if (wheel.wheelCollider == null)
                continue;

            if (wheel.axel == Axel.Rear)
            {
                wheel.wheelCollider.motorTorque = torque;
            }
            else
            {
                wheel.wheelCollider.motorTorque = 0f;
            }
        }
    }

    protected void Steer()
    {
        float maxSpeedMps = maxSpeed / 3.6f;
        float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeedMps);

        // На большой скорости угол поворота должен быть меньше
        float steerMultiplier = Mathf.Lerp(
            1f,
            minSteerMultiplierAtMaxSpeed,
            Mathf.Sqrt(speedFactor)
        );

        float targetSteerAngle =
            steerInput *
            turnSensetivity *
            maxSteerAngle *
            steerMultiplier;

        foreach (var wheel in wheels)
        {
            if (wheel.wheelCollider == null)
                continue;

            if (wheel.axel == Axel.Front)
            {
                wheel.wheelCollider.steerAngle = Mathf.Lerp(
                    wheel.wheelCollider.steerAngle,
                    targetSteerAngle,
                    steerSmoothSpeed * Time.fixedDeltaTime
                );
            }
        }
    }

    protected void Brake()
    {
        float finalBrakeTorque = 0f;

        bool wantsForward = moveInput > 0.05f;
        bool wantsReverse = moveInput < -0.05f;

        // Нажал назад, когда едешь вперёд — обычное торможение
        if (wantsReverse && forwardSpeed > 0.5f)
        {
            finalBrakeTorque = brakeTorque * Mathf.Abs(moveInput);
        }

        // Нажал вперёд, когда катишься назад — тоже торможение
        if (wantsForward && forwardSpeed < -0.5f)
        {
            finalBrakeTorque = brakeTorque * Mathf.Abs(moveInput);
        }

        // Торможение от внешнего агента, пригодится для ИИ
        finalBrakeTorque = Mathf.Max(finalBrakeTorque, brakeInput * brakeTorque);

        // Если газ не нажат, машина чуть-чуть удерживается тормозом,
        // чтобы не катиться бесконечно
        if (Mathf.Abs(moveInput) < 0.05f && handbrakeInput < 0.1f)
        {
            finalBrakeTorque = Mathf.Max(finalBrakeTorque, idleBrakeTorque);
        }

        foreach (var wheel in wheels)
        {
            if (wheel.wheelCollider == null)
                continue;

            float wheelBrakeTorque = finalBrakeTorque;

            // Ручник действует в первую очередь на задние колёса
            if (wheel.axel == Axel.Rear)
            {
                wheelBrakeTorque += handbrakeInput * handbrakeTorque;
            }

            wheel.wheelCollider.brakeTorque = wheelBrakeTorque;
        }
    }

    protected void ApplyDownforce()
    {
        if (carRb == null)
            return;

        carRb.AddForce(
            -transform.up * downforce * carRb.velocity.magnitude,
            ForceMode.Force
        );
    }

    protected void AnimateWheels()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.wheelCollider == null || wheel.wheelModel == null)
                continue;

            Quaternion rot;
            Vector3 pos;

            wheel.wheelCollider.GetWorldPose(out pos, out rot);

            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;
        }
    }
}