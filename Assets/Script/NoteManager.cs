using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;

public class NoteManager : MonoBehaviour
{
    public string swmFileName = "test.swm";
    public GameObject NotePrefab;
    public float spawnX = 0f;
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


    private IEnumerator Init(string swmPath)
    {
        yield return StartCoroutine(AudioManager.Instance.LoadAndPlayFromSwm(swmPath));

        map = AudioManager.Instance.ParsedMap;

        if (map == null || map.notes == null)
        {
            Debug.LogError("노트 데이터 로딩 실패");
            yield break;
        }

        hitsoundNotes = new List<NoteData>(map.notes);

        if (NotePrefab.TryGetComponent<Renderer>(out var renderer))
            noteBounds = renderer.bounds.size;
        else
            noteBounds = Vector3.one;

        initialized = true;
    }

    private void Start()
    {
        string swmPath = Path.Combine(Application.streamingAssetsPath, swmFileName);
        StartCoroutine(Init(swmPath));

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        if (!initialized || !AudioManager.Instance.IsPlaying || map == null || map.notes == null)
            return;

        float spawnAheadTimeMs;
        float defaultSpawnDistance = 100f;
        spawnAheadTimeMs = (defaultSpawnDistance / PlayerData.noteSpeed) * 1000f;

        //노트 간격 계산
        if (Input.GetKeyDown(KeyCode.F1))
        {
            PlayerData.noteSpeed = Mathf.Max(1f, PlayerData.noteSpeed - 1f);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            PlayerData.noteSpeed += 1f;
        }

        float currentTimeMs = AudioManager.Instance.CurrentTimeMs;

        // 노트 미리 생성
        while (noteIndex < map.notes.Count &&
               map.notes[noteIndex].time <= currentTimeMs + spawnAheadTimeMs)
        {
            var noteData = map.notes[noteIndex];
            if (noteData.path == null || noteData.path.Count == 0)
            {
                noteIndex++;
                continue;
            }

            var p = noteData.path[0];
            float timeUntilHit = noteData.time - currentTimeMs;
            float yOffset = (timeUntilHit / 1000f) * PlayerData.noteSpeed;

            Vector3 spawnPos = new Vector3(
                spawnX + p.pos * noteBounds.x * 0.5f,
                yOffset,
                0
            );

            GameObject noteObj = Instantiate(NotePrefab, spawnPos, Quaternion.identity);
            noteObj.transform.localScale = noteBounds * noteData.size;

            if (noteObj.TryGetComponent<chartdata>(out var obj))
            {
                obj.noteData = noteData;
                obj.targetTime = noteData.time;
            }

            activeNotes.Add(noteObj);
            noteIndex++;
        }

        // 노트 위치 실시간 조정 및 despawn 처리
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            GameObject note = activeNotes[i];

            if (note.TryGetComponent<chartdata>(out var obj))
            {
                float timeUntilHit = obj.targetTime - currentTimeMs;
                float yOffset = (timeUntilHit / 1000f) * PlayerData.noteSpeed;

                Vector3 pos = note.transform.position;
                note.transform.position = new Vector3(pos.x, yOffset, pos.z);
            }

            if (note.transform.position.y < despawnY)
            {
                Destroy(note);
                activeNotes.RemoveAt(i);
            }
        }
        float adjustedTime = currentTimeMs - v_offset;

        for (int i = hitsoundNotes.Count - 1; i >= 0; i--)
        {
            float delta = hitsoundNotes[i].time - adjustedTime;

            if (delta <= 0)
            {
                if (hitSound != null)
                    audioSource.PlayOneShot(hitSound);

                hitsoundNotes.RemoveAt(i); // 중복 방지
            }
        }
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
