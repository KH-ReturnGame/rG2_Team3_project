using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SongListExplorer : MonoBehaviour
{
    [Header("UI References")]
    public GameObject songPrefab;
	public ScrollView songList;

    private Dictionary<string, SongInfo> _songs;

    void Start()
    {
        InitSongList();
        UpdateScrollView();
    }

    void InitSongList()
    {
        _songs = new Dictionary<string, SongInfo>();
        _songs.Add("Marenol", new SongInfo("Marenol", "LeaF", 111, new List<int>{18}));
        _songs.Add("MopeMope", new SongInfo("MopeMope", "LeaF", 111, new List<int>{15}));
    }

    void UpdateScrollView()
    {
        foreach (var song in _songs)
        {
            GameObject songListItem = Instantiate(songPrefab);
            songListItem.GetComponent<SongData>().DrawSongListItem(song.Value);
			//songList.Add(songListItem);
        }
    }
}

public class SongInfo
{
    private string _songName, _composer;
    private int _bpm;
    private List<int> _difficulties;

    public SongInfo(string songName, string composer, int bpm, List<int> difficulties)
    {
        this._songName = songName;
        this._composer = composer;
        this._bpm = bpm;
        this._difficulties = difficulties;
    }
}