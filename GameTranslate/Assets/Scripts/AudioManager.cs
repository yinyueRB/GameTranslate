using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("音轨分配 (拖入对应的AudioSource)")]
    public AudioSource ambientSource;   // 底噪声轨道
    public AudioSource bgmSource;       // 音乐轨道
    public AudioSource heartbeatSource; // 心跳声轨道

    [Header("音频文件 (拖入找到的音频)")]
    public AudioClip deepSeaAmbient;    // 深海底噪
    public AudioClip bgmRegion123;      // 前期的BGM
    public AudioClip bgmRegion4;        // 结局的BGM
    public AudioClip heartbeatClip;     // 心跳声

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 游戏开始时，播放底噪和前期的BGM
        PlayLoopingAudio(ambientSource, deepSeaAmbient);
        PlayLoopingAudio(bgmSource, bgmRegion123);
    }

    // 封装一个播放循环音频的方法
    private void PlayLoopingAudio(AudioSource source, AudioClip clip)
    {
        if (clip != null)
        {
            source.clip = clip;
            source.loop = true; // 循环播放
            source.Play();
        }
    }

    // --- 区域3触发：开始播放心跳 ---
    public void StartHeartbeat()
    {
        PlayLoopingAudio(heartbeatSource, heartbeatClip);
    }

    // --- 区域4触发：背景音乐淡出淡入 ---
    public void CrossfadeBGM(float fadeTime)
    {
        StartCoroutine(CrossfadeRoutine(fadeTime));
    }

    private IEnumerator CrossfadeRoutine(float fadeTime)
    {
        float startVolume = bgmSource.volume; // 记录当前音量

        // 1. 旧BGM渐渐淡出到0
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0, t / fadeTime);
            yield return null;
        }
        bgmSource.Stop();

        // 2. 换成新的BGM，渐渐淡入
        bgmSource.clip = bgmRegion4;
        bgmSource.Play();
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(0, startVolume, t / fadeTime);
            yield return null;
        }
        bgmSource.volume = startVolume; // 确保音量完全恢复
    }

    // --- 结局触发：改变心跳音高(变慢变沉) 和 停止所有声音 ---
    public void SetHeartbeatPitch(float pitch)
    {
        heartbeatSource.pitch = pitch;
    }

    public void StopAllAudioForEnding()
    {
        bgmSource.Stop();
        ambientSource.Stop();
        heartbeatSource.Stop();
    }
}