using UnityEngine;
using System.Collections.Generic;

public class JudgeManager : MonoBehaviour
{
    [SerializeField] private NoteManager noteManager;

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

    public KeyCode[] laneKeys = new KeyCode[4] { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K };
    public GameObject[] laneHighlights;

    private List<GameObject> activeNotes = new List<GameObject>();

    void Update()
    {
        float currentTime = AudioManager.Instance.CurrentTimeMs;
        activeNotes = noteManager.GetActiveNotes();
        if (activeNotes == null || activeNotes.Count == 0) return;

        float currentJudgeTime = currentTime - offset;
        for (int i = 0; i < laneKeys.Length; i++)
        {
            if (Input.GetKeyDown(laneKeys[i]))
            {
                if (laneHighlights[i] != null)
                    laneHighlights[i].SetActive(true); // 불 켬
            }

            if (Input.GetKeyUp(laneKeys[i]))
            {
                if (laneHighlights[i] != null)
                    laneHighlights[i].SetActive(false); // 불 끔
            }
        }

        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            var note = activeNotes[i];

            if (!note.TryGetComponent<chartdata>(out var obj) || obj.noteData == null)
                continue;

            var data = obj.noteData;
            float rawDelta = currentJudgeTime - data.time;
            float absDelta = Mathf.Abs(rawDelta);

            // Miss 자동 처리
            if (currentJudgeTime > data.time + missRange)
            {
                RegisterHit(note, "Fail");
                timingOffsetText = "+Miss";
                continue;
            }

            // 레인에 맞는 키가 눌렸는지 확인
            if (data.lane >= 0 && data.lane < laneKeys.Length)
            {
                if (Input.GetKeyDown(laneKeys[data.lane]))
                {
                    if (absDelta <= perfectRange)
                    {
                        RegisterHit(note, "Perfect");
                        SetTimingOffsetText(rawDelta);
                        perfect++;
                    }
                    else if (absDelta <= greatRange)
                    {
                        RegisterHit(note, "Great");
                        SetTimingOffsetText(rawDelta);
                        great++;
                    }
                    else if (absDelta <= goodRange)
                    {
                        RegisterHit(note, "Good");
                        SetTimingOffsetText(rawDelta);
                        good++;
                    }
                    else if (absDelta <= missRange)
                    {
                        RegisterHit(note, "Miss");
                        SetTimingOffsetText(rawDelta);
                    }
                    else
                    {
                        RegisterHit(note, "Fail");
                        SetTimingOffsetText(rawDelta);
                    }
                }
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

    void SetTimingOffsetText(float delta)
    {
        string direction;
        if (delta < -10f) direction = "Fast";
        else if (delta > 10f) direction = "Slow";
        else direction = "Just";

        timingOffsetText = $"{(delta >= 0 ? "+" : "")}{delta:F0}ms ({direction})";
    }

    //디버그용
    void OnGUI()
    {
        GUIStyle leftStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            normal = { textColor = Color.white },
            alignment = TextAnchor.UpperLeft
        };

        GUI.BeginGroup(new Rect(20, 20, 400, 150));
        GUI.Label(new Rect(0, 0, 400, 30), $"Combo: {combo}", leftStyle);
        GUI.Label(new Rect(0, 30, 400, 30), $"MaxCombo: {maxCombo}", leftStyle);
        GUI.Label(new Rect(0, 60, 400, 30), $"Accuracy: {GetAccuracy():F1}%", leftStyle);
        GUI.Label(new Rect(0, 90, 400, 30), $"Result: {recentResult}", leftStyle);
        GUI.Label(new Rect(0, 120, 400, 30), $"Timing: {timingOffsetText}", leftStyle);
        GUI.EndGroup();
    }
}