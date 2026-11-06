using System.Collections.Generic;
using UnityEngine;

//BGMの種類。ステージ1〜3は同じ戦闘BGMを続けて流す。
public enum BgmCue { Title, Battle, Boss }

//BGMの一行ぶん。
[System.Serializable]
public class BgmEntry
{
    public BgmCue cue;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 0.6f;
}

//音を鳴らす役。SE・ジングル・BGMの3つの枠を持つ。
//ルールのコードは SE.Play(...) を呼ぶだけ。BGMは RunManager のイベントを受けて、自動で切り替える。
public class AudioDirector : MonoBehaviour
{
    [Header("SEの割り当て")]
    [SerializeField] private SoundBank bank;

    [Header("BGM")]
    [SerializeField] private RunManager runManager;
    [SerializeField] private List<BgmEntry> bgmEntries = new List<BgmEntry>();
    [Tooltip("曲の切り替えにかける秒数（フェードアウト・フェードイン）")]
    [SerializeField] private float fadeSeconds = 1f;
    [Tooltip("ジングルが鳴っている間の、BGMの音量の割合")]
    [Range(0f, 1f)]
    [SerializeField] private float duckLevel = 0.25f;
    [Tooltip("BGMを下げる・戻すのにかける秒数")]
    [SerializeField] private float duckSeconds = 0.3f;

    [Header("音量")]
    [Range(0f, 1f)][SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float seVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float bgmVolume = 1f;

    [Header("再生設定")]
    [Tooltip("SEを同時に鳴らせる数")]
    [SerializeField] private int seVoiceCount = 8;

    [Header("確認用")]
    [SerializeField] private bool logPlays = false;

    private readonly Dictionary<SeCue, SoundEffect> table = new Dictionary<SeCue, SoundEffect>();
    private readonly Dictionary<SeCue, float> lastPlayTime = new Dictionary<SeCue, float>();
    private readonly HashSet<SeCue> warned = new HashSet<SeCue>();

    private AudioSource[] seVoices;
    private int nextVoice;
    private AudioSource jingleSource;
    private float jingleEndTime;

    private AudioSource bgmSource;
    private BgmCue? currentBgm;
    private bool pending;               //曲の入れ替え待ち。
    private AudioClip pendingClip;
    private float pendingVolume;
    private float pendingStartTime;
    private float bgmEntryVolume;       //今の曲の、曲ごとの音量。
    private float bgmFade;              //フェードの現在値（0〜1）。
    private float bgmFadeTarget;
    private float duckMul = 1f;         //ジングル中にBGMを下げる割合。

    private RunManager subscribed;

    // ---- 準備 ----

    private void Awake()
    {
        //SEの表を作る。
        if (bank != null)
        {
            foreach (var entry in bank.Entries)
            {
                if (entry.effect == null) continue;
                if (table.ContainsKey(entry.cue))
                {
                    Debug.LogWarning($"SE {entry.cue} が SoundBank で重複しています。後のものが使われます。");
                }
                table[entry.cue] = entry.effect;
            }
        }
        else
        {
            Debug.LogWarning("AudioDirector の Bank が未設定です。SEは鳴りません。");
        }

        //SE用の枠（順番に使い回す）。
        seVoices = new AudioSource[Mathf.Max(1, seVoiceCount)];
        for (int i = 0; i < seVoices.Length; i++)
        {
            seVoices[i] = CreateSource(false);
        }
        jingleSource = CreateSource(false);   //ジングル用の専用の枠。
        bgmSource = CreateSource(true);       //BGM用の枠。
    }

    private AudioSource CreateSource(bool loop)
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;   //2Dの音にする。
        source.loop = loop;
        return source;
    }

    private void OnEnable() { SE.OnPlay += Play; }
    private void OnDisable() { SE.OnPlay -= Play; }

