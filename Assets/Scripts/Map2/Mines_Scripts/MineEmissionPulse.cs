using UnityEngine;

public class MineEmissionPulse : MonoBehaviour
{
    [Header("Renderer of the Mine")]
    public Renderer mineRenderer;

    [Header("Pulse Settings")]
    public Color emissionColor = Color.red;
    public float minIntensity = 2f;   
    public float maxIntensity = 8f;   
    public float pulseSpeed = 2f;    

    private Material mineMaterial;

    void Start()
    {
        if (mineRenderer == null)
            mineRenderer = GetComponentInChildren<Renderer>();

        mineMaterial = mineRenderer.material;
        mineMaterial.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);
        mineMaterial.SetColor("_EmissionColor", emissionColor * intensity);
    }
}
