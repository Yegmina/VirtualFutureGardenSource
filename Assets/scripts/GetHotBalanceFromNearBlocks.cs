using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using PlayFab;
using PlayFab.ClientModels;

public class GetHotBalanceFromNearBlocks : MonoBehaviour
{
    public Text userNameText;
    public Text balanceText;

    private string rpcUrl = "https://rpc.mainnet.near.org"; // NEAR mainnet RPC endpoint
    private string contractAddress = "game.hot.tg"; // Contract address of the HOT token
    private int decimals = 6; // Decimals for the HOT token
//I am not using hot sdk hear. But I use near blockchain and check how many hot tokens have user address
//address is the username sent after login in the hot wallet. It is also tested with the sha256 cipher. This gives us confidence that the user is not a fraudster
    void Start()
    {
        StartCoroutine(DelayedFetchBalance());
    }

    IEnumerator DelayedFetchBalance()
    {
        
        yield return new WaitForSeconds(3.0f);

        string walletAddress = userNameText.text;
        if (!string.IsNullOrEmpty(walletAddress))
        {
            StartCoroutine(FetchBalance(walletAddress));
        }
        else
        {
            Debug.LogWarning("Wallet address is empty or null");
        }
    }

    IEnumerator FetchBalance(string walletAddress)
    {
        string method = "ft_balance_of"; 
        string argsJson = "{\"account_id\":\"" + walletAddress + "\"}";
        string argsBase64 = ToBase64(argsJson);
        
        string requestData = "{\"jsonrpc\":\"2.0\",\"id\":\"dontcare\",\"method\":\"query\",\"params\":{\"request_type\":\"call_function\",\"finality\":\"final\",\"account_id\":\"" + contractAddress + "\",\"method_name\":\"" + method + "\",\"args_base64\":\"" + argsBase64 + "\"}}";

        UnityWebRequest request = new UnityWebRequest(rpcUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Log the request data for debugging
        Debug.Log($"Request Data: {requestData}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error fetching balance: {request.error}");
            balanceText.text = "Error fetching balance";
        }
        else
        {
            string responseText = request.downloadHandler.text;
            Debug.Log($"Response: {responseText}");
            string balance = ExtractBalanceFromJson(responseText);
            balanceText.text = $"{balance} HOT";
            SaveBalanceToPlayFab(balance);
        }
    }

    string ToBase64(string plainText)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes);
    }

    string ExtractBalanceFromJson(string json)
    {
        try
        {
            var jsonObject = JObject.Parse(json);
            var balanceBytes = jsonObject["result"]["result"].ToObject<byte[]>();
            string balanceString = Encoding.UTF8.GetString(balanceBytes).Trim('"');
            double balance = double.Parse(balanceString) / Mathf.Pow(10, decimals);
            return balance.ToString();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error parsing JSON response: {ex.Message}");
            return "0";
        }
    }

    void SaveBalanceToPlayFab(string balance)
    {
        try
        {
            double balanceValue = double.Parse(balance);
            var request = new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate>
                {
                    new StatisticUpdate
                    {
                        StatisticName = "HOTBalance",
                        Value = (int)balanceValue // Convert double to int
                    }
                }
            };

            PlayFabClientAPI.UpdatePlayerStatistics(request, 
                result => Debug.Log("Successfully updated player statistics"), 
                error => Debug.LogError($"Error updating player statistics: {error.GenerateErrorReport()}"));
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error saving balance to PlayFab: {ex.Message}");
        }
    }
}
