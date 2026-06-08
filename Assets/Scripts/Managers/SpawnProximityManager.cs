using UnityEngine;

public class SpawnProximityManager : MonoBehaviour
{
    public static SpawnProximityManager Instance { get; private set; }

    [Header("Global Proximity Settings")]
    public float minDistanceBlueToMines = 4f;
    public LayerMask mineLayer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}