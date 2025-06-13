using System;
using System.Collections.Generic;

[Serializable]
public class BMSData
{
    public string title = "";
    public string artist = "";
    public string genre = "";
    public float bpm = 120f;
    public int playLevel = 1;
    public int rank = 2; // 판정 난이도
    public float total = 100f; // 게이지 증가량

    // WAV 파일 정의 (키: 16진수 ID, 값: 파일명)
    public Dictionary<string, string> wavFiles = new Dictionary<string, string>();

    // BPM 변경 정의
    public Dictionary<string, float> bpmChanges = new Dictionary<string, float>();

    // 노트 데이터 (마디번호, 채널, 노트배열)
    public List<BMSNote> notes = new List<BMSNote>();

    // 마디별 길이 배율 (기본 1.0)
    public Dictionary<int, float> measureLengths = new Dictionary<int, float>();
}

[Serializable]
public class BMSNote
{
    public int measure;      // 마디 번호
    public int channel;      // 채널 (레인/트랙)
    public float position;   // 마디 내 위치 (0~1)
    public string soundId;   // 사운드 ID (16진수)
    public float timing;     // 절대 시간 (초)

    public BMSNote(int measure, int channel, float position, string soundId)
    {
        this.measure = measure;
        this.channel = channel;
        this.position = position;
        this.soundId = soundId;
    }
}