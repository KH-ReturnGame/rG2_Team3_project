using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SongItem : MonoBehaviour
{
    [Header("UI References")]
    public Image albumArtImage;
    public TextMeshProUGUI songNameText;
    public TextMeshProUGUI artistText;
    public TextMeshProUGUI difficultyText;
    public TextMeshProUGUI bpmText;
    public Button selectButton;
    public Image backgroundImage;
    public GameObject selectedIndicator;

    [Header("Visual Settings")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;
    public Color hoverColor = Color.gray;

    private SongData _songData;
    private SongSelector _songSelector;
    private bool _isSelected;

    public void Initialize(SongData data, SongSelector selector)
    {
        _songData = data;
        _songSelector = selector;
        
        // UI 요소 설정
        if (songNameText) songNameText.text = data.songName;
        if (artistText) artistText.text = data.artist;
        if (difficultyText) difficultyText.text = $"★{data.difficulty}";
        if (bpmText) bpmText.text = $"BPM: {data.bpm}";
        
        if (albumArtImage && data.albumArt)
            albumArtImage.sprite = data.albumArt;
        
        // 버튼 이벤트 설정
        if (selectButton)
        {
            selectButton.onClick.AddListener(OnSelectSong);
            
            // 호버 효과
            var trigger = selectButton.gameObject.GetComponent<EventTrigger>();
            if (!trigger) trigger = selectButton.gameObject.AddComponent<EventTrigger>();
            
            var pointerEnter = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((callData) => OnPointerEnter());
            trigger.triggers.Add(pointerEnter);
            
            var pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((callData) => OnPointerExit());
            trigger.triggers.Add(pointerExit);
        }
        
        UpdateVisuals();
    }

    private void OnSelectSong()
    {
        _songSelector.SelectSong(_songData);
    }

    private void OnPointerEnter()
    {
        if (!_isSelected && backgroundImage)
            backgroundImage.color = hoverColor;
    }

    private void OnPointerExit()
    {
        if (!_isSelected && backgroundImage)
            backgroundImage.color = normalColor;
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (backgroundImage)
        {
            backgroundImage.color = _isSelected ? selectedColor : normalColor;
        }

        if (selectedIndicator)
        {
            selectedIndicator.SetActive(_isSelected);
        }
    }

    public SongData GetSongData() => _songData;
}