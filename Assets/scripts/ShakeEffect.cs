using UnityEngine;

public class ShakeEffect : MonoBehaviour
{
    public enum ShakeType { Position, Rotation, Both }
    public ShakeType shakeType = ShakeType.Position;

    public float positionAmplitude = 0.1f; // Амплитуда колебаний позиции
    public float rotationAmplitude = 10f; // Амплитуда колебаний ротации (угол в градусах)
    public float frequency = 1f; // Частота колебаний
    public bool randomize = false; // Включение рандомизации
    public float shakeDuration = 5f; // Продолжительность колебаний в секундах

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float randomOffset;
    private float shakeTimer;

    void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        
        if (randomize)
        {
            randomOffset = Random.Range(0f, 2f * Mathf.PI);
        }
        
        shakeTimer = shakeDuration;
    }

    void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

            float offset = (randomize) ? randomOffset : 0f;
            if (shakeType == ShakeType.Position || shakeType == ShakeType.Both)
            {
                ShakePosition(offset);
            }
            if (shakeType == ShakeType.Rotation || shakeType == ShakeType.Both)
            {
                ShakeRotation(offset);
            }

            if (shakeTimer <= 0)
            {
                StopShaking();
            }
        }
    }

    void ShakePosition(float offset)
    {
        Vector3 shakePosition = originalPosition + (Vector3.right * Mathf.Sin(Time.time * frequency + offset) * positionAmplitude);
        transform.localPosition = shakePosition;
    }

    void ShakeRotation(float offset)
    {
        float angle = Mathf.Sin(Time.time * frequency + offset) * rotationAmplitude;
        transform.localRotation = originalRotation * Quaternion.Euler(0, 0, angle);
    }

    // Функция для остановки колебаний
    public void StopShaking()
    {
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        enabled = false;
    }
}
