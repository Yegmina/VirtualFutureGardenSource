using UnityEngine;

public class FixAspectRatio : MonoBehaviour
{
    void Start()
    {
        // Установите желаемое соотношение сторон (например, 9:16 для вертикальной ориентации)
        float targetAspect = 9.0f / 16.0f;

        // Получите текущее соотношение сторон экрана
        float windowAspect = (float)Screen.width / (float)Screen.height;

        // Вычислите масштабируемый коэффициент
        float scaleHeight = windowAspect / targetAspect;

        // Получите камеру
        Camera camera = Camera.main;

        // Если масштабируемый коэффициент меньше 1, нужно добавить черные полосы по вертикали
        if (scaleHeight < 1.0f)
        {
            Rect rect = camera.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;

            camera.rect = rect;
        }
        else // Если масштабируемый коэффициент больше 1, нужно добавить черные полосы по горизонтали
        {
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = camera.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;

            camera.rect = rect;
        }
		
		
    }
}
