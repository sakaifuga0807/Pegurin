using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//BGMの種類。ステージ1~3は同じBGMを続けて流す。
public enum BgmCue { Title,Battle,Boss}

//Bgmの一行ぶん。
[System.Serializable]
public class BgmEntry
{
    public BgmCue cue;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 0.6f;
}

//Bgmを鳴らす。RunManagerのイベントを受けて場面によって曲を切り替える。
public class BgmPlayer:MonoBehaviour
{
    [SerializeField] private RunManager runManager;

    [Header("Bgmの割り当て")]
    [SerializeField] private List<BgmEntry> entries = new List<BgmEntry>();

    [Header("再生設定")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;
    [Tooltip("曲の切り替えに書ける数秒")]
    [SerializeField] private float fadeSeconds = 1f;
    [Tooltip("ボス戦のBgm")]
    [SerializeField] private float bossDelaySeconds = 6.5f;

    [Header("確認用")]
    [SerializeField] private bool logPlays = false;

    private AudioSource source;
    private BgmCue? currentCue;
    private Coroutine routine;

    private void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.volume = 0f;
    }

    private void Start()
    {
        if(runManager==null)
        {
            Debug.LogWarning("BgmPlayerのRunManagerが未設定です");
            return;
        }

        runManager.OnStageStarted += HandleStageStarted;
        runManager.OnRunCleared += HandleStop;
        runManager.OnRunGameOver += HandleStop;
        runManager.OnReturnedToTitle += HandleTitle;

        //RunManagerのStartが先に動いた場合の取りこぼしを防ぐ。
        ApplyCurrentState();
    }

    private void OnDestroy()
    {
        if(runManager==null)
        {
            return;
        }

        runManager.OnStageStarted -= HandleStageStarted;
        runManager.OnRunCleared -= HandleStop;
        runManager.OnRunGameOver -= HandleStop;
        runManager.OnReturnedToTitle -= HandleTitle;
    }

    private void ApplyCurrentState()
    {
        switch (runManager.Phase)
        {
            case RunPhase.Title:
                Play(BgmCue.Title);
                break;
            case RunPhase.Battle:
            case RunPhase.Reward:
            case RunPhase.Map:
                HandleStageStarted(runManager.StageIndex, runManager.CurrentStage);
                break;
        }
    }

    private void HandleStageStarted(int stageIndex,StageConfig stage)
    {
        bool boss = stage != null && stage.isBoss;
        Play(boss ? BgmCue.Boss : BgmCue.Battle, boss ? bossDelaySeconds : 0f);
    }

    private void HandleTitle() { Play(BgmCue.Title); }
    private void HandleStop() { Stop(); }

    //曲を切り替える。今と同じなら何もしない。
    public void Play(BgmCue cue,float delay=0f)
    {
        if(currentCue==cue)
        {
            return;
        }

        currentCue = cue;

        BgmEntry entry = entries.Find(e => e.cue == cue);
        if(logPlays)
        {
            Debug.Log($"[BGM]{cue}{(entry != null && entry.clip != null ? entry.clip.name : "(曲なし)")}");
        }
        StartFade(entry != null ? entry.clip : null, entry != null ? entry.volume : 0f, delay);
    }

    //BGMを止める(フェードアウト)
    public void Stop()
    {
        currentCue = null;
        if(logPlays)
        {
            Debug.Log("[BGM]停止");
        }
        StartFade(null, 0f, 0f);
    }

    private void StartFade(AudioClip clip, float volume, float delay)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = StartCoroutine(FadeRoutine(clip, volume * masterVolume, delay));
        }
    }

    private IEnumerator FadeRoutine(AudioClip clip,float targetVolume,float delay)
    {
        //今の曲をフェードアウトする。
        if(source.isPlaying)
        {
            yield return FadeTo(0f);
            source.Stop();
        }

        //ジングルなどのために遅らせる。
        float wait = delay - fadeSeconds;
        if (wait > 0f) yield return new WaitForSeconds(wait);

        if (clip == null) yield break;

        source.clip = clip;
        source.volume = 0f;
        source.Play();
        yield return FadeTo(targetVolume);
    }
    
    private IEnumerator FadeTo(float target)
    {
        float start = source.volume;
        float time = 0f;
        float duration = Mathf.Max(0f, fadeSeconds);
        while(time<duration)
        {
            time += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(start, target, Mathf.Clamp01(time / duration));
            yield return null;
        }
        source.volume = target;
    }
}