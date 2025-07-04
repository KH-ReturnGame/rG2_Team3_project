using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SongSelector : MonoBehaviour
{
    [Header("UI References")]
    public ScrollRect scrollRect;
    public Transform contentParent;
    public GameObject songItemPrefab;
    public SelectedSongInfo selectedSongInfo;
    public Button playButton;

    [Header("Scroll Settings")]
    public float scrollDuration;

    [Header("Song Database")]
    public List<SongData> songDatabase;

    [Header("Audio")]
    public AudioSource previewAudioSource;
    public float previewDelay = 0.5f;

    private List<SongItem> _songItems = new List<SongItem>();
    private SongData _currentSelectedSong;
    private SongItem _currentSelectedItem;
    private Coroutine _previewCoroutine;
    private Coroutine _scrollCoroutine;

    void Start()
    {
        InitializeSongDatabase();
        CreateSongList();
        SetupButtons();
        SelectSong(songDatabase[0]);
    }

    private void InitializeSongDatabase()
    {
        if (songDatabase.Count == 0)
        {
            songDatabase.Add(new SongData("Electric Dreams", "Synth Master", 3, 128f));
            songDatabase.Add(new SongData("Digital Heartbeat", "Cyber Pulse", 4, 140f));
            songDatabase.Add(new SongData("Neon Nights", "Retro Wave", 2, 120f));
            songDatabase.Add(new SongData("Bass Drop", "EDM King", 5, 150f));
            songDatabase.Add(new SongData("Melody Road", "Acoustic Soul", 1, 90f));
            songDatabase.Add(new SongData("Thunder Storm", "Rock Legend", 4, 145f));
            songDatabase.Add(new SongData("Starlight", "Dream Pop", 3, 110f));
            songDatabase.Add(new SongData("Pixel Adventure", "Chiptune Hero", 2, 135f));
            songDatabase.Add(new SongData("Electric Dreams", "Synth Master", 3, 128f));
            songDatabase.Add(new SongData("Digital Heartbeat", "Cyber Pulse", 4, 140f));
            songDatabase.Add(new SongData("Neon Nights", "Retro Wave", 2, 120f));
            songDatabase.Add(new SongData("Bass Drop", "EDM King", 5, 150f));
            songDatabase.Add(new SongData("Melody Road", "Acoustic Soul", 1, 90f));
            songDatabase.Add(new SongData("Thunder Storm", "Rock Legend", 4, 145f));
            songDatabase.Add(new SongData("Starlight", "Dream Pop", 3, 110f));
            songDatabase.Add(new SongData("Pixel Adventure", "Chiptune Hero", 2, 135f));
            songDatabase.Add(new SongData("Electric Dreams", "Synth Master", 3, 128f));
            songDatabase.Add(new SongData("Digital Heartbeat", "Cyber Pulse", 4, 140f));
            songDatabase.Add(new SongData("Neon Nights", "Retro Wave", 2, 120f));
            songDatabase.Add(new SongData("Bass Drop", "EDM King", 5, 150f));
            songDatabase.Add(new SongData("Melody Road", "Acoustic Soul", 1, 90f));
            songDatabase.Add(new SongData("Thunder Storm", "Rock Legend", 4, 145f));
            songDatabase.Add(new SongData("Starlight", "Dream Pop", 3, 110f));
            songDatabase.Add(new SongData("Pixel Adventure", "Chiptune Hero", 2, 135f));
        }
    }

    private void CreateSongList()
    {
        // 기존 아이템들 제거
        foreach (var item in _songItems)
        {
            if (item != null && item.gameObject != null)
                DestroyImmediate(item.gameObject);
        }
        _songItems.Clear();

        // 새로운 곡 아이템들 생성
        foreach (var song in songDatabase)
        {
            GameObject itemObj = Instantiate(songItemPrefab, contentParent);
            SongItem songItem = itemObj.GetComponent<SongItem>();

            if (songItem != null)
            {
                songItem.Initialize(song, this);
                _songItems.Add(songItem);
            }
        }

        // 스크롤 위치 초기화
        if (scrollRect)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void SetupButtons()
    {
        if (playButton)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
            playButton.interactable = false;
        }
    }

    public void SelectSong(SongData song)
    {
        // 이전 선택 해제
        if (_currentSelectedItem != null)
        {
            _currentSelectedItem.SetSelected(false);
        }

        // 새로운 선택
        _currentSelectedSong = song;
        _currentSelectedItem = _songItems.Find(item => item.GetSongData() == song);

        if (_currentSelectedItem != null)
        {
            _currentSelectedItem.SetSelected(true);
        }

        // UI 업데이트
        UpdateSelectedSongInfo();

        int currentIndex = songDatabase.IndexOf(song);
        ScrollToItemSmooth(currentIndex);

        if (playButton)
            playButton.interactable = true;

        // 미리듣기 시작
        StartPreview();
    }

    private void UpdateSelectedSongInfo()
    {
        Debug.Log(_currentSelectedSong.songName);
        if (selectedSongInfo && _currentSelectedSong != null)
        {
            selectedSongInfo.UpdateSongInfo(_currentSelectedSong);
        }
    }

    private void StartPreview()
    {
        if (_previewCoroutine != null)
        {
            StopCoroutine(_previewCoroutine);
        }

        if (previewAudioSource && _currentSelectedSong != null && _currentSelectedSong.songClip)
        {
            _previewCoroutine = StartCoroutine(PlayPreviewDelayed());
        }
    }

    private System.Collections.IEnumerator PlayPreviewDelayed()
    {
        yield return new WaitForSeconds(previewDelay);

        if (previewAudioSource && _currentSelectedSong != null && _currentSelectedSong.songClip)
        {
            previewAudioSource.clip = _currentSelectedSong.songClip;
            previewAudioSource.Play();
        }
    }

    private void OnPlayButtonClicked()
    {
        if (_currentSelectedSong != null)
        {
            Debug.Log($"게임 시작: {_currentSelectedSong.songName}");
            // 게임 씬 로드 로직
            // SceneManager.LoadScene("GameScene");
        }
    }

    // 외부에서 곡 추가
    public void AddSong(SongData newSong)
    {
        songDatabase.Add(newSong);
        CreateSongList();
    }

    // 선택된 곡 정보 반환
    public SongData GetSelectedSong()
    {
        return _currentSelectedSong;
    }

    // 검색 기능
    public void FilterSongs(string searchTerm)
    {
        foreach (var item in _songItems)
        {
            var songData = item.GetSongData();
            bool shouldShow = string.IsNullOrEmpty(searchTerm) ||
                            songData.songName.ToLower().Contains(searchTerm.ToLower()) ||
                            songData.artist.ToLower().Contains(searchTerm.ToLower());

            item.gameObject.SetActive(shouldShow);
        }
    }

    void OnDestroy()
    {
        if (_previewCoroutine != null)
        {
            StopCoroutine(_previewCoroutine);
        }
    }

    public void Update()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                int prevIndex = songDatabase.IndexOf(_currentSelectedSong);
                if (prevIndex <= 0) return;
                SelectSong(songDatabase[prevIndex - 1]);
            } else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                int prevIndex = songDatabase.IndexOf(_currentSelectedSong);
                if (prevIndex >= songDatabase.Count - 1) return;
                SelectSong(songDatabase[prevIndex + 1]);
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                SceneLoader.LoadScene("PlayScene");
            }
        }
    }

    public void ScrollToItemSmooth(int itemIndex)
    {
        if (_scrollCoroutine != null)
        {
            StopCoroutine(_scrollCoroutine);
        }

        _scrollCoroutine = StartCoroutine(SmoothScrollCoroutine(itemIndex));
    }

    private IEnumerator SmoothScrollCoroutine(int itemIndex)
    {
        yield return new WaitForEndOfFrame();

        float startPosition = scrollRect.verticalNormalizedPosition;
        float targetPosition = 1f - (float)itemIndex / (_songItems.Count - 1);
        float duration = Math.Abs(targetPosition - startPosition) * scrollDuration;
        
        // 스크롤뷰의 높이와 컨텐츠의 높이를 가져옵니다.
        float scrollViewHeight = scrollRect.GetComponent<RectTransform>().rect.height;
        float contentHeight = contentParent.GetComponent<RectTransform>().rect.height;
        float itemHeight =_songItems[0].GetComponent<RectTransform>().rect.height;

        // 아이템이 중앙에 오도록 목표 위치를 조정합니다.
        targetPosition = 1f - (itemIndex * itemHeight + (itemHeight / 2f) - (scrollViewHeight / 2f)) / (contentHeight - scrollViewHeight);
        targetPosition = Mathf.Clamp01(targetPosition);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentPosition = Mathf.Lerp(startPosition, targetPosition, t);
            scrollRect.verticalNormalizedPosition = currentPosition;
            yield return null;
        }

        scrollRect.verticalNormalizedPosition = targetPosition;
    }
}