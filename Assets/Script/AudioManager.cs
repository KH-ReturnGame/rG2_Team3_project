//아마 폐기할듯
using System.IO;
using System.IO.Compression;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    private AudioSource musicSource;
    public Parsing ParsedMap { get; private set; }
    public float CurrentTimeMs => musicSource != null ? musicSource.time * 1000f : 0f;
    public bool IsPlaying => musicSource != null && musicSource.isPlaying;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = 1f;
        musicSource.spatialBlend = 0f;
    }

    public IEnumerator LoadAndPlayFromSwm(string swmPath, string difficulty = "normal")
    {
        using (FileStream fs = new FileStream(swmPath, FileMode.Open))
        using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read))
        {
            var musicEntry = zip.GetEntry("music.mp3");
            if (musicEntry == null)
            {
                Debug.LogError("music.mp3 누락됨");
                yield break;
            }

            string tempPath = Path.Combine(Application.persistentDataPath, "temp_music.mp3");
            using (var stream = musicEntry.Open())
            using (var file = File.Create(tempPath))
            {
                stream.CopyTo(file);
            }

            string url = "file://" + tempPath.Replace("\\", "/");
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("오디오 로드 실패: " + www.error);
                    yield break;
                }

                var clip = DownloadHandlerAudioClip.GetContent(www);
                if (clip == null || clip.length <= 0f)
                {
                    Debug.LogError("오디오 클립이 비었거나 유효하지 않음");
                    yield break;
                }

                musicSource.clip = clip;
                musicSource.Play();

                ParsedMap = new Parsing();
                ParsedMap.Parse(zip, difficulty);

                Debug.Log("오디오 및 차트 로드 완료");
            }
        }
    }
}
