using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreateProfileUI : MonoBehaviour
{
    public ProfileSelectionUI profileSelectionUI;

    [Header("Input Fields")]
    public TMP_InputField nameInput;
    public TMP_InputField ageInput;
    public TMP_InputField passwordInput;
    public TMP_InputField passwordConfirmInput;

    [Header("Gender Buttons (NOT toggles)")]
    public Button girlButton;
    public Button boyButton;

    private string selectedGender = "";

    [Header("Avatar")]
    public int selectedAvatarIndex = 0;
    public Button avatarButton;
    public AvatarSelectionUI avatarPopup;


    [Header("Buttons")]
    public Button createProfileButton;
    public Button backButton;

    [Header("Disable These When Creating")]
    public Button[] disableWhenCreating;

    [Header("Error Display")]
    public TMP_Text errorText;

    [Header("Button Highlight Colors")]
    public Color normalColor = new Color(1, 1, 1, 0.3f);
    public Color selectedColor = new Color(0, 1, 1, 1);

    


    private void Awake()
    {
        createProfileButton.onClick.AddListener(OnCreateProfileClicked);
        backButton.onClick.AddListener(OnBackClicked);
        girlButton.onClick.AddListener(() => SelectGender("Girl"));
        boyButton.onClick.AddListener(() => SelectGender("Boy"));
        avatarButton.onClick.AddListener(OpenAvatarPopup);
        ResetGenderButtons();
    }

    private void OpenAvatarPopup()
    {
        avatarPopup.gameObject.SetActive(true);
        avatarPopup.Setup(this);
    }

    private void SelectGender(string gender)
    {
        selectedGender = gender;

        girlButton.GetComponent<Image>().color =
            (gender == "Girl") ? selectedColor : normalColor;

        boyButton.GetComponent<Image>().color =
            (gender == "Boy") ? selectedColor : normalColor;
    }

    private void ResetGenderButtons()
    {
        selectedGender = "";
        girlButton.GetComponent<Image>().color = normalColor;
        boyButton.GetComponent<Image>().color = normalColor;
    }

    // Clear forms function
    public void ClearForm()
    {
        nameInput.text = "";
        ageInput.text = "";
        passwordInput.text = "";
        passwordConfirmInput.text = "";
        selectedGender = "";
        girlButton.GetComponent<Image>().color = normalColor;
        boyButton.GetComponent<Image>().color = normalColor;
        selectedAvatarIndex = 0;
        if (errorText != null)
            errorText.text = "";
    }

    // Profile create 
    private void OnCreateProfileClicked()
    {
        errorText.text = "";
        string name = nameInput.text.Trim();
        if (name.Length == 0)
        {
            errorText.text = "Please enter your name.";
            return;
        }

        if (!int.TryParse(ageInput.text.Trim(), out int age))
        {
            errorText.text = "Age must be a number.";
            return;
        }

        if (selectedGender == "")
        {
            errorText.text = "Please select gender.";
            return;
        }

        if (passwordInput.text.Length < 4)
        {
            errorText.text = "Password must be at least 4 characters.";
            return;
        }

        if (passwordInput.text != passwordConfirmInput.text)
        {
            errorText.text = "Passwords don't match.";
            return;
        }

        ProfileManager.Instance.CreateProfile(
            name,
            age,
            selectedGender,
            selectedAvatarIndex,  
            passwordInput.text
        );

        // Return to profile list
        profileSelectionUI.ShowSelectPanel();
        gameObject.SetActive(false);
    }

    private void OnBackClicked()
    {
        profileSelectionUI.ShowSelectPanel();
        gameObject.SetActive(false);
    }

    private void DisableExternalButtons()
    {
        foreach (Button b in disableWhenCreating)
            b.interactable = false;
    }

    private void EnableExternalButtons()
    {
        foreach (Button b in disableWhenCreating)
            b.interactable = true;
    }
}
