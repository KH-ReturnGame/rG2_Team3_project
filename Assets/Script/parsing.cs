using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;

public class Parsing
{
    public static float bpm;
    public float offset = 0f;
    public List<NoteData> notes = new List<NoteData>();
    public List<BpmChange> bpmChanges = new List<BpmChange>();

    public void Parse(ZipArchive zip, string difficulty = "normal")
    {
        
        // 필수 파일 검사
        string[] requiredFiles = new[]
        {
            "image.png","music.mp3" ,"metadata.json", $"{difficulty}.csv"
        };

        foreach (string file in requiredFiles)
        {
            if (zip.GetEntry(file) == null)
            {
                Debug.LogError($".swm 파일에 {file} 누락됨.");
                return;
            }
        }

        // metadata 로드
        var metaEntry = zip.GetEntry("metadata.json");
        using (var reader = new StreamReader(metaEntry.Open()))
        {
            string metaText = reader.ReadToEnd();
            Metadata meta = JsonConvert.DeserializeObject<Metadata>(metaText);
            bpm = meta.bpm;
            offset = meta.offset;
        }

        // csv 노트 파싱
        var csvEntry = zip.GetEntry($"{difficulty}.csv");
        using (var reader = new StreamReader(csvEntry.Open()))
        {
            string header = reader.ReadLine(); // skip header
            
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                Debug.Log($"[Parsing] 라인: {line}");
                if (string.IsNullOrWhiteSpace(line)) continue;

                var tokens = line.Split(',');
                Debug.Log($"→ time: {tokens[0]}");
                if (tokens.Length < 5)
                {
                    Debug.LogWarning($"잘못된 줄: \"{line}\"");
                    continue;
                }

                try
                {
                    float time = float.Parse(tokens[0].Trim());
                    int type = int.Parse(tokens[1].Trim());
                    float pos = float.Parse(tokens[2].Trim());
                    float size = float.Parse(tokens[3].Trim());
                    int bend = int.Parse(tokens[4].Trim());
                    float? bpmChange = null;

                    if (tokens.Length >= 6 && float.TryParse(tokens[5].Trim(), out float parsedBpm))
                        bpmChange = parsedBpm;

                    var point = new NotePathPoint
                    {
                        t = 0f,
                        pos = pos,
                        bend = bend
                    };

                    if (bpmChange.HasValue)
                        bpmChanges.Add(new BpmChange { time = time, bpm = bpmChange.Value });

                    var existing = notes.Find(n => Mathf.Approximately(n.time, time));
                    if (existing != null && type == 1)
                    {
                        existing.path.Add(point);
                    }
                    else
                    {
                        notes.Add(new NoteData
                        {
                            time = time,
                            type = (type == 0 ? "tap" : "hold"),
                            path = new List<NotePathPoint> { point },
                            size = size,
                            bpmChange = bpmChange
                        });
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"파싱 실패: \"{line}\" → {ex.Message}");
                    continue;
                }
            }
        }
    }

    public float GetEffectiveBpm(float time)
    {
        float currentBpm = bpm;
        foreach (var change in bpmChanges)
        {
            if (change.time > time) break;
            currentBpm = change.bpm;
        }
        return currentBpm;
    }

    public class BpmChange
    {
        public float time;
        public float bpm;
    }

    public class Metadata
    {
        public float bpm;
        public float offset;
    }
}
