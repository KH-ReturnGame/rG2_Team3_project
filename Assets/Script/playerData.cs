using UnityEngine;

public static class PlayerData
{
    public static float noteSpeed
    {
        get => PlayerPrefs.GetFloat("NoteSpeed", 5f);
        set
        {
            PlayerPrefs.SetFloat("NoteSpeed", value);
            PlayerPrefs.Save();
        }
    }

    public static void SaveResult(string songId, int score, int combo, float accuracy,
        int perfect, int great, int good, int miss, int fail)
    {
        // 더 클 때만 갱신
        int prevScore = PlayerPrefs.GetInt($"HighScore_{songId}", 0);
        if (score > prevScore)
        {
            PlayerPrefs.SetInt($"HighScore_{songId}", score);
            PlayerPrefs.SetInt($"MaxCombo_{songId}", combo);
            PlayerPrefs.SetFloat($"Accuracy_{songId}", accuracy);
            PlayerPrefs.SetInt($"Perfect_{songId}", perfect);
            PlayerPrefs.SetInt($"Great_{songId}", great);
            PlayerPrefs.SetInt($"Good_{songId}", good);
            PlayerPrefs.SetInt($"Miss_{songId}", miss);
            PlayerPrefs.SetInt($"Fail_{songId}", fail);
        }
            
        PlayerPrefs.Save();
    }
}
