using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BMSParser : MonoBehaviour
{
    [Header("BMS File Settings")]
    public string bmsFilePath = "Assets/StreamingAssets/song.bms";
    
    [Header("Debug")]
    public BMSData currentBMSData;
    
    private void Start()
    {
        // 예시: 시작 시 BMS 파일 로드
        LoadBMSFile(bmsFilePath);
    }
    
    /// <summary>
    /// BMS 파일을 로드하고 파싱합니다
    /// </summary>
    public BMSData LoadBMSFile(string filePath)
    {
        try
        {
            string fullPath;
            
            // StreamingAssets 폴더 경로 처리
            if (filePath.StartsWith("Assets/StreamingAssets/"))
            {
                string relativePath = filePath.Replace("Assets/StreamingAssets/", "");
                fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
            }
            else
            {
                fullPath = filePath;
            }
            
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"BMS 파일을 찾을 수 없습니다: {fullPath}");
                return null;
            }
            
            string[] lines = File.ReadAllLines(fullPath);
            BMSData bmsData = ParseBMS(lines);
            
            // 노트 타이밍 계산
            CalculateNoteTiming(bmsData);
            
            currentBMSData = bmsData;
            Debug.Log($"BMS 파일 로드 완료: {bmsData.title} by {bmsData.artist}");
            Debug.Log($"총 노트 수: {bmsData.notes.Count}");
            
            return bmsData;
        }
        catch (Exception e)
        {
            Debug.LogError($"BMS 파일 로드 중 오류 발생: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// BMS 텍스트 데이터를 파싱합니다
    /// </summary>
    private BMSData ParseBMS(string[] lines)
    {
        BMSData data = new BMSData();
        
        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            
            // 빈 줄이나 주석 무시
            if (string.IsNullOrEmpty(trimmedLine) || !trimmedLine.StartsWith("#"))
                continue;
            
            // 헤더 정보 파싱
            if (ParseHeader(trimmedLine, data))
                continue;
            
            // 노트 데이터 파싱
            ParseNoteData(trimmedLine, data);
        }
        
        return data;
    }
    
    /// <summary>
    /// 헤더 정보를 파싱합니다
    /// </summary>
    private bool ParseHeader(string line, BMSData data)
    {
        if (line.StartsWith("#TITLE "))
        {
            data.title = line.Substring(7);
            return true;
        }
        else if (line.StartsWith("#ARTIST "))
        {
            data.artist = line.Substring(8);
            return true;
        }
        else if (line.StartsWith("#GENRE "))
        {
            data.genre = line.Substring(7);
            return true;
        }
        else if (line.StartsWith("#BPM "))
        {
            if (float.TryParse(line.Substring(5), out float bpm))
                data.bpm = bpm;
            return true;
        }
        else if (line.StartsWith("#PLAYLEVEL "))
        {
            if (int.TryParse(line.Substring(11), out int level))
                data.playLevel = level;
            return true;
        }
        else if (line.StartsWith("#RANK "))
        {
            if (int.TryParse(line.Substring(6), out int rank))
                data.rank = rank;
            return true;
        }
        else if (line.StartsWith("#TOTAL "))
        {
            if (float.TryParse(line.Substring(7), out float total))
                data.total = total;
            return true;
        }
        else if (line.StartsWith("#WAV"))
        {
            // #WAV01 filename.wav 형태
            string[] parts = line.Split(' ', 2);
            if (parts.Length >= 2)
            {
                string wavId = parts[0].Substring(4); // "01" 부분 추출
                data.wavFiles[wavId] = parts[1];
            }
            return true;
        }
        else if (line.StartsWith("#BPM"))
        {
            // #BPM01 150.0 형태 (BPM 변경 정의)
            string[] parts = line.Split(' ', 2);
            if (parts.Length >= 2 && parts[0].Length > 4)
            {
                string bpmId = parts[0].Substring(4);
                if (float.TryParse(parts[1], out float bpm))
                    data.bpmChanges[bpmId] = bpm;
            }
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 노트 데이터를 파싱합니다
    /// </summary>
    private void ParseNoteData(string line, BMSData data)
    {
        // #00111:01020304 형태 파싱
        int colonIndex = line.IndexOf(':');
        if (colonIndex == -1 || line.Length < 6)
            return;
        
        // 마디 번호 추출 (001)
        string measureStr = line.Substring(1, 3);
        if (!int.TryParse(measureStr, out int measure))
            return;
        
        // 채널 번호 추출 (11)
        string channelStr = line.Substring(4, 2);
        if (!int.TryParse(channelStr, out int channel))
            return;
        
        // 마디 길이 정의인 경우 (#00102:1.5 등)
        if (channel == 2)
        {
            string lengthStr = line.Substring(colonIndex + 1);
            if (float.TryParse(lengthStr, out float length))
                data.measureLengths[measure] = length;
            return;
        }
        
        // 노트 데이터 추출
        string noteData = line.Substring(colonIndex + 1);
        
        // 2자리씩 분할하여 노트 생성
        int noteCount = noteData.Length / 2;
        for (int i = 0; i < noteCount; i++)
        {
            string soundId = noteData.Substring(i * 2, 2);
            
            // "00"은 빈 노트이므로 무시
            if (soundId == "00")
                continue;
            
            // 마디 내 위치 계산 (0~1)
            float position = (float)i / noteCount;
            
            BMSNote note = new BMSNote(measure, channel, position, soundId);
            data.notes.Add(note);
        }
    }
    
    /// <summary>
    /// 각 노트의 절대 시간을 계산합니다
    /// </summary>
    private void CalculateNoteTiming(BMSData data)
    {
        float currentTime = 0f;
        float currentBPM = data.bpm;
        
        // 마디별로 정렬
        data.notes.Sort((a, b) => 
        {
            int measureCompare = a.measure.CompareTo(b.measure);
            if (measureCompare != 0) return measureCompare;
            return a.position.CompareTo(b.position);
        });
        
        int currentMeasure = 0;
        
        foreach (var note in data.notes)
        {
            // 새로운 마디로 넘어갔을 때
            while (currentMeasure < note.measure)
            {
                // 마디 길이 (기본 4/4박자 = 4박자)
                float measureLength = data.measureLengths.ContainsKey(currentMeasure) 
                    ? data.measureLengths[currentMeasure] : 1.0f;
                
                // 한 마디의 시간 = (60 / BPM) * 4 * 마디길이배율
                float measureTime = (60f / currentBPM) * 4f * measureLength;
                currentTime += measureTime;
                currentMeasure++;
            }
            
            // 마디 내 위치에 따른 시간 계산
            float measureLength2 = data.measureLengths.ContainsKey(note.measure) 
                ? data.measureLengths[note.measure] : 1.0f;
            float measureTime2 = (60f / currentBPM) * 4f * measureLength2;
            
            note.timing = currentTime + (measureTime2 * note.position);
        }
    }
    
    /// <summary>
    /// 특정 시간대의 노트들을 가져옵니다
    /// </summary>
    public List<BMSNote> GetNotesInTimeRange(float startTime, float endTime)
    {
        List<BMSNote> result = new List<BMSNote>();
        
        foreach (var note in currentBMSData.notes)
        {
            if (note.timing >= startTime && note.timing <= endTime)
                result.Add(note);
        }
        
        return result;
    }
    
    /// <summary>
    /// 특정 채널의 노트들만 가져옵니다
    /// </summary>
    public List<BMSNote> GetNotesByChannel(int channel)
    {
        List<BMSNote> result = new List<BMSNote>();
        
        foreach (var note in currentBMSData.notes)
        {
            if (note.channel == channel)
                result.Add(note);
        }
        
        return result;
    }
}