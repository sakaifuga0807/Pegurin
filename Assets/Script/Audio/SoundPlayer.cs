using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SeEntry
{
    public SeCue cue;
    public SoundEffect effect;
}

//効果音を鳴らす。呼び出し側はSoundPlayer.PlayCueを呼ぶだけ。
//音源は未設定でもエラーにはならない。
public class SoundPlayer : MonoBehaviour
{
    [Header("SEの割り当て")]
    [SerializeField] private List<SeEntry> entries = new List<SeEntry>();

    [Header("再生設定")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;
    [Tooltip("同時にならせる数")]
    [SerializeField] private int voiceCount = 8;

    [Header("確認用")]
    [SerializeField] private bool logPlays = false;

    private readonly Dictionary<SeCue, SoundEffect> table = new Dictionary<SeCue, SoundEffect>();
    private readonly HashSet<SeCue> warned = new HashSet<SeCue>();
    private AudioSource[] voices;
    private int nextVoice;

    private static bool warnedNoContext;
    private static bool warnedNoPlayer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        warnedNoContext = false;
        warnedNoContext = false;
    }

    private void Awake()
    {
        //表を作る。
        foreach(var entry in entries)
        {
            if(entry.effect==null)
            {
                continue;
            }

            if(table.ContainsKey(entry.cue))
            {
                Debug.LogWarning($"SE{entry.cue}が重複登録されています。");
            }
            table[entry.cue] = entry.effect;
        }

        //再生用のAudioSourceを作る。
        voices = new AudioSource[Mathf.Max(1, voiceCount)];
        for(int i=0;i<voices.Length;i++)
        {
            voices[i] = gameObject.AddComponent<AudioSource>();
            voices[i].playOnAwake = false;
            voices[i].spatialBlend = 0f;    //2Dの音にする。
        }
    }

    private void Start()
    {
        //音が出ない環境になって以内かを起動時に確認する。
        if(FindFirstObjectByType<AudioListener>()==null)
        {
            Debug.LogWarning("シーンにAudioListenerがありません。MainCameraについているか確認してねん。");
        }
        if (AudioListener.volume <= 0f)
        {
            Debug.LogWarning("AudioListenerの全体音量が0です。");
        }
    }

    //どこからでも呼び出せるように。
    public static void PlayCue(SeCue cue)
    {
        var context = BattleContext.Current;
        if(context==null)
        {
            if(!warnedNoContext)
            {
                warnedNoContext = true;
                Debug.LogWarning("SE:BattleContextがシーンにありません。");
            }
            return;
        }
        if(context.SoundPlayer==null)
        {
            if(!warnedNoPlayer)
            {
                warnedNoPlayer = true;
                Debug.LogWarning("SE:BattleContextのSoundPlayerが未設定です。");
            }
            return;
        }
        context.SoundPlayer.Play(cue);
    }

    public void Play(SeCue cue)
    {
        if(!table.TryGetValue(cue,out var effect)||!effect.HasClip)
        {
            if (logPlays)
            {
                Debug.Log($"[SE]{cue}(音源なし)");
            }
            return;
        }

        var clip = effect.PickClip();
        if(clip==null)
        {
            return;
        }

        var voice = voices[nextVoice];
        nextVoice = (nextVoice + 1) % voices.Length;

        voice.clip = clip;
        voice.volume = effect.Volume * masterVolume;
        voice.pitch = effect.PickPitch();
        voice.Play();
    }

    private void WarnOnce(SeCue cue, string message)
    {
        if (warned.Add(cue)) Debug.LogWarning(message);
    }

    //確認用：音源がまだ入っていないSEの一覧を出す。
    [ContextMenu("テスト：音源が未設定のSEを一覧表示")]
    private void ListMissing()
    {
        var done = new HashSet<SeCue>();
        foreach (var entry in entries)
        {
            if (entry.effect != null && entry.effect.HasClip) done.Add(entry.cue);
        }
        foreach (SeCue cue in System.Enum.GetValues(typeof(SeCue)))
        {
            if (!done.Contains(cue)) Debug.Log($"未設定：{cue}");
        }
    }

    //確認用：登録済みの全SEを、0.6秒ずつ順に鳴らす（再生中に実行する）。
    [ContextMenu("テスト：全SEを順に鳴らす")]
    private void PlayAll()
    {
        StartCoroutine(PlayAllRoutine());
    }

    private System.Collections.IEnumerator PlayAllRoutine()
    {
        foreach (var entry in entries)
        {
            if (entry.effect == null) continue;
            Debug.Log($"[SE確認] {entry.cue}");
            Play(entry.cue);
            yield return new WaitForSeconds(0.6f);
        }
    }
}