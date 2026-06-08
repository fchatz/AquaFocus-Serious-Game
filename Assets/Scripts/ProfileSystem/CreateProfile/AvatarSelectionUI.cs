using UnityEngine;
using UnityEngine.UI;

public class AvatarSelectionUI : MonoBehaviour
{
    [Header("Avatar Buttons")]
    public Button[] avatarButtons;

    [Header("Highlight Effect")]
    public Image[] avatarHighlights; 

    [Header("Buttons")]
    public Button okButton;
    public Button cancelButton;

    private CreateProfileUI createProfileUI;

    private int chosenAvatarIndex = -1;

    private void Awake()
    {
        for (int i = 0; i < avatarButtons.Length; i++)
        {
            int index = i;
            avatarButtons[i].onClick.AddListener(() => SelectAvatar(index));
        }
        okButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(OnCancel);
        HideHighlights();
    }

    public void Setup(CreateProfileUI createProfile)
    {
        createProfileUI = createProfile;
        chosenAvatarIndex = createProfileUI.selectedAvatarIndex;
        HighlightAvatar(chosenAvatarIndex);
    }

    private void SelectAvatar(int index)
    {
        chosenAvatarIndex = index;
        HighlightAvatar(index);
    }

    private void HighlightAvatar(int index)
    {
        HideHighlights();
        if (index >= 0 && index < avatarHighlights.Length)
            avatarHighlights[index].enabled = true;
    }

    private void HideHighlights()
    {
        foreach (var h in avatarHighlights)
            h.enabled = false;
    }

    private void OnConfirm()
    {
        if (chosenAvatarIndex >= 0)
            createProfileUI.selectedAvatarIndex = chosenAvatarIndex;

        gameObject.SetActive(false);
    }

    private void OnCancel()
    {
        gameObject.SetActive(false);
    }
}
