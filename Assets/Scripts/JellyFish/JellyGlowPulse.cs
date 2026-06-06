using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class JellyGlowPulse : MonoBehaviour
{
    public Color glowColor = new Color(0, 1f, 1f);
    public float baseIntensity = 3f;
    public float pulseAmplitude = 2f;
    public float pulseSpeed = 1f;

    private Material mat;
    private float emissionBase;

    void Start()
    {
        mat = GetComponent<Renderer>().material; // instance for this jelly
        mat.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f; // 0-1
        float intensity = baseIntensity + pulse * pulseAmplitude;
        Color finalColor = glowColor * intensity;
        mat.SetColor("_EmissionColor", finalColor);
    }
}
