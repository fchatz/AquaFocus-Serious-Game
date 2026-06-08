using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ParentGateUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField passwordInput;
    public TMP_Text errorText;
    public Button confirmButton;
    public Button cancelButton;

    [Header("Hide BTNS on Dashboard Popup")]
    public GameObject staticMenu;

    [Header("Config")]
    public string dashboardSceneName = "ParentDashboard";

    private void Awake()
    {
        gameObject.SetActive(false);

        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(OnCancel);
    }

    // Called from ParentButton.OnClick in MainMenu
    public void Open()
    {
        errorText.text = "";
        passwordInput.text = "";
        gameObject.SetActive(true);
        staticMenu.SetActive(false);
    }

    private void OnCancel()
    {
        gameObject.SetActive(false);
        staticMenu.SetActive(true);
    }

    private void OnConfirm()
    {
        if (ProfileManager.Instance == null ||
            ProfileManager.Instance.currentProfile == null)
        {
            errorText.text = "Choose a profile.";
            return;
        }

        string input = passwordInput.text.Trim();
        if (string.IsNullOrEmpty(input))
        {
            errorText.text = "Password.";
            return;
        }

        string hash = ProfileManager.Instance.currentProfile.parentPasswordHash;

        if (PasswordUtils.CheckPassword(input, hash))
        {
            // Correct, load dashboard
            SceneManager.LoadScene(dashboardSceneName);
        }
        else
        {
            errorText.text = "Wrong password.";
        }
    }
}
