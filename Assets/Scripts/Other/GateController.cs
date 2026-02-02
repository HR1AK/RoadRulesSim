using UnityEngine;

public class GateController: MonoBehaviour
{
    [Header("Настройки шлагбаума")]
    [SerializeField] private float raiseAngle = -90f;     // Угол подъёма
    [SerializeField] private float rotationSpeed = 2f;  // Скорость поворота
    
    [Header("Ссылки")]
    [SerializeField] private Transform pivot;           // Объект, который нужно вращать (ось)
    
    private float currentAngle = 0f;
    private bool shouldRaise = false;

    private void Update()
    {
        // Плавное вращение
        float targetAngle = shouldRaise ? raiseAngle : 0f;
        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
        pivot.localRotation = Quaternion.Euler(currentAngle, 0f, 0f);
    }

    // Машина заехала в триггер
    private void OnTriggerEnter(Collider other)
    {
        shouldRaise = true;
    }

    // Машина выехала из триггера
    private void OnTriggerExit(Collider other)
    {
        shouldRaise = false;
    }
}