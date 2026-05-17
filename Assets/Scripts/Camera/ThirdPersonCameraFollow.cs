using UnityEngine;

public class ThirdPersonCameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Camera position")]
    [SerializeField] private float distance = 7f;
    [SerializeField] private float height = 3f;
    [SerializeField] private float sideOffset = 0f;
    [SerializeField] private float lookHeight = 1.2f;

    [Header("Smooth settings")]
    [SerializeField] private float positionSmoothTime = 0.12f;
    [SerializeField] private float rotationSmoothSpeed = 10f;
    [SerializeField] private float autoReturnSpeed = 6f;

    [Header("Manual camera rotation")]
    [SerializeField] private KeyCode orbitKey = KeyCode.LeftControl;
    [SerializeField] private bool allowRightControl = true;
    [SerializeField] private float mouseSensitivityX = 3f;
    [SerializeField] private float mouseSensitivityY = 2f;
    [SerializeField] private float minPitch = 8f;
    [SerializeField] private float maxPitch = 35f;
    [SerializeField] private float defaultPitch = 18f;

    private Vector3 currentVelocity;
    private float yaw;
    private float pitch;

    private void Start()
    {
        if (target != null)
        {
            yaw = target.eulerAngles.y;
            pitch = defaultPitch;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        UpdateCameraRotation();
        UpdateCameraPosition();
        UpdateCameraLook();
    }

    private void UpdateCameraRotation()
    {
        bool isOrbitKeyPressed =
            Input.GetKey(orbitKey) ||
            (allowRightControl && Input.GetKey(KeyCode.RightControl));

        if (isOrbitKeyPressed)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            yaw += mouseX * mouseSensitivityX;
            pitch -= mouseY * mouseSensitivityY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
        else
        {
            yaw = Mathf.LerpAngle(
                yaw,
                target.eulerAngles.y,
                autoReturnSpeed * Time.deltaTime
            );

            pitch = Mathf.Lerp(
                pitch,
                defaultPitch,
                autoReturnSpeed * Time.deltaTime
            );
        }
    }

    private void UpdateCameraPosition()
    {
        Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);

        Vector3 targetPosition =
            target.position
            - yawRotation * Vector3.forward * distance
            + Vector3.up * height
            + yawRotation * Vector3.right * sideOffset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            positionSmoothTime
        );
    }

    private void UpdateCameraLook()
    {
        Vector3 lookPoint = target.position + Vector3.up * lookHeight;

        Quaternion pitchRotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 directionToLookPoint = lookPoint - transform.position;

        if (directionToLookPoint.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(directionToLookPoint.normalized);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSmoothSpeed * Time.deltaTime
        );
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        if (target != null)
        {
            yaw = target.eulerAngles.y;
            pitch = defaultPitch;
        }
    }
}