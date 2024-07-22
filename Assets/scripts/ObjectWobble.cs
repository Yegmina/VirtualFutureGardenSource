using UnityEngine;

public class ObjectWobble : MonoBehaviour
{
    public float wobbleAmount = 15.0f; // Максимальный угол отклонения в градусах
    public float wobbleSpeed = 1.0f; // Скорость покачивания

    private Vector3 initialRotation;
    private float wobbleTime;

    void Start()
    {
        initialRotation = transform.eulerAngles;
        wobbleTime = Random.Range(0, Mathf.PI * 2); // Случайное начальное значение для разнообразия движения
    }

    void Update()
    {
        // Вычисляем новое значение угла покачивания
        float wobbleX = Mathf.Sin(wobbleTime * wobbleSpeed) * wobbleAmount;
        float wobbleY = Mathf.Cos(wobbleTime * wobbleSpeed) * wobbleAmount;

        // Применяем покачивание к вращению объекта
        transform.eulerAngles = initialRotation + new Vector3(wobbleX, wobbleY, 0);

        // Обновляем время для следующего кадра
        wobbleTime += Time.deltaTime;
    }
}
