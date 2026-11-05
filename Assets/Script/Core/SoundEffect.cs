using UnityEngine;

//音の種類。ジングルは専用の枠で鳴り、なっている間BGMの音量が下がる。
public enum SoundKind { Se,Jingle}

//効果音一つ分の設定。音源を複数入れると、鳴らすたびにランダムで選ぶ。
[CreateAssetMenu(fileName ="SoundEffect",menuName ="Game/SoundEffect")]
public class SoundEffect:ScriptableObject
{
    [Tooltip("Se:普通の効果音。Jingle:クリア・ーゲームオーバー・ボス登場")]
    [SerializeField] private SoundKind kind = SoundKind.Se;
    [Tooltip("複数入れると毎回ランダムに選ぶ")]
    [SerializeField] private AudioClip[] clips;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;
    [Tooltip("ピッチのばらつき。ジングルは両方1にする。")]
    [SerializeField] private float pitchMin = 0.95f;
    [SerializeField] private float pitchMax = 1.05f;
    [Tooltip("同じ音をつづけて鳴らさない最短の間隔。ペグの命中など、連打する音のため。0なら制限なし")]
    [Min(0f)]
    [SerializeField] private float cooldown = 0.03f;

    public SoundKind Kind => kind;
    public float Volume => volume;
    public float Cooldown => cooldown;

    //音源が一つでも入っているか。
    public bool HasClip
    {
        get 
        {
            if (clips == null) return false;
            foreach(var clip in clips)
            {
                if (clip != null) return true;
            }
            return false;
        }
    }

    //入ってる音源から、ランダムに一つ選ぶ。
    public AudioClip PickClip()
    {
        if (!HasClip) return null;

        int count = 0;
        foreach(var clip in clips)
        {
            if (clip != null) count++;
        }

        int pick = Random.Range(0, count);
        foreach(var clip in clips)
        {
            if (clip == null) continue;
            if (pick == 0) return clip;
            pick--;
        }
        return null;
    }

    public float PickPitch()
    {
        return Random.Range(Mathf.Min(pitchMin, pitchMax), Mathf.Max(pitchMin, pitchMax));
    }
}