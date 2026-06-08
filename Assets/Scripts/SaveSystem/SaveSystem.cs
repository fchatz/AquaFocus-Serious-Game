using UnityEngine;
using System.IO;

/// <summary>
/// Static subsystem responsible for profile-based local state persistence.
/// Handles I/O operations using Unity's platform-agnostic persistent data path and JSON serialization.
/// </summary>

public static class SaveSystem
{
    private static readonly string savesFolder =
        Path.Combine(Application.persistentDataPath, "saves");


    /// <summary>
    /// Computes the fully qualified absolute path for a specific profile's save state.
    /// Lazily initializes the underlying subdirectory structure if missing.
    /// </summary>
    private static string GetSavePath(string profileId)
    {
        if (!Directory.Exists(savesFolder))
            Directory.CreateDirectory(savesFolder);

        return Path.Combine(savesFolder, profileId + "_save.json");
    }

    /// <summary>
    /// Serializes the active session data into a pretty-printed JSON payload and writes it to disk.
    /// </summary>
    public static void Save(SaveData data, string profileId)
    {
        string path = GetSavePath(profileId);

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log("Saved to: " + path);
    }

    /// <summary>
    /// Retrieves and deserializes a profile's saved state from the persistent disk storage.
    /// Returns null if the file does not exist, deferring fallback initialization to the management layer
    /// to prevent structural side effects (e.g., accidental currency/coin wiping).
    /// </summary>
    public static SaveData Load(string profileId)
    {
        string path = GetSavePath(profileId);

        if (!File.Exists(path))
        {
            Debug.Log("No save found for profile " + profileId + ". Creating new save.");
            // Return null to allow the higher-level state manager to intercept and handle baseline generation gracefully
            return null;
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }

    /// <summary>
    /// Deletes the specified profile's state representation from the local file system.
    /// </summary>
    public static void DeleteSave(string profileId)
    {
        string path = GetSavePath(profileId);
        if (File.Exists(path))
            File.Delete(path);
    }
}