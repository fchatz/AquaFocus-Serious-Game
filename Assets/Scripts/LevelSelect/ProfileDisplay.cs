using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileDisplay : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text nameText;        // Profile name text component
    public Image avatarImage;        // Profile avatar image component

    [Header("Avatars")]
    public Sprite[] avatarSprites;

    private void Start()
    {
        if (ProfileManager.Instance == null)
        {
            Debug.LogWarning("ProfileNameAndAvatarDisplay: No ProfileManager in scene.");
            SetFallbackUI("No Profile", null);
            return;
        }

        var profile = ProfileManager.Instance.currentProfile;
        if (profile == null)
        {
            Debug.LogWarning("ProfileNameAndAvatarDisplay: currentProfile is null.");
            SetFallbackUI("No Profile", null);
            return;
        }

        // Set profile name display
        if (nameText != null)
        {
            nameText.text = profile.name;
        }

        // Set profile avatar sprite configuration
        if (avatarImage != null && avatarSprites != null && avatarSprites.Length > 0)
        {
            int index = profile.avatarIndex;

            if (index < 0 || index >= avatarSprites.Length)
            {
                Debug.LogWarning($"ProfileNameAndAvatarDisplay: avatarIndex {index} out of range, using 0.");
                index = 0;
            }

            avatarImage.sprite = avatarSprites[index];
        }
    }

    private void SetFallbackUI(string fallbackName, Sprite fallbackSprite)
    {
        if (nameText != null)
            nameText.text = fallbackName;

        if (avatarImage != null)
            avatarImage.sprite = fallbackSprite;
    }
}