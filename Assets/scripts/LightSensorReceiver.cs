using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class LightSensorReceiver : MonoBehaviour 
{
    [DllImport("__Internal")]
    private static extern void StartLightSensor();

    public Text displayLight;
    public Light sceneLight;  // Ссылка на компонент Light в сцене

    void Start() 
    {
        Debug.Log("Starting Light Sensor...");
        StartLightSensor();
    }

    public void OnLightSensorChanged(string lightValue) 
    {
        Debug.Log("Light sensor value received: " + lightValue);
        if (displayLight != null) 
        {
            displayLight.text = "Light : " + lightValue + " [lux]";
        }
        
        // Notify the OxygenCalculator about the new light value
        OxygenCalculator.Instance.UpdateLightValue(float.Parse(lightValue));
        
        // Update the light intensity based on the lux value
        if (sceneLight != null)
        {
            // Можно выбрать масштабирование значения lux для интенсивности света
            sceneLight.intensity = float.Parse(lightValue) / 200f;  // Пример: делим на 200 для подходящего диапазона
        }
    }
}
