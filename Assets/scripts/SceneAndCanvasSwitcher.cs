using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneAndCanvasSwitcher : MonoBehaviour
{
    // Method to switch scenes by name
    public void SwitchScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

   public void ToggleCanvas(UnityEngine.Object obj)
{
    Canvas canvas = obj as Canvas;
    if (canvas != null)
    {
        canvas.enabled = !canvas.enabled;
    }
    else
    {
        Debug.LogError("The provided object is not a Canvas!");
    }
}

}
