using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class PlayFabStatisticManager : MonoBehaviour
{
    public void IncrementStatistic(string statisticName)
    {
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            Debug.LogWarning("Player not logged in. Please log in before attempting to modify statistics.");
            return;
        }

        GetPlayerStatistic(statisticName);
    }

    private void GetPlayerStatistic(string statisticName)
    {
        var request = new GetPlayerStatisticsRequest();
        PlayFabClientAPI.GetPlayerStatistics(request, result =>
        {
            int currentValue = 0;
            bool statisticFound = false;

            foreach (var statistic in result.Statistics)
            {
                if (statistic.StatisticName == statisticName)
                {
                    currentValue = statistic.Value;
                    statisticFound = true;
                    break;
                }
            }

            if (!statisticFound)
            {
                Debug.Log($"Statistic '{statisticName}' not found. Initializing to 0.");
            }

            UpdatePlayerStatistic(statisticName, currentValue + 1);
        },
        error =>
        {
            Debug.LogError("Error retrieving player statistics: " + error.GenerateErrorReport());
        });
    }

    private void UpdatePlayerStatistic(string statisticName, int newValue)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = statisticName,
                    Value = newValue
                }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, result =>
        {
            Debug.Log($"Statistic '{statisticName}' updated successfully. New value: {newValue}");
        },
        error =>
        {
            Debug.LogError("Error updating player statistic: " + error.GenerateErrorReport());
        });
    }
}
