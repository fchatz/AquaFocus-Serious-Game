using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    public int TotalCoins = 0;

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

    private void Start()
    {
        // Initialize coin count from stored save file metadata
        if (GameDataManager.Instance != null)
            TotalCoins = GameDataManager.Instance.Data.totalCoins;

        if (CoinUI.Instance != null)
            CoinUI.Instance.UpdateCoinText(TotalCoins);
    }

    // Process currency addition and trigger relevant UI updates
    public void AddCoins(int amount)
    {
        TotalCoins += amount;
        Debug.Log($"[Coins] Added {amount}, Total = {TotalCoins}");
        GameDataManager.Instance.Data.totalCoins = TotalCoins;
        GameDataManager.Instance.SaveGame();

        if (CoinUI.Instance != null)
        {
            CoinUI.Instance.UpdateCoinText(TotalCoins);
            CoinUI.Instance.PlayCoinPopup(amount);
        }
    }

    // Process currency deduction and validate balance requirements
    public bool SpendCoins(int amount)
    {
        if (TotalCoins < amount) return false;

        TotalCoins -= amount;
        GameDataManager.Instance.Data.totalCoins = TotalCoins;
        GameDataManager.Instance.SaveGame();

        if (CoinUI.Instance != null)
            CoinUI.Instance.UpdateCoinText(TotalCoins);

        return true;
    }

    public void UpdateUI()
    {
        if (CoinUI.Instance != null)
            CoinUI.Instance.UpdateCoinText(TotalCoins);
    }
}