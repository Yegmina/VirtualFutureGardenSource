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
    public Text displayCurrentLightValue;
    public Text displayNextLevelCost;
    public Text displayNextLevelValue;

    private float currentOxygen = 0f;
    private float lightValue = 0f;
    private float artificialLightValue = 50f;  // Значение по умолчанию
    public float boost = 1f;  // Значение по умолчанию

    private bool isPlayerStatisticsLoaded = false;
    private int retryCount = 0;
    private const int maxRetryCount = 5;

    private DateTime lastLogoutTime;
    private double maxOfflineHours = 1; // Значение по умолчанию

    private List<int> lightValues = new List<int> { 50, 75, 100, 125 }; // Значения lux для каждого уровня
    private List<int> oxygenCosts = new List<int> { 0, 1000, 2000, 5000 }; // Стоимость кислорода для улучшения

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

        // Обновляем отображение уровней и стоимости
        UpdateLightValueDisplay();
    }

    public void UpdateLightValue(float newLightValue)
    {
        lightValue = newLightValue;
    }

    void CalculateOxygen()
    {
        currentOxygen += boost * (1f / 600f) * (lightValue + artificialLightValue);

        if (displayOxygen != null)
        {
            displayOxygen.text = "Oxygen: " + currentOxygen.ToString("F2") + " [units]";
        }

       // Debug.Log("Oxygen value updated: " + currentOxygen);

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

        // Обновляем отображение уровней и стоимости
        UpdateLightValueDisplay();
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

        if (currentOxygen < 0) { currentOxygen = 1; }
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

    // Функция для улучшения значения artificialLightValue
    public void UpgradeArtificialLightValue()
    {
        if (!isPlayerStatisticsLoaded)
        {
            Debug.LogWarning("Player statistics not loaded yet. Cannot upgrade artificial light value.");
            return;
        }

        int currentLevel = lightValues.IndexOf((int)artificialLightValue);

        if (currentLevel == -1 || currentLevel >= lightValues.Count - 1)
        {
            Debug.LogWarning("Cannot upgrade artificial light value. Either at max level or invalid current value.");
            return;
        }

        int nextLevel = currentLevel + 1;
        int costForNextLevel = oxygenCosts[nextLevel];

        if (currentOxygen < costForNextLevel)
        {
            Debug.LogWarning("Not enough oxygen to upgrade artificial light value.");
            return;
        }

        currentOxygen -= costForNextLevel;
        artificialLightValue = lightValues[nextLevel];

        if (displayOxygen != null)
        {
            displayOxygen.text = "Oxygen: " + currentOxygen.ToString("F2") + " [units]";
        }

        Debug.Log("Artificial Light value upgraded to: " + artificialLightValue);

        // Обновление значения кислорода и искусственного света в PlayFab
        UpdateOxygenStatistic(currentOxygen, artificialLightValue);

        // Обновляем отображение уровней и стоимости
        UpdateLightValueDisplay();
    }

    // Обновление отображения уровней и стоимости
    void UpdateLightValueDisplay()
    {
        int currentLevel = lightValues.IndexOf((int)artificialLightValue);
        int nextLevel = currentLevel + 1;

        if (displayCurrentLightValue != null)
        {
            displayCurrentLightValue.text = "Current Light Value: " + artificialLightValue + " lux";
        }

        if (nextLevel < lightValues.Count)
        {
            if (displayNextLevelCost != null)
            {
                displayNextLevelCost.text = "Next Level Cost: " + oxygenCosts[nextLevel] + " oxygen";
            }
            if (displayNextLevelValue != null)
            {
                displayNextLevelValue.text = "Next Level Value: " + lightValues[nextLevel] + " lux";
            }
        }
        else
        {
            if (displayNextLevelCost != null)
            {
                displayNextLevelCost.text = "Next Level Cost: MAX";
            }
            if (displayNextLevelValue != null)
            {
                displayNextLevelValue.text = "Next Level Value: MAX";
            }
        }
    }
}
