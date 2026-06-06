using UnityEngine;

public class ButtonPulse : MonoBehaviour
{
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.05f;

    private bool pulsing = false;
    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (!pulsing)
            return;

        float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale * scale;
    }

    public void EnablePulse(bool state)
    {
        pulsing = state;

        if (!state)
            transform.localScale = originalScale;
    }
}
