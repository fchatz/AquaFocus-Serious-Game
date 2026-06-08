using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;


public class DashboardStatsController : MonoBehaviour
{
    [Header("UI References")]
    public Button buttonMap1;
    public Button buttonMap2;
    public Button buttonMap3;
    public Transform contentParent;    
    public GameObject rowTemplate;      
    public Button buttonBack;


    private string profileId;

    private void Start()
    {
        buttonMap1.onClick.AddListener(() => LoadMap1());
        buttonMap2.onClick.AddListener(() => LoadMap2());
        buttonMap3.onClick.AddListener(() => LoadMap3());
        buttonBack.onClick.AddListener(() => BackToMainMenu());
        profileId = GameDataManager.Instance.currentProfileId;
        LoadMap1();
    }

    public void LoadMap1()
    {
        ClearRows();
        string folder = GetFolder("telemetry_map1");
        var files = GetLatestCsvFiles(folder);
        foreach (string file in files)
            CreateRowFromCsv(file);
    }

    public void LoadMap2()
    {
        ClearRows();
        string folder = GetFolder("telemetry_map2");
        var files = GetLatestCsvFiles(folder);
        foreach (string file in files)
            CreateRowFromCsv(file);
    }

    public void LoadMap3()
    {
        ClearRows();
        string folder = GetFolder("telemetry_map3");
        var files = GetLatestCsvFiles(folder);
        foreach (string file in files)
            CreateRowFromCsv(file);
    }



    private string GetFolder(string folderName)
    {
        string root = Application.persistentDataPath;
        return Path.Combine(root, "profiles", profileId, folderName);
    }

    private List<string> GetLatestCsvFiles(string folder)
    {
        if (!Directory.Exists(folder))
            return new List<string>();

        return Directory
            .GetFiles(folder, "*.csv")
            .OrderByDescending(f => File.GetCreationTime(f))
            .Take(10)
            .ToList();
    }

    private void ClearRows()
    {
        foreach (Transform child in contentParent)
        {
            if (child.gameObject != rowTemplate)
                Destroy(child.gameObject);
        }
    }

    private void CreateRowFromCsv(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length < 2) return;

        string header = lines[0];
        string data = lines[1];

        string[] headers = header.Split(',');
        string[] values = data.Split(',');

        GameObject row = Instantiate(rowTemplate, contentParent);
        row.SetActive(true);

        // Get map value
        string mapName = GetValue(headers, values, "map");

        // Date
        var txtDate = row.transform.Find("Text_Date");
        if (txtDate != null)
            txtDate.GetComponent<TMP_Text>().text = GetValue(headers, values, "date");

        // Session duration
        string durationRaw = GetValue(headers, values, "session_duration_sec");
        row.transform.Find("Text_Duration").GetComponent<TMP_Text>().text =
            FormatDuration(durationRaw);


        // Accuracy
        string acc;

        if (mapName == "Map3")
        {
            acc = GetValue(headers, values, "go_accuracy");
        }
        else
        {
            acc = GetValue(headers, values, "accuracy");
            if (string.IsNullOrEmpty(acc))
                acc = GetValue(headers, values, "mine_accuracy");
        }

        var txtAcc = row.transform.Find("Text_Accuracy");
        if (txtAcc != null)
            txtAcc.GetComponent<TMP_Text>().text = acc;

        // Avg speed
        var txtSpeed = row.transform.Find("Text_AvgSpeed");
        if (txtSpeed != null)
            txtSpeed.GetComponent<TMP_Text>().text = GetValue(headers, values, "avg_speed");

        // Streak:
        // Map1 = max_streak
        // Map2 & Map3 = mine_max_streak
        string streak = GetValue(headers, values, "max_streak");
        if (string.IsNullOrEmpty(streak))
            streak = GetValue(headers, values, "mine_max_streak");

        var txtStreak = row.transform.Find("Text_Streak");
        if (txtStreak != null)
            txtStreak.GetComponent<TMP_Text>().text = streak;

        if (mapName == "Map3")
        {
            var txtBlue = row.transform.Find("Text_Blue");
            if (txtBlue != null)
                txtBlue.GetComponent<TMP_Text>().text = GetValue(headers, values, "blue_clicked");

            var txtRed = row.transform.Find("Text_Red");
            if (txtRed != null)
                txtRed.GetComponent<TMP_Text>().text = GetValue(headers, values, "red_clicked");

            var txtMines = row.transform.Find("Text_Mines");
            if (txtMines != null)
                txtMines.GetComponent<TMP_Text>().text = GetValue(headers, values, "total_hit_mines");
        }
        Debug.Log("RAW DURATION = '" + durationRaw + "'");

    }



    private string GetValue(string[] headers, string[] values, string name)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            if (headers[i].Trim() == name)
                return values[i];
        }
        return "";
    }

    public void BackToMainMenu()
    {
        LoadingManager.Instance.LoadScene("MainMenu");
    }


    private string FormatDuration(string rawSeconds)
    {
        if (string.IsNullOrWhiteSpace(rawSeconds))
            return "0:00";

        rawSeconds = rawSeconds.Trim();

        float sec;

        if (!float.TryParse(rawSeconds, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out sec))
        {
            if (!float.TryParse(rawSeconds, out sec))
                return "0:00";
        }

        int minutes = Mathf.FloorToInt(sec / 60f);
        int seconds = Mathf.FloorToInt(sec % 60f);

        return $"{minutes}:{seconds:00}";
    }





}
