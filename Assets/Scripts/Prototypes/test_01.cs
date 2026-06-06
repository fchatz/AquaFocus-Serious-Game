using UnityEngine;

public class Map2SessionStarter : MonoBehaviour
{
    [Header("Session Duration (seconds)")]
    public float sessionDuration = 60f;

    private float timer;
    private bool sessionActive = false;

    void Start()
    {
        if (TelemetryManager.Instance != null)
        {
            TelemetryManager.Instance.StartNewSession("Map2");
            Debug.Log("<color=#00ffea>[MAP2 SESSION]</color> Started NEW telemetry session.");
        }
        else
        {
            Debug.LogError("❌ TelemetryManager.Instance is NULL! Make sure it's in the scene.");
        }

        timer = sessionDuration;
        sessionActive = true;
    }

    void Update()
    {
        if (!sessionActive) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            EndSession();
        }
    }

    private void EndSession()
    {
        sessionActive = false;

        if (TelemetryManager.Instance != null)
        {
            TelemetryManager.Instance.ExportTelemetry();
            Debug.Log("<color=#ffea00>[MAP2 SESSION]</color> Session ended — JSON exported!");
        }
    }
}
