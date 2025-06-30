using UnityEngine;
using System.Collections.Generic;

public class JudgeManager : MonoBehaviour
{
    [SerializeField] private NoteManager noteManager;
    [SerializeField] private GameObject subjectObject;

    public float offset = 0f;
    public float perfectRange = 50f;
    public float greatRange = 100f;
    public float goodRange = 150f;
    public float missRange = 200f;

    private int combo = 0;
    private int maxCombo = 0;
    private int totalJudged = 0;
    private int perfect = 0;
    private int great = 0;
    private int good = 0;
    private string recentResult = "";
    private string timingOffsetText = "";

    public float bounceHeight = 0.015f;
    public Transform judge;

    private List<GameObject> activeNotes = new List<GameObject>();

    void Update()
    {   //subject.cs
        float sine = Mathf.Sin(Time.time * (Parsing.bpm / 60f) * Mathf.PI * 2);
        float y = 2 + sine * bounceHeight;
        Vector3 pos = judge.position;
        pos.y = y;

        float currentTime = AudioManager.Instance.CurrentTimeMs;
        activeNotes = noteManager.GetActiveNotes();
        if (activeNotes == null || activeNotes.Count == 0) return;

        GameObject targetNote = null;
        float minTime = float.MaxValue;

        foreach (var note in activeNotes)
        {
            if (note.TryGetComponent<chartdata>(out var obj) && obj.noteData != null)
            {
                if (obj.noteData.time < minTime)
                {
                    minTime = obj.noteData.time;
                    targetNote = note;
                }
            }
        }

        if (targetNote == null) return;

        var data = targetNote.GetComponent<chartdata>().noteData;
        float currentJudgeTime = currentTime - offset;
        float rawDelta = currentJudgeTime - data.time;
        float absDelta = Mathf.Abs(rawDelta);

        // Miss 자동 처리 (시간 초과)
        if (currentJudgeTime > data.time + missRange)
        {
            RegisterHit(targetNote, "Fail");
            timingOffsetText = "+Miss";
            return;
        }

        // 판정 처리 (입력 + 충돌)
        if ((Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X)) &&
            IsOverlapping(subjectObject, targetNote))
        {
            if (absDelta <= perfectRange)
            {
                RegisterHit(targetNote, "Perfect");
                SetTimingOffsetText(rawDelta);
                perfect++;
            }
            else if (absDelta <= greatRange)
            {
                RegisterHit(targetNote, "Great");
                SetTimingOffsetText(rawDelta);
                great++;
            }
            else if (absDelta <= goodRange)
            {
                RegisterHit(targetNote, "Good");
                SetTimingOffsetText(rawDelta);
                good++;
            }
            else if (absDelta <= missRange)
            {
                RegisterHit(targetNote, "Miss");
                SetTimingOffsetText(rawDelta);
            }
            else
            {
                RegisterHit(targetNote, "Fail");
                SetTimingOffsetText(rawDelta);
            }
        }
    }

    private void RegisterHit(GameObject note, string result)
    {
        noteManager.RemoveNote(note);

        if (result == "Miss" || result == "Fail") combo = 0;
        else
        {
            combo++;
            if (combo > maxCombo) maxCombo = combo;
        }

        recentResult = result;
        totalJudged++;
    }

    private float GetAccuracy()
    {
        if (totalJudged == 0) return 0f;
        float score = perfect * 1.0f + great * 0.7f + good * 0.4f;
        return score / totalJudged * 100f;
    }

    private bool IsOverlapping(GameObject a, GameObject b)
    {
        if (a == null || b == null) return false;
        Collider2D colA = a.GetComponent<Collider2D>();
        Collider2D colB = b.GetComponent<Collider2D>();
        return colA != null && colB != null && colA.bounds.Intersects(colB.bounds);
    }

    void SetTimingOffsetText(float delta)
    {
        string direction;
        if (delta < -10f) direction = "Fast";
        else if (delta > 10f) direction = "Slow";
        else direction = "Just";

        timingOffsetText = $"{(delta >= 0 ? "+" : "")}{delta:F0}ms ({direction})";
    }

    void OnGUI()
    {
        GUIStyle leftStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            normal = { textColor = Color.white },
            alignment = TextAnchor.UpperLeft
        };

        // 왼쪽 상단 정보
        GUI.BeginGroup(new Rect(20, 20, 400, 150));
        GUI.Label(new Rect(0, 0, 400, 30), $"Combo: {combo}", leftStyle);
        GUI.Label(new Rect(0, 30, 400, 30), $"MaxCombo: {maxCombo}", leftStyle);
        GUI.Label(new Rect(0, 60, 400, 30), $"Accuracy: {GetAccuracy():F1}%", leftStyle);
        GUI.Label(new Rect(0, 90, 400, 30), $"Result: {recentResult}", leftStyle);
        GUI.Label(new Rect(0, 120, 400, 30), $"Timing: {timingOffsetText}", leftStyle);
        GUI.EndGroup();

        GUIStyle rightStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            normal = { textColor = Color.cyan },
            alignment = TextAnchor.UpperRight
        };

        // 오른쪽 상단 정보
        GUI.BeginGroup(new Rect(Screen.width - 320, 20, 300, 100));

        float currentTime = AudioManager.Instance.CurrentTimeMs;
        float adjustedTime = currentTime - offset;
        GUI.Label(new Rect(0, 0, 300, 30), $"C.time - offset: {adjustedTime:F1} ms", rightStyle);

        GameObject targetNote = null;
        float minTime = float.MaxValue;

        List<GameObject> currentNotes = noteManager?.GetActiveNotes();
        if (currentNotes != null && currentNotes.Count > 0)
        {
            foreach (var note in currentNotes)
            {
                if (note.TryGetComponent<chartdata>(out var obj) && obj.noteData != null)
                {
                    if (obj.noteData.time < minTime)
                    {
                        minTime = obj.noteData.time;
                        targetNote = note;
                    }
                }
            }

            if (targetNote != null && targetNote.TryGetComponent<chartdata>(out var targetObj))
            {
                GUI.Label(new Rect(0, 30, 300, 30), $"noteTime: {targetObj.noteData.time:F1} ms", rightStyle);
            }
            else
            {
                GUI.Label(new Rect(0, 30, 300, 30), "noteTime: (null)", rightStyle);
            }
        }
        else
        {
            GUI.Label(new Rect(0, 30, 300, 30), "노트 없음", rightStyle);
        }

        GUI.EndGroup();
    }
}
