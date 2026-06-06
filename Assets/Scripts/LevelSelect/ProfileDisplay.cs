using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileDisplay : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text nameText;        // Profile name text
    public Image avatarImage;        // Profile avatar image

    [Header("Avatars")]
    public Sprite[] avatarSprites;   // Same order as when you created profiles

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

        // Set name
        if (nameText != null)
        {
            nameText.text = profile.name;
        }

        // Set avatar
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
