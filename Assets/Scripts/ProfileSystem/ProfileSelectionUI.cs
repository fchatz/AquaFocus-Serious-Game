using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileSelectionUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject selectProfilePanel;
    public GameObject createProfilePanel;

    [Header("Profile List UI")]
    public Transform contentParent;
    public ProfileItemUI profileItemPrefab;
    public Sprite[] avatarSprites;

    [Header("Buttons")]
    public Button createNewProfileBtn;
    public Button cancelCreateBtn;

    public DeleteConfirmationUI deletePopup;

    public Button createNewProfileBTN;
    public Button exitBTN;

    public TMP_Text selectProfTitle;




    private void Start()
    {
        selectProfilePanel.SetActive(true);
        createProfilePanel.SetActive(false);

        createNewProfileBtn.onClick.AddListener(ShowCreatePanel);
        cancelCreateBtn.onClick.AddListener(ShowSelectPanel);

        if (exitBTN != null)
        {
            exitBTN.onClick.AddListener(ExitGame);
        }

        RefreshList();
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void ShowCreatePanel()
    {
        selectProfilePanel.SetActive(false);
        createProfilePanel.SetActive(true);
        createNewProfileBTN.gameObject.SetActive(false);
        exitBTN.gameObject.SetActive(false);
        selectProfTitle.gameObject.SetActive(false);
        createProfilePanel.GetComponent<CreateProfileUI>().ClearForm();
    }

    public void ShowSelectPanel()
    {
        createProfilePanel.SetActive(false);
        selectProfilePanel.SetActive(true);
        createNewProfileBTN.gameObject.SetActive(true);
        exitBTN.gameObject.SetActive(true);
        selectProfTitle.gameObject.SetActive(true);
        RefreshList();
    }

    public void RefreshList()
    {
        foreach (Transform child in contentParent)
        {
            if (child.CompareTag("ProfileItem"))
                Destroy(child.gameObject);
        }
        // Spawn all profile
        foreach (var header in ProfileManager.Instance.profileHeaders)
        {
            var item = Instantiate(profileItemPrefab, contentParent);
            item.gameObject.tag = "ProfileItem";
            int idx = header.avatarIndex;
            if (idx < 0 || idx >= avatarSprites.Length)
                idx = 0;

            item.Setup(header, avatarSprites[idx]);


            // Select profile
            item.selectButton.onClick.AddListener(() =>
            {
                string id = item.GetID();
                if (string.IsNullOrEmpty(id))
                {
                    Debug.LogError("[ProfileSelectionUI] ERROR: item.GetID() returned NULL or empty!");
                    return;
                }

                Debug.Log("[ProfileSelectionUI] Selecting profile: " + id);

                if (ProfileManager.Instance == null)
                {
                    Debug.LogError("[ProfileSelectionUI] ERROR: ProfileManager.Instance is NULL!");
                    return;
                }

                var profile = ProfileManager.Instance.LoadProfile(id);
                if (profile == null)
                {
                    Debug.LogError("[ProfileSelectionUI] ERROR: LoadProfile returned NULL. Save file may be missing or corrupted.");
                    return;
                }

                ProfileManager.Instance.currentProfile = profile;

                if (GameDataManager.Instance == null)
                {
                    Debug.LogError("[ProfileSelectionUI] ERROR: GameDataManager.Instance is NULL!");
                    return;
                }

                GameDataManager.Instance.SetCurrentProfile(id);

                LoadingManager.Instance.LoadScene("MainMenu");
            });



            // Delete profile with confirmation popup
            item.deleteButton.onClick.AddListener(() =>
            {
                deletePopup.Show(() =>
                {
                    ProfileManager.Instance.DeleteProfile(item.GetID());
                    RefreshList();
                });
            });
        }

    }

}