    private void Start()
    {
        if (FindFirstObjectByType<AudioListener>() == null)
        {
            Debug.LogWarning("シーンに Audio Listener がありません。Main Camera に付いているか確認してください。");
        }
        if (AudioListener.volume <= 0f)
        {
            Debug.LogWarning("AudioListener の全体の音量が0です。");
        }

        if (runManager == null)
        {
            Debug.LogWarning("AudioDirector の Run Manager が未設定です。BGMは自動で切り替わりません。");
            return;
        }

        subscribed = runManager;
        subscribed.OnStageStarted += HandleStageStarted;
        subscribed.OnRunCleared += HandleStop;
        subscribed.OnRunGameOver += HandleStop;
        subscribed.OnReturnedToTitle += HandleTitle;
        ApplyCurrentState();   //RunManager の Start が先に動いた分の取りこぼしを防ぐ。
    }

    private void OnDestroy()
    {
        if (subscribed == null) return;
        subscribed.OnStageStarted -= HandleStageStarted;
        subscribed.OnRunCleared -= HandleStop;
        subscribed.OnRunGameOver -= HandleStop;
        subscribed.OnReturnedToTitle -= HandleTitle;
    }

    // ---- SE・ジングル ----

    //鳴らす。variant があれば、それを優先する（玉ごと・敵ごとの音）。
    private void Play(SeCue cue, SoundEffect variant)
    {
        if (seVoices == null || !isActiveAndEnabled) return;

        SoundEffect effect = (variant != null && variant.HasClip) ? variant : null;
        if (effect == null && !table.TryGetValue(cue, out effect))
        {
            if (logPlays) Debug.Log($"[SE]{cue}（未登録）");
            return;
        }

        if (!effect.HasClip)
        {
            WarnOnce(cue, $"SE {cue} の SoundEffect「{effect.name}」に音源が入っていません。");
            return;
        }

        //同じ音の連打を制限する（ボムで一度に何個もペグが消えるときなど）。
        float now = Time.unscaledTime;
        if (lastPlayTime.TryGetValue(cue, out float last) && now - last < effect.Cooldown) return;
        lastPlayTime[cue] = now;

        AudioClip clip = effect.PickClip();
        if (clip == null) return;

        float pitch = effect.PickPitch();
        float volume = effect.Volume * seVolume * masterVolume;

        if (effect.Kind == SoundKind.Jingle)
        {
            jingleSource.clip = clip;
            jingleSource.volume = volume;
            jingleSource.pitch = pitch;
            jingleSource.Play();
            jingleEndTime = now + clip.length / Mathf.Max(0.01f, Mathf.Abs(pitch));
        }
        else
        {
            var voice = seVoices[nextVoice];
            nextVoice = (nextVoice + 1) % seVoices.Length;
            voice.clip = clip;
            voice.volume = volume;
            voice.pitch = pitch;
            voice.Play();
        }

        if (logPlays) Debug.Log($"[SE]{cue} → {clip.name}（{effect.Kind}）");
    }

    private void WarnOnce(SeCue cue, string message)
    {
        if (warned.Add(cue)) Debug.LogWarning(message);
    }

    // ---- BGM ----

    private void ApplyCurrentState()
    {
        switch (runManager.Phase)
        {
            case RunPhase.Title:
                PlayBgm(BgmCue.Title);
                break;
            case RunPhase.Battle:
            case RunPhase.Reward:
            case RunPhase.Map:
                HandleStageStarted(runManager.StageIndex, runManager.CurrentStage);
                break;
        }
    }

    private void HandleStageStarted(int stageIndex, StageConfig stage)
    {
        bool boss = stage != null && stage.isBoss;
        //ボス戦は、登場のジングルが終わってからBGMを始める（ジングルが鳴っていなければ、すぐ始まる）。
        PlayBgm(boss ? BgmCue.Boss : BgmCue.Battle, boss);
    }

    private void HandleTitle() { PlayBgm(BgmCue.Title); }
    private void HandleStop() { StopBgm(); }

