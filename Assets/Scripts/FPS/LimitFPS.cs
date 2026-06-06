using UnityEngine;

public class LimitFPS : MonoBehaviour
{
    public int targetFPS = 120;

    void Awake()
    {
        QualitySettings.vSyncCount = 0;   // Disable VSync so targetFrameRate works
        Application.targetFrameRate = targetFPS;
    }
}
