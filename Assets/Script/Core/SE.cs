using System;

//この音を鳴らしてという指示の入り口。
public static class SE
{
    //個別の音がnullならSoundBankの音を使う。
    public static event Action<SeCue, SoundEffect> OnPlay;

    public static void Play(SeCue cue)
    {
        OnPlay?.Invoke(cue, null);
    }

    //玉ごと・敵ごとの音を使いたいときの指示。
    public static void Play(SeCue cue,SoundEffect variant)
    {
        OnPlay?.Invoke(cue, variant);
    }
}