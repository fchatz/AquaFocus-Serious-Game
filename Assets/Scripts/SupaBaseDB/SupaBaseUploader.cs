using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class SupabaseUploader : MonoBehaviour
{
    public static SupabaseUploader Instance { get; private set; }

    [Header("Supabase Settings")]
    public string supabaseUrl = "";
    public string supabaseKey = "";

    private string PendingFolder => Path.Combine(Application.persistentDataPath, "pending_uploads");

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (!Directory.Exists(PendingFolder)) Directory.CreateDirectory(PendingFolder);
    }

    private void Start()
    {
        StartCoroutine(RetryPendingUploads());
    }

    /// <summary>
    /// Custom lightweight JSON serializer designed to avoid overhead from heavy external dependencies.
    /// Explicitly forces a dot decimal separator to ensure compatibility with PostgreSQL numeric locales.
    /// </summary>

    private string ToJson(Dictionary<string, object> dict)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        bool first = true;
        foreach (var kv in dict)
        {
            if (!first) sb.Append(",");
            first = false;
            sb.Append("\"").Append(kv.Key).Append("\":");
            if (kv.Value is string) sb.Append("\"").Append(kv.Value).Append("\"");
            else sb.Append(kv.Value.ToString().Replace(",", ".")); // Force invariant decimal format
        }
        sb.Append("}");
        return sb.ToString();
    }

    /// <summary>
    /// Parses localized session CSV data into a dynamic payload and pushes it to Supabase via REST API.
    /// Implements an offline-first fallback mechanism to survive unexpected network faults.
    /// </summary>

    public IEnumerator UploadCSV(string csvPath, string profileGuid, bool isRetry = false)
    {
        if (!File.Exists(csvPath)) yield break;

        string[] lines = File.ReadAllLines(csvPath);
        if (lines.Length < 2) yield break;

        string[] headers = lines[0].Split(',');
        string[] values = lines[1].Split(',');

        var data = new Dictionary<string, object>();
        data["profile_guid"] = profileGuid;

        for (int i = 0; i < headers.Length && i < values.Length; i++)
        {
            string key = headers[i].Trim();
            string raw = values[i].Trim();
            if (int.TryParse(raw, out int iv)) data[key] = iv;
            else if (float.TryParse(raw, out float fv)) data[key] = fv;
            else data[key] = raw;
        }

        string json = ToJson(data);
        string lowerName = Path.GetFileName(csvPath).ToLower();
        string tableName = lowerName.Contains("map2") ? "map2_sessions" : (lowerName.Contains("map3") ? "map3_sessions" : "map1_sessions");
        string url = supabaseUrl + tableName;

        UnityWebRequest www = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();

        // Security and schema context boundaries required for Row Level Security (RLS) evaluation
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("apikey", supabaseKey);
        www.SetRequestHeader("Authorization", "Bearer " + supabaseKey);
        www.SetRequestHeader("Content-Profile", "public");
        www.SetRequestHeader("Prefer", "return=representation");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            // Failures fallback gracefully to local persistent storage
            Debug.LogWarning("Upload failed: " + www.error + "\n" + www.downloadHandler.text);
            if (!isRetry) SavePendingCopy(csvPath, profileGuid);
        }
        else
        {
            Debug.Log("Upload success to " + tableName + ": " + www.downloadHandler.text);
            if (isRetry) File.Delete(csvPath);
        }
    }

    private void SavePendingCopy(string originalPath, string profileGuid)
    {
        string pendingName = profileGuid + "--" + Path.GetFileName(originalPath);
        string dest = Path.Combine(PendingFolder, pendingName);
        File.Copy(originalPath, dest, true);
        Debug.Log("Saved to pending: " + dest);
    }

    /// <summary>
    /// Periodic background synchronization routine that sweeps the local disk cache
    /// to re-transmit pending payloads once connectivity or permissions stabilize.
    /// </summary>

    private IEnumerator RetryPendingUploads()
    {
        while (true)
        {
            if (Directory.Exists(PendingFolder))
            {
                string[] files = Directory.GetFiles(PendingFolder);
                foreach (string file in files)
                {
                    yield return UploadCSV(file, ExtractProfileFromPending(file), true);
                }
            }
            yield return new WaitForSeconds(15f);
        }
    }

    private string ExtractProfileFromPending(string filePath)
    {
        string file = Path.GetFileName(filePath);
        if (file.Contains("--")) return file.Split(new string[] { "--" }, System.StringSplitOptions.None)[0];
        return "unknown";
    }
}