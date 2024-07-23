using System.Collections;
using System.Text;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq; // может потом поменяю, но пока работает. А главное правило: работает - не трогай

public class GetHotBalanceFromNearBlocks : MonoBehaviour
{
    public Text userNameText;
    public Text balanceText;

    private string rpcUrl = "https://rpc.mainnet.near.org"; // NEAR mainnet RPC endpoint
    private string contractAddress = "game.hot.tg"; // Contract address of the HOT token
    private int decimals = 6; // Decimals for the HOT token

    void Start()
    {
        StartCoroutine(DelayedFetchBalance());
    }

    IEnumerator DelayedFetchBalance()
    {
        // ожидание 3 сек на всяк случай
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
        string method = "ft_balance_of"; //кол контракт
        string argsJson = "{\"account_id\":\"" + walletAddress + "\"}";
        string argsBase64 = ToBase64(argsJson);
        
        string requestData = "{\"jsonrpc\":\"2.0\",\"id\":\"dontcare\",\"method\":\"query\",\"params\":{\"request_type\":\"call_function\",\"finality\":\"final\",\"account_id\":\"" + contractAddress + "\",\"method_name\":\"" + method + "\",\"args_base64\":\"" + argsBase64 + "\"}}";

        UnityWebRequest request = new UnityWebRequest(rpcUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // для дебага
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
}
