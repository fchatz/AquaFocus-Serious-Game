using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Audio;
using System.Collections;

public class OptionsMenu : MonoBehaviour
{
    [Header("UI")]
    public Slider masterVolumeSlider;
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown resolutionDropdown;

    public AudioMixer masterMixer;

    private bool ignoreQualityCallback = false;
    private Coroutine qualityRoutine;
    Resolution[] resolutions;

    private void Start()
    {
        // ----- RESOLUTIONS -----
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width} x {resolutions[i].height}";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);

        // ----- LOAD FROM MANAGER -----
        var data = OptionsManager.Instance.Data;

        masterVolumeSlider.value = data.masterVolume;
        qualityDropdown.value = Mathf.Clamp(data.qualityLevel, 0, QualitySettings.names.Length - 1);

        // Load saved resolution index
        int savedResIndex = data.resolutionIndex >= 0 ? data.resolutionIndex : currentResolutionIndex;
        savedResIndex = Mathf.Clamp(savedResIndex, 0, resolutions.Length - 1);
        resolutionDropdown.value = savedResIndex;
        resolutionDropdown.RefreshShownValue();

        // ----- APPLY VALUES -----
        ApplyMasterVolume(data.masterVolume);
        ApplyQuality(qualityDropdown.value);
        ApplyResolution(savedResIndex);

        // ----- HOOK EVENTS -----
        masterVolumeSlider.onValueChanged.AddListener(ApplyMasterVolume);
        qualityDropdown.onValueChanged.AddListener(ApplyQuality);
        resolutionDropdown.onValueChanged.AddListener(ApplyResolution);
    }

    // MASTER VOLUME
    public void ApplyMasterVolume(float value)
    {
        OptionsManager.Instance.Data.masterVolume = value;
        OptionsManager.Instance.Save();

        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;

        masterMixer.SetFloat("MasterVolume", dB);
        Debug.Log($"[AUDIO] Master Volume set to {value:F2} ( {dB:F2} dB )");
    }

    // QUALITY
    public void ApplyQuality(int index)
    {
        if (ignoreQualityCallback)
            return;

        if (qualityRoutine != null)
            StopCoroutine(qualityRoutine);

        qualityRoutine = StartCoroutine(ApplyQualityDelayed(index));
    }

    private IEnumerator ApplyQualityDelayed(int index)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.05f);

        ignoreQualityCallback = true;

        QualitySettings.SetQualityLevel(index, true);

        OptionsManager.Instance.Data.qualityLevel = index;
        OptionsManager.Instance.Save();

        qualityDropdown.SetValueWithoutNotify(index);

        Debug.Log($"QUALITY LEVEL APPLIED: {QualitySettings.names[index]}");
        var rp = QualitySettings.renderPipeline;
        Debug.Log("URP Loaded Asset: " + rp.name);

        ignoreQualityCallback = false;
        qualityRoutine = null;
    }

    // RESOLUTION
    public void ApplyResolution(int index)
    {
        if (resolutions == null || resolutions.Length == 0)
            return;

        index = Mathf.Clamp(index, 0, resolutions.Length - 1);

        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);

        OptionsManager.Instance.Data.resolutionIndex = index;
        OptionsManager.Instance.Save();

        Debug.Log($"[RESOLUTION CHANGED] {res.width} x {res.height}");
    }

    // BUTTONS
    public void OnSaveButton()
    {
        OptionsManager.Instance.Save();
        Debug.Log("Options saved.");
    }

    public void OnMenuButton()
    {
        Debug.Log("Returning to Main Menu...");
        LoadingManager.Instance.LoadScene("MainMenu");
    }
}
