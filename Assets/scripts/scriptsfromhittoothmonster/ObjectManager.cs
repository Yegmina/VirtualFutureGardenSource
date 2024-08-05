using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    public GameObject targetObject; // Объект, который будет отключаться
    private string prefsKey;

    void Start()
    {
        // Уникальный ключ для сохранения состояния объекта
        prefsKey = targetObject.name + "_disabled";

        // Проверка сохраненного состояния объекта
        if (PlayerPrefs.GetInt(prefsKey, 0) == 1)
        {
            targetObject.SetActive(false);
        }
    }

    public void DisableObject(GameObject obj)
    {
        if (obj != null)
        {
            obj.SetActive(false);

            // Сохранение состояния объекта
            PlayerPrefs.SetInt(prefsKey, 1);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogWarning("The object to disable is null.");
        }
    }
}
