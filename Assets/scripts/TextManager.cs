using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextManager : MonoBehaviour
{
    public Text targetText; // The Text component to update
    public GameObject backgroundObject; // The optional background object to disable

    private Coroutine blinkCoroutine;
    private float blinkInterval = 1f; // Blinking interval in seconds
    private bool isBlinking = false;

    void Start()
    {
        if (targetText == null)
        {
            Debug.LogError("Target Text is not assigned.");
            enabled = false;
            return;
        }

        targetText.gameObject.SetActive(false); // Ensure the text is initially hidden
        if (backgroundObject != null)
        {
            backgroundObject.SetActive(false); // Ensure the background is initially hidden
        }
    }

    public void UpdateText(string newText)
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }

        targetText.text = newText;
        targetText.gameObject.SetActive(true);
        if (backgroundObject != null)
        {
            backgroundObject.SetActive(true);
        }
        blinkCoroutine = StartCoroutine(BlinkAndDisappear());
    }

    private IEnumerator BlinkAndDisappear()
    {
        isBlinking = true;

        // Blink the text
        float startTime = Time.time;
        while (Time.time < startTime + 8f)
        {
           // targetText.enabled = !targetText.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }

        DisableTextAndBackground();
        isBlinking = false;
    }

    void Update()
    {
        if (isBlinking && Input.GetMouseButtonDown(0))
        {
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
            }

            DisableTextAndBackground();
            isBlinking = false;
        }
    }

    private void DisableTextAndBackground()
    {
        targetText.gameObject.SetActive(false);
        if (backgroundObject != null)
        {
            backgroundObject.SetActive(false);
        }
    }
}
