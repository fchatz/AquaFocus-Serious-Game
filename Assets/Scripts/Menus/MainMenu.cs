using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{

    [SerializeField] GameObject bigButton;
    [SerializeField] GameObject animCam;
    [SerializeField] GameObject mainCam;
    [SerializeField] GameObject menuControls;
    [SerializeField] GameObject startText;
    public static bool hasClicked;
    [SerializeField] GameObject staticCam;

    [SerializeField] GameObject test;

    [Header("Profile Display UI")]
    public Image profileAvatarImage;
    [SerializeField] GameObject shopButton;
    public TMP_Text profileNameText;
    public TMP_Text profileCoinsText;

    [Header("Avatar Sprites")]
    public Sprite[] avatarSprites;

    [Header("Informations Popup")]
    public GameObject staticMenu;
    public GameObject informationCanvas;


    private void Start()
    {
        if (hasClicked == true)
        {
            staticCam.SetActive(true);
            animCam.SetActive(false);
            menuControls.SetActive(true);
            shopButton.SetActive(true);
            bigButton.SetActive(false);
            startText.SetActive(false);
            test.SetActive(false);
        }

        // Load profile info into UI
        DisplayProfileInfo();
    }

    private void DisplayProfileInfo()
    {
        var profile = ProfileManager.Instance.currentProfile;
        if (profile == null)
        {
            Debug.LogError("No profile selected! Cannot display UI info.");
            return;
        }

        // Display Name
        profileNameText.text = profile.name;

        // Display Avatar
        if (profile.avatarIndex >= 0 && profile.avatarIndex < avatarSprites.Length)
            profileAvatarImage.sprite = avatarSprites[profile.avatarIndex];
        else
            Debug.LogWarning("Invalid avatar index!");

        // Display Coins (from SaveData)
        profileCoinsText.text = GameDataManager.Instance.Data.totalCoins.ToString();
    }
    private void Update()
    {
        
    }
    
    public void MenuBeginButton()
    {
        StartCoroutine(AnimCam());
    }

    public void StartGame()
    {
        if (ProfileManager.Instance.currentProfile != null)
        {
            GameDataManager.Instance.SetCurrentProfile(
                ProfileManager.Instance.currentProfile.id
            );
        }

        LoadingManager.Instance.LoadScene("LevelSelect");
    }

    public void ProfileButton()
    {
        LoadingManager.Instance.LoadScene("ProfileSelectionScene");
    }

    public void OptionsButton()
    {
        LoadingManager.Instance.LoadScene("Options");
    }

    public void ToStoreButton()
    {
        LoadingManager.Instance.LoadScene("Store");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public void OnInformationButton()
    {
        staticMenu.SetActive(false);
        informationCanvas.SetActive(true);
    }

    public void OnBackToMenu()
    {
        informationCanvas.SetActive(false);
        staticMenu.SetActive(true);
    }



    IEnumerator AnimCam()
    {
        animCam.GetComponent<Animator>().Play("AnimCamMenu");
        startText.SetActive(false);
        bigButton.SetActive(false);
        yield return new WaitForSeconds(1.5f);
        staticCam.SetActive(true);
        animCam.SetActive(false);
        menuControls.SetActive(true);
        shopButton.SetActive(true);
        hasClicked = true;
    }
}
