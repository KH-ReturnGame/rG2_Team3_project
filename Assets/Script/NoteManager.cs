using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;

public class NoteManager : MonoBehaviour
{
    public string chartFileName = "test.chart";
    public GameObject NotePrefab;
    public Transform spawnPoint; // 스폰 기준 위치

    public float v_offset = 0f;
    public float despawnY = -5f;

    private Parsing map;
    private int noteIndex = 0;
    private Vector3 noteBounds;
    private List<GameObject> activeNotes = new List<GameObject>();
    private bool initialized = false;

    public AudioClip hitSound;
    private AudioSource audioSource;
    private List<NoteData> hitsoundNotes = new List<NoteData>();

    private IEnumerator Start()
    {
        string chartPath = Path.Combine(Application.streamingAssetsPath, chartFileName);
        yield return StartCoroutine(AudioManager.Instance.LoadAndPlayFromChart(chartPath));

        map = AudioManager.Instance.ParsedMap;

        if (map == null || map.notes == null)
        {
            Debug.LogError("노트 데이터 로딩 실패");
            yield break;
        }

        hitsoundNotes = new List<NoteData>(map.notes);

        noteBounds = NotePrefab.TryGetComponent<Renderer>(out var renderer)
            ? renderer.bounds.size
            : Vector3.one;

        audioSource = gameObject.AddComponent<AudioSource>();
        initialized = true;
    }

    private void Update()
    {
        if (!initialized || !AudioManager.Instance.IsPlaying || map == null || map.notes == null)
            return;

        HandleSpeedAdjustment();

        float currentTimeMs = AudioManager.Instance.CurrentTimeMs;
        float spawnAheadTimeMs = (100f / PlayerData.noteSpeed) * 1000f;

        // 노트 스폰
        while (noteIndex < map.notes.Count && map.notes[noteIndex].time <= currentTimeMs + spawnAheadTimeMs)
        {
            var noteData = map.notes[noteIndex];
            float timeUntilHit = noteData.time - currentTimeMs;
            float yOffset = (timeUntilHit / 1000f) * PlayerData.noteSpeed;

            Vector3 spawnPos = spawnPoint.position +
                new Vector3(noteData.lane * noteBounds.x * 0.5f, yOffset, 0);

            GameObject noteObj = Instantiate(NotePrefab, spawnPos, Quaternion.identity);

            // 롱노트이면 길이 조절
            if (noteData.type == 1 && noteData.length > 0f)
            {
                float visualLength = (noteData.length / 1000f) * PlayerData.noteSpeed;
                noteObj.transform.localScale = new Vector3(noteBounds.x, visualLength, noteBounds.z);
            }
            else
            {
                noteObj.transform.localScale = noteBounds;
            }

            if (noteObj.TryGetComponent<chartdata>(out var obj))
            {
                obj.noteData = noteData;
                obj.targetTime = noteData.time;
            }

            activeNotes.Add(noteObj);
            noteIndex++;
        }

        // 노트 이동 및 삭제 처리
        float adjustedTime = currentTimeMs - v_offset;
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            var note = activeNotes[i];
            if (!note.TryGetComponent<chartdata>(out var obj)) continue;

            float timeUntilHit = obj.targetTime - currentTimeMs;
            float yOffset = (timeUntilHit / 1000f) * PlayerData.noteSpeed;

            Vector3 pos = note.transform.position;
            note.transform.position = new Vector3(pos.x, yOffset + spawnPoint.position.y, pos.z);

            if (note.transform.position.y < despawnY)
            {
                Destroy(note);
                activeNotes.RemoveAt(i);
            }
        }

        // 히트사운드
        for (int i = hitsoundNotes.Count - 1; i >= 0; i--)
        {
            float delta = hitsoundNotes[i].time - adjustedTime;
            if (delta <= 0)
            {
                if (hitSound != null)
                    audioSource.PlayOneShot(hitSound);

                hitsoundNotes.RemoveAt(i);
            }
        }
    }

    private void HandleSpeedAdjustment()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            PlayerData.noteSpeed = Mathf.Max(1f, PlayerData.noteSpeed - 1f);

        if (Input.GetKeyDown(KeyCode.F2))
            PlayerData.noteSpeed += 1f;
    }

    public List<GameObject> GetActiveNotes() => activeNotes;

    public void RemoveNote(GameObject note)
    {
        if (activeNotes.Contains(note))
        {
            activeNotes.Remove(note);
            Destroy(note);
        }
    }
}
