using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NotePathPoint
{
    public float t;
    public float pos;
    public int bend;
}

[System.Serializable]
public class NoteData
{
    public float time;
    public int type;
    public int lane;
    public float length;
    public float speed;
}

public class Metadata
{
    public string title;
    public string artist;
    public float bpm;
    public float offset;
}

public class chartdata : MonoBehaviour
{
    public NoteData noteData;
    public float targetTime;
}
