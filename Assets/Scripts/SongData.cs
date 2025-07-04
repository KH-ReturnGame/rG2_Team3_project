using UnityEngine;

[System.Serializable]
public class SongData
{
    public string songName;
    public string artist;
    public Sprite albumArt;
    public AudioClip songClip;
    public int difficulty;
    public float bpm;

    public SongData(string name, string artist, int difficulty, float bpm)
    {
        this.songName = name;
        this.artist = artist;
        this.difficulty = difficulty;
        this.bpm = bpm;
    }
}