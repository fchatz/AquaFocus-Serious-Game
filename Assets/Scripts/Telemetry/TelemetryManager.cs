using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;

/// <summary>
/// Represents a single telemetry data point captured during gameplay.
/// </summary>
[System.Serializable]
public class TelemetryEvent
{
    public float time;
    public string type;
    public float speed;
    public float offset;
    public int streak;

    public TelemetryEvent(float time, string type, float speed, float offset, int streak)
    {
        this.time = time;
        this.type = type;
        this.speed = speed;
        this.offset = offset;
        this.streak = streak;
    }
}

/// <summary>
/// Evaluated performance indicators specifically structured for Map 1 (focused on manual tracking and baseline collection).
/// </summary>
[System.Serializable]
public class TelemetrySummaryMap1
{
    public float session_duration_sec;
    public int total_collected;
    public int total_missed;
    public float accuracy;
    public float avg_speed;
    public int speed_penalty_events;
    public float avg_manual_offset;
    public int max_streak;
    public int total_red_collected;
}

/// <summary>
/// Evaluated performance indicators structured for Map 2 (focused on obstacle avoidance and Go/No-Go cognitive tasks).
/// </summary>
[System.Serializable]
public class TelemetrySummaryMap2
{
    public float session_duration_sec;
    public float avg_speed;
    public int total_passed_mines;
    public int total_hit_mines;
    public float mine_accuracy;
    public int mine_speed_penalty_events;
    public int mine_max_streak;

    public int squid_total_blue_shown;
    public int squid_total_blue_clicked;
    public int squid_total_blue_missed;

    public int squid_total_red_shown;
    public int squid_total_red_clicked;
    public int squid_total_red_ignored;

    public float squid_go_accuracy;      // clicked blue / blue shown
    public float squid_nogo_accuracy;    // ignored red / red shown

    public float avg_blue_reaction_time;
    public float avg_red_reaction_time;  // impulsive reaction time
}

/// <summary>
/// Evaluated performance indicators structured for Map 3, consolidating tracking, avoidance, and Go/No-Go execution metrics.
/// </summary>
[System.Serializable]
public class TelemetrySummaryMap3
{
    public float session_duration_sec;
    public float avg_speed;

    // Jellyfish (Map1 logic)
    public int jelly_total_collected;
    public int jelly_total_missed;
    public float jelly_accuracy;
    public int jelly_total_red_collected;

    // Mines (Map2 logic)
    public int total_passed_mines;
    public int total_hit_mines;
    public float mine_accuracy;
    public int mine_speed_penalty_events;
    public int mine_max_streak;

    // Squids (Map2 logic)
    public int squid_total_blue_shown;
    public int squid_total_blue_clicked;
    public int squid_total_blue_missed;

    public int squid_total_red_shown;
    public int squid_total_red_clicked;
    public int squid_total_red_ignored;

    public float squid_go_accuracy;      // clicked blue / blue shown
    public float squid_nogo_accuracy;    // ignored red / red shown

    public float avg_blue_reaction_time;
    public float avg_red_reaction_time;
}

/// <summary>
/// Data wrapper representing an entire tracking session, containing metadata, chronologically ordered events, and contextual summaries.
/// </summary>
[System.Serializable]
public class TelemetrySession
{
    public string session_id;
    public string mapName;
    public string profileId;

    public List<TelemetryEvent> events = new List<TelemetryEvent>();

    [NonSerialized]
    public object summary;

    public TelemetrySummaryMap1 summaryMap1 = null;
    public TelemetrySummaryMap2 summaryMap2 = null;
    public TelemetrySummaryMap3 summaryMap3 = null;
}

/// <summary>
/// Singleton manager handling cognitive training telemetry capture, aggregate metric compilation, and data serialization (JSON/CSV) for local storage and database upload syncs.
/// </summary>
public class TelemetryManager : MonoBehaviour
{
    public static TelemetryManager Instance { get; private set; }

    private TelemetrySession currentSession;
    private float sessionStartTime;
    private float speedSum = 0f;
    private float offsetSum = 0f;
    private int offsetCount = 0;
    private int currentMaxStreak = 0;

    private string currentMapName = "Map1";

    public string CurrentMapName => currentSession.mapName;

