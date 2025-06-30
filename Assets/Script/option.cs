using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;


public class Option : MonoBehaviour
{
    public Button[] btns;
    public GameObject[] panels;
    public TMP_Dropdown resDrop, winDrop;
    private LocalizedString windowed, borderless, fullscreen;
    public TMP_InputField fpsInput;
    public Toggle vsyncToggle, showFpsToggle;
    private float deltaTime = 0f;

    void Start()
    {
        // UI 초기화
        InitializeButtons();
        InitializeResolutionDropdown();
        InitializeWindowModeDropdown();
        UpdateVSyncToggle();

        // InputField 초기화
        UpdateFPSInput();

        // 기본값 설정
        OnButtonClick(0); // 첫 번째 버튼 누르기
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        if (!showFpsToggle.isOn) return;
        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(10, 10, Screen.width, Screen.height);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        float ms = deltaTime * 1000f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.} FPS ({1:0.0} ms)", fps, ms);

        GUI.Label(rect, text, style);
    }

    void InitializeButtons()
    {
        for (int i = 0; i < btns.Length; i++)
        {
            int index = i;
            btns[i].onClick.AddListener(() => OnButtonClick(index));
        }
    }

    void OnButtonClick(int index)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == index);
            btns[i].interactable = i != index;

            var rectTransform = btns[i].GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, i == index ? 35 : 25);
        }
    }

    void InitializeResolutionDropdown()
    {
        resDrop.ClearOptions();
        var resolutions = Screen.resolutions;
        HashSet<string> uniqueResolutions = new HashSet<string>();

        foreach (var res in resolutions)
        {
            string resolutionString = $"{res.width}x{res.height}";
            if (uniqueResolutions.Add(resolutionString))
            {
                resDrop.options.Add(new TMP_Dropdown.OptionData(resolutionString));
            }
        }

        resDrop.onValueChanged.AddListener(index =>
        {
            string[] resolutionParts = resDrop.options[index].text.Split('x');
            int width = int.Parse(resolutionParts[0]);
            int height = int.Parse(resolutionParts[1]);
            Screen.SetResolution(width, height, Screen.fullScreenMode);
        });

        resDrop.value = 0;
    }

    void InitializeWindowModeDropdown()
    {
        // Initialize the localization keys for window modes
        windowed = new LocalizedString { TableReference = "option", TableEntryReference = "windowed" };
        borderless = new LocalizedString { TableReference = "option", TableEntryReference = "borderless" };
        fullscreen = new LocalizedString { TableReference = "option", TableEntryReference = "fullscreen" };

        UpdateDropdownOptions();

        winDrop.onValueChanged.AddListener(index =>
        {
            // Apply the selected window mode
            switch (index)
            {
                case 0:
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                    break;
                case 1:
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                    break;
                case 2:
                    Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                    break;
            }
        });

        winDrop.value = 0; // Set default value
    }

    void UpdateDropdownOptions()
    {
        winDrop.ClearOptions();

        // Load localized strings for dropdown options
        windowed.GetLocalizedStringAsync().Completed += handle =>
        {
            string windowedText = handle.Result;

            borderless.GetLocalizedStringAsync().Completed += handle2 =>
            {
                string borderlessText = handle2.Result;

                fullscreen.GetLocalizedStringAsync().Completed += handle3 =>
                {
                    string fullscreenText = handle3.Result;

                    // Add localized options to the dropdown
                    winDrop.options.Add(new TMP_Dropdown.OptionData(windowedText));
                    winDrop.options.Add(new TMP_Dropdown.OptionData(borderlessText));
                    winDrop.options.Add(new TMP_Dropdown.OptionData(fullscreenText));
                };
            };
        };
    }

    void UpdateVSyncToggle()
    {
        // Set the toggle to match the current VSync state
        vsyncToggle.isOn = QualitySettings.vSyncCount > 0;
        UpdateFPSInputInteractivity();

        // Add listener for the toggle change
        vsyncToggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
            {
                // Enable VSync
                QualitySettings.vSyncCount = 1;
                Application.targetFrameRate = -1; // Reset to unlimited since VSync manages FPS
            }
            else
            {
                // Disable VSync
                QualitySettings.vSyncCount = 0;

                // Set frame rate from FPS input
                if (int.TryParse(fpsInput.text, out int fps))
                {
                    Application.targetFrameRate = fps;
                }
            }

            UpdateFPSInputInteractivity();
        });
    }

    void UpdateFPSInput()
    {
        // Set the input field to show "Unlimited" when target frame rate is -1
        fpsInput.text = Application.targetFrameRate == -1 ? "-1" : Application.targetFrameRate.ToString();

        // Add listener for changes in the input field
        fpsInput.onEndEdit.AddListener(value =>
        {
            if (value == "-1")
            {
                Application.targetFrameRate = -1; // Set unlimited frame rate
                fpsInput.text = "-1"; // Reflect the value in the input field
            }
            else if (int.TryParse(value, out int fps))
            {
                Application.targetFrameRate = fps; // Set to the entered frame rate
            }
        });
    }

    void UpdateFPSInputInteractivity()
    {
        fpsInput.interactable = !vsyncToggle.isOn;
    }
}
