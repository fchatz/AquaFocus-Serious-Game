using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileItemUI : MonoBehaviour
{
    [Header("UI References")]
    public Image avatarImage;
    public TMP_Text nameText;
    public TMP_Text ageText;
    public Button selectButton;
    public Button deleteButton;
    public DeleteConfirmationUI deletePopup;


    private string profileID;

    // ---- Setup with ProfileHeader (Correct for your UI system) ----
    public void Setup(ProfileHeader header, Sprite avatar)
    {
        profileID = header.id;

        nameText.text = header.name;
        ageText.text = "Age: " + header.age;

        if (avatar != null)
            avatarImage.sprite = avatar;
    }

    // ---- Return ID when selecting/deleting ----
    public string GetID()
    {
        return profileID;
    }
}
