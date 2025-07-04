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

    // 현재 BPM/박자 상태
    private float currentBpm;
    private int numerator = 4;
    private int denominator = 4;

    private const float ticksPerBeat = 16f;

    public void Parse(ZipArchive zip, string difficulty = "normal")
    {
        string[] requiredFiles = new[] { "image.png", "music.mp3", "metadata.json", $"{difficulty}.csv" };
        foreach (string file in requiredFiles)
        {
            if (zip.GetEntry(file) == null)
            {
                Debug.LogError($".chart 파일에 {file} 누락됨.");
                return;
            }
        }

        var metaEntry = zip.GetEntry("metadata.json");
        using (var reader = new StreamReader(metaEntry.Open()))
        {
            string metaText = reader.ReadToEnd();
            Metadata meta = JsonConvert.DeserializeObject<Metadata>(metaText);
            bpm = meta.bpm;
            currentBpm = bpm;
            offset = meta.offset;
        }

        var csvEntry = zip.GetEntry($"{difficulty}.csv");
        using (var reader = new StreamReader(csvEntry.Open()))
        {
            _ = reader.ReadLine(); // 헤더 스킵
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                Debug.Log($"[Parsing] 라인: {line}");
                var tokens = line.Split(',');

                try
                {
                    int type = int.Parse(tokens[0].Trim());

                    if (type == 2)
                    {
                        // BPM/박자 변경
                        if (tokens.Length >= 2 && float.TryParse(tokens[1].Trim(), out float parsedBpm))
                            currentBpm = parsedBpm;

                        if (tokens.Length >= 4)
                        {
                            if (int.TryParse(tokens[2].Trim(), out int num))
                                numerator = num;
                            if (int.TryParse(tokens[3].Trim(), out int den))
                                denominator = den;
                        }

                        Debug.Log($"[BPM 변경] bpm = {currentBpm}, 박자 = {numerator}/{denominator}");
                        continue;
                    }

                    if (tokens.Length < 4)
                    {
                        Debug.LogWarning($"[Parsing] 잘못된 줄: \"{line}\"");
                        continue;
                    }

                    int measure = int.Parse(tokens[1].Trim());
                    int position = int.Parse(tokens[2].Trim());
                    int lane = int.Parse(tokens[3].Trim());

                    // tick to ms
                    float ticksPerMeasure = numerator * ticksPerBeat;
                    float absoluteTick = measure * ticksPerMeasure + position;
                    float time = (60000f / currentBpm) * (absoluteTick / ticksPerBeat);
                    float speed = (currentBpm * 4f / denominator) / bpm;

                    // 노트 생성
                    if (type == 0)
                    {
                        notes.Add(new NoteData
                        {
                            time = time,
                            type = 0,
                            lane = lane,
                            speed = speed
                        });
                    }
                    else if (type == 1)
                    {
                        if (tokens.Length < 6)
                        {
                            Debug.LogWarning($"롱노트 줄 형식 오류: {line}");
                            continue;
                        }

                        int sMeasure = int.Parse(tokens[1].Trim());
                        int sPos = int.Parse(tokens[2].Trim());
                        int eMeasure = int.Parse(tokens[3].Trim());
                        int ePos = int.Parse(tokens[4].Trim());
                        lane = int.Parse(tokens[5].Trim());

                        ticksPerMeasure = numerator * ticksPerBeat;
                        float sTick = sMeasure * ticksPerMeasure + sPos;
                        float eTick = eMeasure * ticksPerMeasure + ePos;

                        float sTime = (60000f / currentBpm) * (sTick / ticksPerBeat);
                        float eTime = (60000f / currentBpm) * (eTick / ticksPerBeat);
                        speed = (currentBpm * 4f / denominator) / bpm;

                        notes.Add(new NoteData
                        {
                            time = sTime,
                            type = 1,
                            lane = lane,
                            speed = speed,
                            length = eTime - sTime
                        });
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[Parsing] 파싱 실패: \"{line}\" → {ex.Message}");
                }
            }
        }
    }

    public class Metadata
    {
        public float bpm;
        public float offset;
    }
}
