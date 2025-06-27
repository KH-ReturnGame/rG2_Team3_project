using UnityEngine;

public class SongData : MonoBehaviour
{
    public SongInfo SongInfo;

    public void DrawSongListItem(SongInfo songInfo)
    {
        SongInfo = songInfo;
    }
}
