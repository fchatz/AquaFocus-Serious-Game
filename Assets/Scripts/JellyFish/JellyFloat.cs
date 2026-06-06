using UnityEngine;

public class JellyFloat : MonoBehaviour
{
    public float hoverAmp = 0.25f;
    public float hoverFreq = 1.2f;
    public float spinSpeed = 40f;

    private Vector3 startPos;
    private float phaseOffset; // for desyncing hover/spin

    void Start()
    {
        startPos = transform.position;
        phaseOffset = Random.Range(0f, Mathf.PI * 2f); // small variation
    }

    void Update()
    {
        // Hover
        float y = Mathf.Sin(Time.time * hoverFreq + phaseOffset) * hoverAmp;
        transform.position = startPos + Vector3.up * y;

        // Keep the jellyfish upright (-90° on X)
        Quaternion baseRotation = Quaternion.Euler(-90f, 0f, 0f);

        // Rotate gently around its own vertical axis (local Z in this model)
        Quaternion spin = Quaternion.Euler(0f, 0f, Time.time * spinSpeed);

        transform.rotation = baseRotation * spin;
    }

}
