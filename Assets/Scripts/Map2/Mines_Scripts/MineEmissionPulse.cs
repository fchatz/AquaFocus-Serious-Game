using UnityEngine;

public class MineEmissionPulse : MonoBehaviour
{
    [Header("Renderer of the Mine")]
    public Renderer mineRenderer;

    [Header("Pulse Settings")]
    public Color emissionColor = Color.red;
    public float minIntensity = 2f;   // soft bottom glow
    public float maxIntensity = 8f;   // strong pulse peak - works with bloom
    public float pulseSpeed = 2f;     // pulse rate

    private Material mineMaterial;

    void Start()
    {
        if (mineRenderer == null)
            mineRenderer = GetComponentInChildren<Renderer>();

        // Get a unique material instance so all mines don't share pulsing state
        mineMaterial = mineRenderer.material;
        mineMaterial.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        // Pulse between 0 and 1
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        // Interpolate intensity
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);

        // Apply HDR emission
        mineMaterial.SetColor("_EmissionColor", emissionColor * intensity);
    }
}
