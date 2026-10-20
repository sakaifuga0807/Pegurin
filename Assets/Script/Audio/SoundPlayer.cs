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
    private AudioSource[] voices;
    private int nextVoice;

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
        }
    }

    //どこからでも呼び出せるように。
    public static void PlayCue(SeCue cue)
    {
        var context = BattleContext.Current;
        if(context==null||context.SoundPlayer==null)
        {
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
}