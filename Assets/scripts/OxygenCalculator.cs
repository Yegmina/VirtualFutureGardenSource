using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;

public class OxygenCalculator : MonoBehaviour
{
    public static OxygenCalculator Instance;

    public Text displayOxygen;
    private float currentOxygen = 0f;
    private float lightValue = 0f;
    private float artificialLightValue = 50f;  // Значение по умолчанию

    private bool isPlayerStatisticsLoaded = false;
    private int retryCount = 0;
    private const int maxRetryCount = 5;

    private DateTime lastLogoutTime;
    private double maxOfflineHours = 1; // Значение по умолчанию

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Загружаем статистику игрока
        LoadPlayerStatistics();

        // Начинаем таймер обновления кислорода
        InvokeRepeating("CalculateOxygen", 1.0f, 1.0f);
		
    }

    public void UpdateLightValue(float newLightValue)
    {
        lightValue = newLightValue;
    }

    void CalculateOxygen()
    {
        currentOxygen += (1f / 60f) * (lightValue + artificialLightValue);

        if (displayOxygen != null)
        {
            displayOxygen.text = "Oxygen: " + currentOxygen.ToString("F2") + " [units]";
        }

        Debug.Log("Oxygen value updated: " + currentOxygen);

        // Обновление значения кислорода в PlayFab
        UpdateOxygenStatistic(currentOxygen, artificialLightValue);
    }

    public void LoadPlayerStatistics()
    {
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            Debug.LogWarning("Player not logged in. Retrying in 1 second...");
            Invoke(nameof(LoadPlayerStatistics), 1.0f);
            return;
        }

        var request = new GetPlayerStatisticsRequest();
        PlayFabClientAPI.GetPlayerStatistics(request, OnGetStatisticsSuccess, OnGetStatisticsFailure);
    }

    void OnGetStatisticsSuccess(GetPlayerStatisticsResult result)
    {
        Debug.Log("Player statistics retrieved successfully!");

        foreach (var statistic in result.Statistics)
        {
            if (statistic.StatisticName == "Oxygen")
            {
                currentOxygen = statistic.Value;
                if (displayOxygen != null)
                {
                    displayOxygen.text = "Oxygen: " + currentOxygen.ToString("F2") + " [units]";
                }
                Debug.Log("Current Oxygen value: " + currentOxygen);
            }
            else if (statistic.StatisticName == "ArtificialLight")
            {
                artificialLightValue = statistic.Value;
                Debug.Log("Artificial Light value: " + artificialLightValue);
            }
            else if (statistic.StatisticName == "LastLogoutTime")
            {
                if (long.TryParse(statistic.Value.ToString(), out long logoutTimeSeconds))
                {
                    lastLogoutTime = DateTimeOffset.FromUnixTimeSeconds(logoutTimeSeconds).UtcDateTime;
                    Debug.Log("Last Logout Time: " + lastLogoutTime);
                }
            }
            else if (statistic.StatisticName == "MaxOfflineHours")
            {
                maxOfflineHours = statistic.Value;
                Debug.Log("Max Offline Hours value: " + maxOfflineHours);
            }
        }

        UpdateOxygenForOfflineTime();
        isPlayerStatisticsLoaded = true;
        retryCount = 0;
    }

    void OnGetStatisticsFailure(PlayFabError error)
    {
        Debug.LogError("Error getting player statistics: " + error.GenerateErrorReport());
        if (!PlayFabClientAPI.IsClientLoggedIn() && retryCount < maxRetryCount)
        {
            retryCount++;
            Debug.LogWarning("Retrying to load player statistics in 1 second...");
            Invoke(nameof(LoadPlayerStatistics), 1.0f);
        }
    }

    void UpdateOxygenForOfflineTime()
    {
        DateTime currentTime = DateTime.UtcNow;
        TimeSpan offlineDuration = currentTime - lastLogoutTime;
        double offlineHours = offlineDuration.TotalHours;

        Debug.Log("Current Time: " + currentTime);
        Debug.Log("Offline Duration (hours): " + offlineHours);

        if (offlineHours > maxOfflineHours)
        {
            offlineHours = maxOfflineHours;
        }

        double offlineSeconds = offlineHours * 3600;
        currentOxygen += (float)((offlineSeconds / 60f) * artificialLightValue);

        if (displayOxygen != null)
        {
            displayOxygen.text = "Oxygen: " + currentOxygen.ToString("F2") + " [units]";
        }
		if (currentOxygen<0) {currentOxygen=1;}
        Debug.Log("Oxygen value updated for offline time: " + currentOxygen);
		
    }

    void UpdateOxygenStatistic(float oxygenValue, float artificialLightValue)
    {
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            Debug.LogWarning("Player not logged in. Retrying update in 1 second...");
            Invoke(nameof(RetryUpdateOxygenStatistic), 1.0f);
            return;
        }

        lastLogoutTime = DateTime.UtcNow;
        long lastLogoutTimeSeconds = new DateTimeOffset(lastLogoutTime).ToUnixTimeSeconds();

        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "Oxygen",
                    Value = Mathf.FloorToInt(oxygenValue)
                },
                new StatisticUpdate
                {
                    StatisticName = "ArtificialLight",
                    Value = Mathf.FloorToInt(artificialLightValue)
                },
                new StatisticUpdate
                {
                    StatisticName = "LastLogoutTime",
                    Value = (int)lastLogoutTimeSeconds // Ensure it fits in an int
                },
                new StatisticUpdate
                {
                    StatisticName = "MaxOfflineHours",
                    Value = (int)maxOfflineHours
                }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, OnStatisticsUpdateSuccess, OnStatisticsUpdateFailure);
    }

    void RetryUpdateOxygenStatistic()
    {
        UpdateOxygenStatistic(currentOxygen, artificialLightValue);
    }

    void OnStatisticsUpdateSuccess(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Player statistics updated successfully!");
    }

    void OnStatisticsUpdateFailure(PlayFabError error)
    {
        Debug.LogError("Error updating player statistics: " + error.GenerateErrorReport());
        if (!PlayFabClientAPI.IsClientLoggedIn() && retryCount < maxRetryCount)
        {
            retryCount++;
            Debug.LogWarning("Retrying to update player statistics in 1 second...");
            Invoke(nameof(RetryUpdateOxygenStatistic), 1.0f);
        }
    }
}
