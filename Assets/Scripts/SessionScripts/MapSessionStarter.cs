using UnityEngine;

public class MapSessionStarter : MonoBehaviour
{
    [SerializeField] private string mapName = "Map1Main";

    private void Start()
    {
        CoinManager.Instance.TotalCoins = GameDataManager.Instance.Data.totalCoins;
        CoinManager.Instance.UpdateUI();

        Debug.Log("=== MAP 1 DEBUG ===");
        Debug.Log("GameDataManager exists: " + GameDataManager.Instance);
        Debug.Log("Profile ID: " + GameDataManager.Instance.currentProfileId);
        Debug.Log("Coins BEFORE LoadGame: " + GameDataManager.Instance.Data.totalCoins);

        if (!string.IsNullOrEmpty(GameDataManager.Instance.currentProfileId))
            GameDataManager.Instance.LoadGame();

        Debug.Log("Coins AFTER LoadGame: " + GameDataManager.Instance.Data.totalCoins);


        // Load save for this profile before we update UI
        if (GameDataManager.Instance != null &&
            !string.IsNullOrEmpty(GameDataManager.Instance.currentProfileId))
        {
            GameDataManager.Instance.LoadGame();
        }
        Debug.Log("MapSessionStarter STARTED for: " + mapName);

        // --- Start telemetry for this map ---
        if (TelemetryManager.Instance != null)
        {
            TelemetryManager.Instance.StartNewSession(mapName);
            Debug.Log("📊 Started Telemetry Session for: " + mapName);
        }
        else
        {
            Debug.LogError("❌ TelemetryManager missing in scene!");
        }

        // --- Refresh the coin UI when the map loads ---
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.UpdateUI();
        }
        else
        {
            Debug.LogWarning("⚠ CoinManager.Instance is null in MapSessionStarter");
        }
    }
}
