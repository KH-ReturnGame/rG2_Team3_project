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
    public string type;
    public List<NotePathPoint> path = new List<NotePathPoint>();
    public float size = 1.0f;
    public float? bpmChange = null;
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
