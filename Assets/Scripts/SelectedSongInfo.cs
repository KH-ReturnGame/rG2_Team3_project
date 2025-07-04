using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectedSongInfo: MonoBehaviour
{
    [Header("UI References")]
    public Image albumArtImage;
    public TextMeshProUGUI songName;
    public TextMeshProUGUI artist;
    public TextMeshProUGUI bpm;
    public TextMeshProUGUI level;

    public void UpdateSongInfo(SongData data)
    {
        if (albumArtImage && data.albumArt != null) albumArtImage.sprite = data.albumArt;
        songName.text = data.songName;
        artist.text = data.artist;
        bpm.text = $"BPM: {data.bpm}";
        level.text = data.difficulty.ToString();
    }
}