using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VideoSettings : MonoBehaviour
{
    [SerializeField] TMP_Dropdown levelOfDetailDropdown;
    [SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] Toggle isFullscreen;

    List<int> width = new List<int>() { 960, 1280, 1280, 1440, 1920 };
    List<int> height = new List<int>() { 540, 720, 1024, 810, 1080 };
    
    void Start()
    {
        isFullscreen.isOn = Screen.fullScreen; 
        SetupResolution(); 
        levelOfDetailDropdown.value = QualitySettings.GetQualityLevel();
    }

    public void FullscreenMode(bool active)
    {
        Screen.fullScreen = active;
        print(active);
    }

    #region Resolution
 
    void SetupResolution()
    {
        List<string> resOptions = new List<string>();
        resolutionDropdown.ClearOptions();

        int currentResolutionIndex = 0;
        for (int i = 0; i < width.Count; i++)
        {
            resOptions.Add(width[i] + "x" + height[i]);

            // Check if current resolution matches the available resolutions
            if (Screen.width == width[i] && Screen.height == height[i])
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(resOptions);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }
    public void SetResolution(int resolutionIndex)
    {
        Screen.SetResolution(width[resolutionIndex], height[resolutionIndex], Screen.fullScreen);
    }
    #endregion

    public void SetLevelOfDeatail(int quality)
    {
        print(quality);
        QualitySettings.SetQualityLevel(quality, true);
    }
}
