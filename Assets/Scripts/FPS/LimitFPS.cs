using UnityEngine;

public class LimitFPS : MonoBehaviour
{
    public int targetFPS = 120;

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFPS;
    }
}