using UnityEngine;

public class MapUnlockManager : MonoBehaviour
{
    public static MapUnlockManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void EvaluateUnlocks()
    {
        var data = GameDataManager.Instance.Data;
        var mapName = TelemetryManager.Instance.CurrentMapName;

        // -------- MAP 2 UNLOCK LOGIC --------
        if (mapName.Contains("Map1") && !data.map2Unlocked)
        {
            var s = TelemetryManager.Instance.SummaryMap1;

            bool accuracyOK = s.accuracy >= 0.70f;
            bool streakOK = s.max_streak >= 10;
            bool sessionsOK = data.map1SessionsPlayed >= 5;

            if (accuracyOK && streakOK && sessionsOK)
            {
                data.map2Unlocked = true;
                Debug.Log("🎉 MAP 2 UNLOCKED!");
            }
        }

        // -------- MAP 3 UNLOCK LOGIC --------
        if (mapName.Contains("Map2") && !data.map3Unlocked)
        {
            var s = TelemetryManager.Instance.SummaryMap2;

            bool minesOK = s.mine_accuracy >= 0.65f;
            bool goOK = s.squid_go_accuracy >= 0.70f;
            bool sessionsOK = data.map2SessionsPlayed >= 5;

            if (minesOK && goOK && sessionsOK)
            {
                data.map3Unlocked = true;
                Debug.Log("🎉 MAP 3 UNLOCKED!");
            }
        }

        GameDataManager.Instance.SaveGame();
    }
}
