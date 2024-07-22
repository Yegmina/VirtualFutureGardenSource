using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using PlayFab;
using PlayFab.ClientModels;

public class login : MonoBehaviour {
    [DllImport("__Internal")]
    private static extern System.IntPtr GetUserName();

    public Text userNameText;
    public bool isEditorTest;
    public string editorUserName;

    void Start() {
       // ShowAlert("Start function called");
        string userName = string.Empty;

        #if UNITY_EDITOR
        if (isEditorTest) {
           // ShowAlert("Editor test mode enabled");
            userName = editorUserName;
           // ShowAlert("Editor userName: " + userName);
        } else {
            userName = "DefaultEditorUser";
           // ShowAlert("Default editor userName: " + userName);
        }
        #else
        try {
            //ShowAlert("Attempting to retrieve userName from JavaScript");
            userName = Marshal.PtrToStringAnsi(GetUserName());
           // ShowAlert("Retrieved userName from JavaScript: " + userName);
        } catch (System.Exception ex) {
          //  ShowAlert("Exception when calling GetUserName: " + ex.Message);
        }
        #endif

       // ShowAlert("Final userName to use: " + userName);
        userNameText.text = userName;

        if (!string.IsNullOrEmpty(userName) && userName != "User not available") {
            LoginWithPlayFab(userName);
        } else {
           // ShowAlert("userName is either null or not available");
        }
    }

    private void LoginWithPlayFab(string userName) {
       // ShowAlert("Attempting to login with PlayFab for user: " + userName);
        PlayFabSettings.staticSettings.TitleId = "A8791"; 
       // ShowAlert("PlayFab Title ID: " + PlayFabSettings.staticSettings.TitleId);

        var request = new LoginWithCustomIDRequest {
            CustomId = userName,
            CreateAccount = true
        };

       // ShowAlert("Login request created with CustomId: " + request.CustomId);
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result) {
    //    ShowAlert("PlayFab login successful for user: " + result.PlayFabId);
    //    ShowAlert("Session ticket: " + result.SessionTicket);
    }

    private void OnLoginFailure(PlayFabError error) {
    //    ShowAlert("PlayFab login failed: " + error.GenerateErrorReport());
        if (error.Error == PlayFabErrorCode.InvalidTitleId) {
     //       ShowAlert("Invalid Title ID provided");
        }
        if (error.Error == PlayFabErrorCode.AccountNotFound) {
        //    ShowAlert("Account not found for the provided Custom ID");
        }
        if (error.Error == PlayFabErrorCode.InvalidParams) {
         //   ShowAlert("Invalid parameters provided");
        }
        if (error.Error == PlayFabErrorCode.ServiceUnavailable) {
           // ShowAlert("PlayFab service is unavailable");
        }
    }

    private void ShowAlert(string message) {
        #if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalEval("alert('" + message + "');");
        #else
        Debug.Log(message);
        #endif
    }
}
