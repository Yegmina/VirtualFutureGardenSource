using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BlinkingText : MonoBehaviour
{   public Text textToBlink; // Ссылка на компонент Text
    public float blinkDuration = 1.0f; // Длительность одного цикла мигания (в секундах)
    public float minAlpha = 0.0f; // Минимальная прозрачность
    public float maxAlpha = 1.0f; // Максимальная прозрачность

    void Start()
    {
        if (textToBlink != null)
        {
            StartCoroutine(Blink());
        }
        else
        {
            Debug.LogWarning("Text component is not assigned.");
        }
    }

    IEnumerator Blink()
    {
        Color originalColor = textToBlink.color;
        float halfDuration = blinkDuration / 2;

        while (true)
        {
            // Плавное увеличение прозрачности
            for (float t = 0; t < halfDuration; t += Time.deltaTime)
            {
                float normalizedTime = t / halfDuration;
                Color newColor = originalColor;
                newColor.a = Mathf.Lerp(minAlpha, maxAlpha, normalizedTime);
                textToBlink.color = newColor;
                yield return null;
            }

            // Плавное уменьшение прозрачности
            for (float t = 0; t < halfDuration; t += Time.deltaTime)
            {
                float normalizedTime = t / halfDuration;
                Color newColor = originalColor;
                newColor.a = Mathf.Lerp(maxAlpha, minAlpha, normalizedTime);
                textToBlink.color = newColor;
                yield return null;
            }
        }
    }
}