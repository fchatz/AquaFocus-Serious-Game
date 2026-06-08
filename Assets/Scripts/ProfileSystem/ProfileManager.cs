using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance;

    private string profilesFolder;
    private string indexFilePath;

    public List<ProfileHeader> profileHeaders = new List<ProfileHeader>();
    public PlayerProfile currentProfile;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        profilesFolder = Path.Combine(Application.persistentDataPath, "profiles");
        indexFilePath = Path.Combine(profilesFolder, "profiles_index.json");

        EnsureFolder();
        LoadProfileIndex();
    }

    private void EnsureFolder()
    {
        if (!Directory.Exists(profilesFolder))
            Directory.CreateDirectory(profilesFolder);
    }

    public void LoadProfileIndex()
    {
        if (!File.Exists(indexFilePath))
        {
            profileHeaders = new List<ProfileHeader>();
            SaveProfileIndex();
            return;
        }

        string json = File.ReadAllText(indexFilePath);
        profileHeaders = JsonUtility.FromJson<ProfileHeaderList>(json).list;
    }

    public void SaveProfileIndex()
    {
        var wrapper = new ProfileHeaderList { list = profileHeaders };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(indexFilePath, json);
    }

    [Serializable]
    private class ProfileHeaderList
    {
        public List<ProfileHeader> list;
    }

    // Profiles

    public PlayerProfile LoadProfile(string guid)
    {
        // Folder: profiles/<guid>/
        string folder = Path.Combine(profilesFolder, guid);

        // File path: profiles/<guid>/<guid>.json
        string path = Path.Combine(folder, guid + ".json");

        if (!File.Exists(path))
        {
            Debug.LogError($"[ProfileManager] Profile file not found: {path}");
            return null;
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<PlayerProfile>(json);
    }

    public void SaveProfile(PlayerProfile profile)
    {
        // Create folder: profiles/<guid>/
        string folder = Path.Combine(profilesFolder, profile.id);
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        // File path: profiles/<guid>/<guid>.json
        string path = Path.Combine(folder, profile.id + ".json");

        string json = JsonUtility.ToJson(profile, true);
        File.WriteAllText(path, json);

        Debug.Log($"[ProfileManager] Saved profile at {path}");
    }

    // Create profile

    public void CreateProfile(string name, int age, string sex, int avatarIndex, string parentPassword)
    {
        string guid = Guid.NewGuid().ToString();

        PlayerProfile newProfile = new PlayerProfile
        {
            id = guid,
            name = name,
            age = age,
            sex = sex,
            avatarIndex = avatarIndex,
            parentPasswordHash = PasswordUtils.HashPassword(parentPassword)
        };

        SaveProfile(newProfile);

        ProfileHeader header = new ProfileHeader
        {
            id = guid,
            name = name,
            age = age,
            sex = sex,
            avatarIndex = avatarIndex,
            lastPlayed = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
        };

        profileHeaders.Add(header);
        SaveProfileIndex();
    }

    // Delete profile

    public void DeleteProfile(string guid)
    {
        // Delete profile json
        string jsonPath = Path.Combine(profilesFolder, guid + ".json");
        if (File.Exists(jsonPath))
        {
            File.Delete(jsonPath);
            Debug.Log($"[ProfileManager] Deleted profile JSON: {jsonPath}");
        }

        // Delete profile folder
        string profileFolderPath = Path.Combine(profilesFolder, guid);
        if (Directory.Exists(profileFolderPath))
        {
            Directory.Delete(profileFolderPath, true); // true = delete recursively
            Debug.Log($"[ProfileManager] Deleted profile folder: {profileFolderPath}");
        }

        // Delete save data
        SaveSystem.DeleteSave(guid);
        Debug.Log($"[ProfileManager] Deleted save data for: {guid}");

        // Remove profile index
        profileHeaders.RemoveAll(h => h.id == guid);
        SaveProfileIndex();

        // Clear profile in memory
        if (currentProfile != null && currentProfile.id == guid)
            currentProfile = null;

        Debug.Log($"[ProfileManager] Profile '{guid}' fully deleted.");
    }


}
