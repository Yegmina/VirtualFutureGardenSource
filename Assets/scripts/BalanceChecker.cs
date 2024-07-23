using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class BalanceChecker : MonoBehaviour
{
    public Text messageText; // Текстовое поле для вывода сообщений
    public GameObject object0; // Объект, который будет выключен
    public GameObject object1; // Объект, который будет включен
    public OxygenCalculator oxygenCalculator; // Ссылка на скрипт OxygenCalculator
    public string statisticName = "HOTBalance"; // Имя статистики в PlayFab
    public float boost = 1f; // Значение boost, устанавливаемое через Inspector или другой метод
	public TutorialManager tutorialManager; // Drag and drop your TutorialManager script here in the inspector

    // Публичная функция для проверки баланса и изменения boost
    public void CheckBalanceAndSetBoost(int N)
    {
        PlayFabClientAPI.GetPlayerStatistics(
            new GetPlayerStatisticsRequest(),
            result => OnGetPlayerStatistics(result, N),
            error => OnPlayFabError(error)
        );
    }

    // Callback функция при успешном получении статистики игрока
    private void OnGetPlayerStatistics(GetPlayerStatisticsResult result, int N)
    {
        int hotBalance = 0;
        foreach (var stat in result.Statistics)
        {
            if (stat.StatisticName == statisticName)
            {
                hotBalance = stat.Value;
                break;
            }
        }

        if (hotBalance < N)
        {
            messageText.text = "You are not holding enough HOT";
        }
        else
        {
            messageText.text = $"Congratulations! You get X{boost} boost to your oxygen farming and your plant also improved";
            oxygenCalculator.boost = boost;
            object0.SetActive(false);
            object1.SetActive(true);
			
			            // Если достигнут этап туториала, включить подсказку
            tutorialManager.SetTutorialStageReached(true);

        }
    }

    // Callback функция при ошибке получения статистики игрока
    private void OnPlayFabError(PlayFabError error)
    {
        Debug.LogError("Error getting player statistics: " + error.GenerateErrorReport());
    }
}
