using UnityEngine;
using UnityEngine.UI;

public class ScreenFeedback : MonoBehaviour
{
    public Image redPulseImage;
    public float pulseDuration = 1.5f;

    private float pulseTimer = 0f;
    private bool pulsing = false;
    private Color startColor;

    void Start()
    {
        if (redPulseImage != null)
        {
            startColor = redPulseImage.color;
            redPulseImage.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (pulsing && redPulseImage != null)
        {
            pulseTimer += Time.deltaTime;
            float t = pulseTimer / pulseDuration;

            Color faded = startColor;
            faded.a = Mathf.Lerp(startColor.a, 0f, t);
            redPulseImage.color = faded;

            if (pulseTimer >= pulseDuration)
            {
                redPulseImage.gameObject.SetActive(false);
                pulsing = false;
            }
        }
    }

    public void TriggerRedPulse()
    {
        if (redPulseImage != null)
        {
            redPulseImage.color = startColor;
            redPulseImage.gameObject.SetActive(true);
            pulseTimer = 0f;
            pulsing = true;
        }
    }
}
