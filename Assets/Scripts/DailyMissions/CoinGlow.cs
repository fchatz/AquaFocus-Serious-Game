using UnityEngine;
using UnityEngine.UI;

public class CoinGlow : MonoBehaviour
{
    public Image icon;
    public float glowSpeed = 2f;
    public float minAlpha = 0.6f;
    public float maxAlpha = 1f;

    private bool glowing = false;

    void Update()
    {
        if (!glowing || icon == null)
            return;

        float a = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * glowSpeed) + 1f) * 0.5f);

        Color c = icon.color;
        c.a = a;
        icon.color = c;
    }

    public void EnableGlow(bool state)
    {
        glowing = state;
        if (!state && icon != null)
        {
            // Reset normal color when disabled
            Color c = icon.color;
            c.a = 1f;
            icon.color = c;
        }
    }
}
