using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public GameObject tutorialHint; // Объект подсказки
    public Button tutorialButton; // Кнопка для завершения туториала
    public bool isTutorialStageReached; // Публичная переменная для проверки этапа туториала
    public string tutorialId; // Уникальный идентификатор для каждого туториала

    private void Start()
    {
        // Проверяем, был ли показан туториал раньше
        if (PlayerPrefs.GetInt(tutorialId, 0) == 0 && isTutorialStageReached)
        {
            tutorialHint.SetActive(true);
        }
        else
        {
            tutorialHint.SetActive(false);
        }

        // Привязываем метод к нажатию на кнопку
        if (tutorialButton != null)
        {
            tutorialButton.onClick.AddListener(OnTutorialButtonClick);
        }
    }

    private void OnTutorialButtonClick()
    {
        StartCoroutine(DisableTutorialWithDelay());
    }

    // Публичная функция для отключения туториала и сохранения состояния
    public void DisableTutorial()
    {
        tutorialHint.SetActive(false);
        PlayerPrefs.SetInt(tutorialId, 1);
        PlayerPrefs.Save();
        Debug.Log("Tutorial disabled and state saved.");
    }

    private IEnumerator DisableTutorialWithDelay()
    {
        yield return new WaitForSeconds(0.3f);
        DisableTutorial();
    }

    // Публичная функция для изменения переменной isTutorialStageReached
    public void SetTutorialStageReached(bool reached)
    {
        isTutorialStageReached = reached;
        if (reached)
        {
            Debug.Log("Tutorial stage reached. Checking if tutorial should be shown.");
            if (PlayerPrefs.GetInt(tutorialId, 0) == 0)
            {
                tutorialHint.SetActive(true);
            }
        }
        else
        {
            tutorialHint.SetActive(false);
        }
    }
}
