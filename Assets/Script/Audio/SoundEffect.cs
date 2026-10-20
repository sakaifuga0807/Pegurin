using UnityEngine;

// 効果音1つぶんの設定。音源を複数入れると、鳴らすたびにランダムで選ぶ。
[CreateAssetMenu(fileName = "SoundEffect", menuName = "Game/SoundEffect")]
public class SoundEffect : ScriptableObject
{
    [Tooltip("音源（.wav／.mp3）。複数入れると毎回ランダムに選ぶ")]
    [SerializeField] private AudioClip[] clips;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;
    [Tooltip("ピッチのばらつき（1.0が元の高さ）。連打する音は少しずらすと単調にならない")]
    [SerializeField] private float pitchMin = 0.95f;
    [SerializeField] private float pitchMax = 1.05f;

    public bool HasClip => clips != null && clips.Length > 0;
    public float Volume => volume;

    public AudioClip PickClip()
    {
        if (!HasClip) return null;
        return clips[Random.Range(0, clips.Length)];
    }

    public float PickPitch()
    {
        return Random.Range(Mathf.Min(pitchMin, pitchMax), Mathf.Max(pitchMin, pitchMax));
    }
}