    //曲を切り替える。今と同じ曲なら何もしない。
    public void PlayBgm(BgmCue cue, bool waitForJingle = false)
    {
        if (currentBgm == cue) return;
        currentBgm = cue;

        BgmEntry entry = bgmEntries.Find(e => e.cue == cue);
        pendingClip = entry != null ? entry.clip : null;
        pendingVolume = entry != null ? entry.volume : 0f;
        pendingStartTime = waitForJingle ? Mathf.Max(Time.unscaledTime, jingleEndTime) : Time.unscaledTime;
        pending = true;
        bgmFadeTarget = 0f;   //今の曲をフェードアウトする。

        if (logPlays) Debug.Log($"[BGM]{cue} → {(pendingClip != null ? pendingClip.name : "（曲なし）")}");
    }

    //BGMを止める（フェードアウト）。
    public void StopBgm()
    {
        currentBgm = null;
        pendingClip = null;
        pendingVolume = 0f;
        pendingStartTime = Time.unscaledTime;
        pending = true;
        bgmFadeTarget = 0f;

        if (logPlays) Debug.Log("[BGM]停止");
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;

        //ジングルが鳴っている間は、BGMを下げる。
        float duckTarget = Time.unscaledTime < jingleEndTime ? duckLevel : 1f;
        duckMul = Mathf.MoveTowards(duckMul, duckTarget, dt / Mathf.Max(0.01f, duckSeconds));

        //BGMのフェード。
        bgmFade = Mathf.MoveTowards(bgmFade, bgmFadeTarget, dt / Mathf.Max(0.01f, fadeSeconds));

        //フェードアウトが終わったら、次の曲に入れ替える。
        if (pending && bgmFade <= 0f)
        {
            if (bgmSource.isPlaying) bgmSource.Stop();

            if (Time.unscaledTime >= pendingStartTime)
            {
                pending = false;
                bgmEntryVolume = pendingVolume;
                if (pendingClip != null)
                {
                    bgmSource.clip = pendingClip;
                    bgmSource.Play();
                    bgmFadeTarget = 1f;
                }
            }
        }

        bgmSource.volume = bgmEntryVolume * bgmFade * duckMul * bgmVolume * masterVolume;
    }

    // ---- 音量（あとで設定画面から呼べる） ----

    public void SetMasterVolume(float value) { masterVolume = Mathf.Clamp01(value); }
    public void SetSeVolume(float value) { seVolume = Mathf.Clamp01(value); }
    public void SetBgmVolume(float value) { bgmVolume = Mathf.Clamp01(value); }

    public float MasterVolume => masterVolume;
    public float SeVolume => seVolume;
    public float BgmVolume => bgmVolume;

    // ---- 確認用メニュー（再生中に、このコンポーネントの見出しを右クリックして実行する） ----

    [ContextMenu("テスト：音源が未設定のSEを一覧表示")]
    private void ListMissing()
    {
        foreach (SeCue cue in System.Enum.GetValues(typeof(SeCue)))
        {
            if (!table.TryGetValue(cue, out var effect) || !effect.HasClip) Debug.Log($"未設定：{cue}");
        }
    }

    [ContextMenu("テスト：全SEを順に鳴らす")]
    private void PlayAll()
    {
        StartCoroutine(PlayAllRoutine());
    }

    private System.Collections.IEnumerator PlayAllRoutine()
    {
        foreach (SeCue cue in System.Enum.GetValues(typeof(SeCue)))
        {
            if (!table.ContainsKey(cue)) continue;
            Debug.Log($"[SE確認] {cue}");
            Play(cue, null);
            yield return new WaitForSeconds(0.8f);
        }
    }

    [ContextMenu("テスト：タイトルBGM")]
    private void TestTitle() { PlayBgm(BgmCue.Title); }

    [ContextMenu("テスト：戦闘BGM")]
    private void TestBattle() { PlayBgm(BgmCue.Battle); }

    [ContextMenu("テスト：ボスBGM")]
    private void TestBoss() { PlayBgm(BgmCue.Boss); }

    [ContextMenu("テスト：BGM停止")]
    private void TestStop() { StopBgm(); }
}