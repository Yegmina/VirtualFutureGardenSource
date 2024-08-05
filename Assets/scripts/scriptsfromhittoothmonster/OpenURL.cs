using UnityEngine;

public class OpenURL : MonoBehaviour
{
    public void OpenLink(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Application.ExternalCall("openURL", url);
#else
            Application.OpenURL(url);
#endif
        }
        else
        {
            Debug.LogWarning("URL is empty or null.");
        }
    }
}