    public TelemetrySummaryMap3 SummaryMap3 => currentSession.summaryMap3;
    public TelemetrySummaryMap2 SummaryMap2 => currentSession.summaryMap2;
    public TelemetrySummaryMap1 SummaryMap1 => currentSession.summaryMap1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;
        Debug.Log($"[Telemetry] Awake. persistentDataPath = {Application.persistentDataPath}");
    }

    private void Update()
    {
        // Debug manual export trigger
        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.pKey.wasPressedThisFrame)
        {
            ExportTelemetry();
        }
    }

    /// <summary>
    /// Initializes a clean telemetry session configured for a specific scene mapping.
    /// Allocates appropriate specialized summary objects based on the active tracking parameters.
    /// </summary>
    public void StartNewSession(string mapName)
    {
        currentMapName = mapName;

        currentSession = new TelemetrySession();
        currentSession.session_id = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        currentSession.mapName = currentMapName;

        if (GameDataManager.Instance != null)
            currentSession.profileId = GameDataManager.Instance.currentProfileId;

        // Reset all summaries to maintain state encapsulation
        currentSession.summaryMap1 = null;
        currentSession.summaryMap2 = null;
        currentSession.summaryMap3 = null;
        currentSession.summary = null;

        bool isMap2 = currentMapName.Contains("Map2");
        bool isMap3 = currentMapName.Contains("Map3");

        if (isMap2)
        {
            var s2 = new TelemetrySummaryMap2();
            currentSession.summary = s2;
            currentSession.summaryMap2 = s2;
        }
        else if (isMap3)
        {
            var s3 = new TelemetrySummaryMap3();
            currentSession.summary = s3;
            currentSession.summaryMap3 = s3;
        }
        else
        {
            var s1 = new TelemetrySummaryMap1();
            currentSession.summary = s1;
            currentSession.summaryMap1 = s1;
        }

        currentSession.events = new List<TelemetryEvent>();
        sessionStartTime = Time.time;
        speedSum = 0f;
        offsetSum = 0f;
        offsetCount = 0;
        currentMaxStreak = 0;

        Debug.Log($"[Telemetry] Started new session. Map={currentMapName}, Profile={currentSession.profileId}");
    }

    /// <summary>
    /// Backwards-compatible overload routing fallback initialization to Map1 context.
    /// </summary>
    public void StartNewSession()
    {
        StartNewSession("Map1");
    }

    /// <summary>
    /// Appends a structured telemetry event data-point to the active tracking history.
    /// Updates rolling sums used for compiling session-end statistical averages.
    /// </summary>
    public void LogEvent(string type, float speed, float offset = 0f, int streak = 0)
    {
        if (currentSession == null) return;

        float elapsed = Time.time - sessionStartTime;
        currentSession.events.Add(new TelemetryEvent(elapsed, type, speed, offset, streak));

        // Aggregate tracking variables for post-processing calculations
        speedSum += speed;
        offsetSum += offset;
        offsetCount++;

        if (streak > currentMaxStreak)
            currentMaxStreak = streak;
    }

    /// <summary>
    /// Compyles runtime metrics, handles directory structural initialization per profile,
    /// serializes specific raw metrics to JSON, and exports statistical summary reports via CSV.
    /// </summary>
    public void ExportTelemetry()
    {
        if (currentSession == null || currentSession.events.Count == 0)
        {
            Debug.LogWarning("Warning: No telemetry data to export!");
            return;
        }

        // Build comprehensive statistical performance metrics
        BuildSummary();

        string root = Application.persistentDataPath;
        string profileId = GameDataManager.Instance != null ? GameDataManager.Instance.currentProfileId : null;

        string folderPath;

        if (!string.IsNullOrEmpty(profileId))
        {
            string profileFolder = Path.Combine(root, "profiles", profileId);

            if (!Directory.Exists(profileFolder))
                Directory.CreateDirectory(profileFolder);

            // Determine target folder path based on active scene architecture
            string telemetryFolderName;
            if (currentMapName.Contains("Map2"))
                telemetryFolderName = "telemetry_map2";
            else if (currentMapName.Contains("Map3"))
                telemetryFolderName = "telemetry_map3";
            else
                telemetryFolderName = "telemetry_map1";

            folderPath = Path.Combine(profileFolder, telemetryFolderName);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }
        else
        {
            // Fallback directory path allocated for testing context
            folderPath = Path.Combine(root, "TelemetryLogs");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }

        string mapToken = string.IsNullOrEmpty(currentMapName) ? "Map" : currentMapName;
        string fileName = $"{currentSession.session_id}_{mapToken}.json";
        string filePath = Path.Combine(folderPath, fileName);

        string csvFileName = $"{currentSession.session_id}_{mapToken}.csv";
        string json = "";

        // Strip non-active context schemas to keep clean exports
        if (currentMapName.Contains("Map2"))
        {
            currentSession.summaryMap1 = null;
            currentSession.summaryMap3 = null;
        }
        else if (currentMapName.Contains("Map3"))
        {
            currentSession.summaryMap1 = null;
            currentSession.summaryMap2 = null;
        }
        else
        {
            currentSession.summaryMap2 = null;
            currentSession.summaryMap3 = null;
        }

        // Handle structural payload serialization via explicit generic exports
        if (currentMapName.Contains("Map2"))
        {
            var exportObj = new TelemetrySessionExport<TelemetrySummaryMap2>()
            {
                session_id = currentSession.session_id,
                mapName = currentSession.mapName,
                profileId = currentSession.profileId,
                events = currentSession.events,
                summary = currentSession.summaryMap2
            };
            json = JsonUtility.ToJson(exportObj, true);
        }
        else if (currentMapName.Contains("Map3"))
        {
            var exportObj = new TelemetrySessionExport<TelemetrySummaryMap3>()
            {
                session_id = currentSession.session_id,
                mapName = currentSession.mapName,
                profileId = currentSession.profileId,
                events = currentSession.events,
                summary = currentSession.summaryMap3
            };
            json = JsonUtility.ToJson(exportObj, true);
        }
        else
        {
            var exportObj = new TelemetrySessionExport<TelemetrySummaryMap1>()
            {
                session_id = currentSession.session_id,
                mapName = currentSession.mapName,
                profileId = currentSession.profileId,
                events = currentSession.events,
                summary = currentSession.summaryMap1
            };
            json = JsonUtility.ToJson(exportObj, true);
        }

        File.WriteAllText(filePath, json);

        // Generate flat analytical spreadsheet report
        ExportSummaryCSV(folderPath, csvFileName);

        Debug.Log($"Telemetry exported: {filePath}");

        string csvFullPath = Path.Combine(folderPath, csvFileName);
        StartCoroutine(SupabaseUploader.Instance.UploadCSV(csvFullPath, currentSession.profileId));
    }

    /// <summary>
    /// Processes captured data pools to extract cross-examined cognitive efficiency metrics.
    /// Pulls and binds behavioral values from specialized subsystem singletons where necessary.
    /// </summary>
    private void BuildSummary()
    {
        float duration = Time.time - sessionStartTime;

        bool isMap2 = currentSession.mapName.Contains("Map2");
        bool isMap3 = currentSession.mapName.Contains("Map3");

        if (isMap2)
        {
            // ------ MAP 2 SUMMARY COMPILATION ------
            var s = (TelemetrySummaryMap2)currentSession.summary;

            s.session_duration_sec = duration;
            s.avg_speed = offsetCount > 0 ? speedSum / offsetCount : 0f;

            s.total_hit_mines = CountEvents("mine_hit");
            s.total_passed_mines = CountEvents("mine_avoided");

            int total = s.total_hit_mines + s.total_passed_mines;
            s.mine_accuracy = total > 0 ? (float)s.total_passed_mines / total : 0f;

            s.mine_speed_penalty_events = CountEvents("speed_penalty");
            s.mine_max_streak = currentMaxStreak;

            var squid = FindFirstObjectByType<ButtonStreakController>();
            if (squid != null)
            {
                s.squid_total_blue_shown = squid.totalBlueShown;
                s.squid_total_blue_clicked = squid.totalBlueClicked;
                s.squid_total_blue_missed = squid.totalBlueMissed;

                s.squid_total_red_shown = squid.totalRedShown;
                s.squid_total_red_clicked = squid.totalRedClicked;
                s.squid_total_red_ignored = squid.totalRedIgnored;

                s.squid_go_accuracy = (squid.totalBlueShown > 0)
                    ? (float)squid.totalBlueClicked / squid.totalBlueShown
                    : 0f;

                s.squid_nogo_accuracy = (squid.totalRedShown > 0)
                    ? (float)squid.totalRedIgnored / squid.totalRedShown
                    : 0f;

                s.avg_blue_reaction_time = (squid.blueReactionCount > 0)
                    ? squid.totalBlueReactionTime / squid.blueReactionCount
                    : 0f;

                s.avg_red_reaction_time = (squid.redReactionCount > 0)
                    ? squid.totalRedReactionTime / squid.redReactionCount
                    : 0f;
            }
        }
        else if (isMap3)
        {
            // ------ MAP 3 SUMMARY COMPILATION (COMBINED ARCHITECTURE) ------
            var s = (TelemetrySummaryMap3)currentSession.summary;

            s.session_duration_sec = duration;
            s.avg_speed = offsetCount > 0 ? speedSum / offsetCount : 0f;
            s.mine_max_streak = currentMaxStreak;

            // Extract Tracking Metrics from ScoreManager (Map1 logic core)
            if (ScoreManager.Instance != null)
            {
                s.jelly_total_collected = ScoreManager.Instance.GetCollectedCount();
                s.jelly_total_missed = ScoreManager.Instance.GetMissedCount();

                int totalJelly = s.jelly_total_collected + s.jelly_total_missed;
                s.jelly_accuracy = totalJelly > 0
                    ? (float)s.jelly_total_collected / totalJelly
                    : 0f;

                s.jelly_total_red_collected = ScoreManager.Instance.GetWrongCount();
            }

            // Extract Avoidance Metrics from ScoreManagerMap2 (Map2 logic core)
            if (ScoreManagerMap2.Instance != null)
            {
                s.total_passed_mines = ScoreManagerMap2.Instance.totalPassedMines;
                s.total_hit_mines = ScoreManagerMap2.Instance.totalHitMines;

                int totalMines = s.total_hit_mines + s.total_passed_mines;
                s.mine_accuracy = totalMines > 0
                    ? (float)s.total_passed_mines / totalMines
                    : 0f;
            }

            s.mine_speed_penalty_events = CountEvents("speed_penalty");

            // Extract Attention/Inhibition Performance from ButtonStreakController
            var squid = FindFirstObjectByType<ButtonStreakController>();
            if (squid != null)
            {
                s.squid_total_blue_shown = squid.totalBlueShown;
                s.squid_total_blue_clicked = squid.totalBlueClicked;
                s.squid_total_blue_missed = squid.totalBlueMissed;

                s.squid_total_red_shown = squid.totalRedShown;
                s.squid_total_red_clicked = squid.totalRedClicked;
                s.squid_total_red_ignored = squid.totalRedIgnored;

                s.squid_go_accuracy = (squid.totalBlueShown > 0)
                    ? (float)squid.totalBlueClicked / squid.totalBlueShown
                    : 0f;

                s.squid_nogo_accuracy = (squid.totalRedShown > 0)
                    ? (float)squid.totalRedIgnored / squid.totalRedShown
                    : 0f;

                s.avg_blue_reaction_time = (squid.blueReactionCount > 0)
                    ? squid.totalBlueReactionTime / squid.blueReactionCount
                    : 0f;

                s.avg_red_reaction_time = (squid.redReactionCount > 0)
                    ? squid.totalRedReactionTime / squid.redReactionCount
                    : 0f;
            }
        }
        else
        {
            // ------ MAP 1 SUMMARY COMPILATION ------
            var s = (TelemetrySummaryMap1)currentSession.summary;

            s.session_duration_sec = duration;
            s.avg_speed = offsetCount > 0 ? speedSum / offsetCount : 0f;
            s.avg_manual_offset = offsetCount > 0 ? offsetSum / offsetCount : 0f;
            s.max_streak = currentMaxStreak;

            s.total_collected = CountEvents("collect");
            s.total_missed = CountEvents("miss");

            int total = s.total_collected + s.total_missed;
            s.accuracy = total > 0 ? (float)s.total_collected / total : 0f;

            s.speed_penalty_events = CountEvents("penalty");
            s.total_red_collected = CountEvents("collect_red");
        }
    }

    /// <summary>
    /// Helper calculation method parsing history logs to count specific event frequency.
    /// </summary>
    public int CountEvents(string type)
    {
        int count = 0;
        foreach (var e in currentSession.events)
        {
            if (e.type == type)
                count++;
        }
        return count;
    }

    /// <summary>
    /// Generic transport wrapper targeting strictly modular payload compilation for JSON serialization.
    /// </summary>
    [System.Serializable]
    public class TelemetrySessionExport<T>
    {
        public string session_id;
        public string mapName;
        public string profileId;
        public List<TelemetryEvent> events;
        public T summary;
    }

    /// <summary>
    /// Formats data into comma-separated flat tables structured appropriately per scene environment context.
    /// Enforces invariant baseline cultures to resolve float point discrepancies.
    /// </summary>
    private void ExportSummaryCSV(string folderPath, string fileName)
    {
        string csvFile = Path.Combine(folderPath, fileName);

        // Helper string method maintaining consistent dot separators across regional settings
        string F(float v) => v.ToString("0.######", CultureInfo.InvariantCulture);

        using (StreamWriter sw = new StreamWriter(csvFile, false))
        {
            // -------- CSV WRITER: MAP 1 STRUCTURE --------
            if (currentMapName != "Map2" && currentSession.summaryMap1 != null)
            {
                var s = currentSession.summaryMap1;

                sw.WriteLine(
                    "session_id,date,map," +
                    "session_duration_sec,total_collected,total_missed," +
                    "accuracy,avg_speed,speed_penalty_events,avg_manual_offset,max_streak"
                );

                sw.WriteLine(
                    $"{currentSession.session_id}," +
                    $"{DateTime.Now:yyyy-MM-dd}," +
                    $"{currentMapName}," +
                    $"{F(s.session_duration_sec)}," +
                    $"{s.total_collected}," +
                    $"{s.total_missed}," +
                    $"{F(s.accuracy)}," +
                    $"{F(s.avg_speed)}," +
                    $"{s.speed_penalty_events}," +
                    $"{F(s.avg_manual_offset)}," +
                    $"{s.max_streak}"
                );
            }

            // -------- CSV WRITER: MAP 2 STRUCTURE --------
            if (currentMapName == "Map2" && currentSession.summaryMap2 != null)
            {
                var s = currentSession.summaryMap2;

                sw.WriteLine(
                    "session_id,date,map," +
                    "session_duration_sec,avg_speed," +
                    "total_passed_mines,total_hit_mines,mine_accuracy," +
                    "mine_speed_penalty,mine_max_streak," +
                    "blue_shown,blue_clicked,blue_missed," +
                    "red_shown,red_clicked,red_ignored," +
                    "go_accuracy,nogo_accuracy," +
                    "avg_blue_rt,avg_red_rt"
                );

                sw.WriteLine(
                    $"{currentSession.session_id}," +
                    $"{DateTime.Now:yyyy-MM-dd}," +
                    $"{currentMapName}," +
                    $"{F(s.session_duration_sec)}," +
                    $"{F(s.avg_speed)}," +
                    $"{s.total_passed_mines}," +
                    $"{s.total_hit_mines}," +
                    $"{F(s.mine_accuracy)}," +
                    $"{s.mine_speed_penalty_events}," +
                    $"{s.mine_max_streak}," +
                    $"{s.squid_total_blue_shown}," +
                    $"{s.squid_total_blue_clicked}," +
                    $"{s.squid_total_blue_missed}," +
                    $"{s.squid_total_red_shown}," +
                    $"{s.squid_total_red_clicked}," +
                    $"{s.squid_total_red_ignored}," +
                    $"{F(s.squid_go_accuracy)}," +
                    $"{F(s.squid_nogo_accuracy)}," +
                    $"{F(s.avg_blue_reaction_time)}," +
                    $"{F(s.avg_red_reaction_time)}"
                );
            }

            // -------- CSV WRITER: MAP 3 STRUCTURE --------
            if (currentMapName.Contains("Map3") && currentSession.summaryMap3 != null)
            {
                var s = currentSession.summaryMap3;

                sw.WriteLine(
                    "session_id,date,map," +
                    "session_duration_sec,avg_speed," +
                    "jelly_collected,jelly_missed,jelly_accuracy,jelly_red_collected," +
                    "total_passed_mines,total_hit_mines,mine_accuracy,mine_speed_penalty,mine_max_streak," +
                    "blue_shown,blue_clicked,blue_missed," +
                    "red_shown,red_clicked,red_ignored," +
                    "go_accuracy,nogo_accuracy," +
                    "avg_blue_rt,avg_red_rt"
                );

                sw.WriteLine(
                    $"{currentSession.session_id}," +
                    $"{DateTime.Now:yyyy-MM-dd}," +
                    $"{currentMapName}," +
                    $"{F(s.session_duration_sec)}," +
                    $"{F(s.avg_speed)}," +
                    $"{s.jelly_total_collected}," +
                    $"{s.jelly_total_missed}," +
                    $"{F(s.jelly_accuracy)}," +
                    $"{s.jelly_total_red_collected}," +
                    $"{s.total_passed_mines}," +
                    $"{s.total_hit_mines}," +
                    $"{F(s.mine_accuracy)}," +
                    $"{s.mine_speed_penalty_events}," +
                    $"{s.mine_max_streak}," +
                    $"{s.squid_total_blue_shown}," +
                    $"{s.squid_total_blue_clicked}," +
                    $"{s.squid_total_blue_missed}," +
                    $"{s.squid_total_red_shown}," +
                    $"{s.squid_total_red_clicked}," +
                    $"{s.squid_total_red_ignored}," +
                    $"{F(s.squid_go_accuracy)}," +
                    $"{F(s.squid_nogo_accuracy)}," +
                    $"{F(s.avg_blue_reaction_time)}," +
                    $"{F(s.avg_red_reaction_time)}"
                );
            }
        }

        Debug.Log($"CSV exported: {csvFile}");
    }
}