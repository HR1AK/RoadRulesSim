using System;
using System.Collections.Generic;
using UnityEngine;

public class BotCarController : MonoBehaviour
{
    public enum Axel
    {
        Front,
        Rear
    }

    [Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public Axel axel;
    }

    [Header("Motor")]
    [SerializeField] private float maxAcceleration = 500f;
    [SerializeField] private float brakeTorque = 3500f;

    [Header("Steering")]
    [SerializeField] private float maxSteerAngle = 22f;
    [SerializeField] private float steerSmoothSpeed = 4f;

    [Header("Stability")]
    [SerializeField] private Vector3 centerOfMass = new Vector3(0f, -0.8f, 0f);
    [SerializeField] private float downforce = 60f;

    [Header("Wheels")]
    [SerializeField] private List<Wheel> wheels = new();

    private Rigidbody rb;

    private float moveInput;
    private float steerInput;
    private float brakeInput;

    public float CurrentSpeedKmh
    {
        get
        {
            if (rb == null)
                return 0f;

            return rb.velocity.magnitude * 3.6f;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
            rb.centerOfMass = centerOfMass;
    }

    private void FixedUpdate()
    {
        ApplyMotor();
        ApplySteering();
        ApplyBrakes();
        ApplyDownforce();
    }

    private void Update()
    {
        AnimateWheels();
    }

    public void SetInputs(float move, float steer, float brake)
    {
        moveInput = Mathf.Clamp(move, -1f, 1f);
        steerInput = Mathf.Clamp(steer, -1f, 1f);
        brakeInput = Mathf.Clamp01(brake);
    }

    private void ApplyMotor()
    {
        float torque = moveInput * maxAcceleration;

        foreach (Wheel wheel in wheels)
        {
            if (wheel.wheelCollider == null)
                continue;

            if (wheel.axel == Axel.Rear)
                wheel.wheelCollider.motorTorque = torque;
            else
                wheel.wheelCollider.motorTorque = 0f;
        }
    }

    private void ApplySteering()
    {
        float targetSteerAngle = steerInput * maxSteerAngle;

        foreach (Wheel wheel in wheels)
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

    private void ApplyBrakes()
    {
        float currentBrakeTorque = brakeInput * brakeTorque;

        foreach (Wheel wheel in wheels)
        {
            if (wheel.wheelCollider == null)
                continue;

            wheel.wheelCollider.brakeTorque = currentBrakeTorque;
        }
    }

    private void ApplyDownforce()
    {
        if (rb == null)
            return;

        rb.AddForce(
            -transform.up * downforce * rb.velocity.magnitude,
            ForceMode.Force
        );
    }

    private void AnimateWheels()
    {
        foreach (Wheel wheel in wheels)
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