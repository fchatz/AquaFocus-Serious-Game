using UnityEngine;
using TMPro;

public class RequirementsPanel : MonoBehaviour
{
    public static RequirementsPanel Instance;

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI reqText;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void ShowMap2Requirements()
    {
        panel.SetActive(true);

        titleText.text = "Sea Minefield Requirements";
        reqText.text =
            "• Blue Jellyfish Accuracy  ≥ 70%\n" +
            "• Blue Jellyfish Streak ≥ 10\n" +
            "• Play Jellyfish Path at least 5 times";
    }

    public void ShowMap3Requirements()
    {
        panel.SetActive(true);

        titleText.text = "Marine Chaos Requirements";
        reqText.text =
            "• Mine Avoidance ≥ 65%\n" +
            "• Blue Jellyfish Accuracy ≥ 70%\n" +
            "• Play Sea Minefield at least 5 times";
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
