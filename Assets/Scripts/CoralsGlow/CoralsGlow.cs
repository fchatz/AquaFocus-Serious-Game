using UnityEngine;

public class CoralsGlow : MonoBehaviour
{
    [Header("Emission Settings")]
    public Color emissionColor = Color.cyan;
    public float minIntensity = 1f;
    public float maxIntensity = 4f;
    public float pulseSpeed = 5f;

    private Material mat;
    private float pulseTimer = 0f;

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            mat = rend.material;
            mat.EnableKeyword("_EMISSION");
        }
    }

    void Update()
    {
        if (mat == null) return;

        pulseTimer += Time.deltaTime * pulseSpeed;
        float intensity = Mathf.Lerp(minIntensity, maxIntensity,
                                     (Mathf.Sin(pulseTimer) + 1) * 0.5f);

        mat.SetColor("_EmissionColor", emissionColor * intensity);
    }
}
