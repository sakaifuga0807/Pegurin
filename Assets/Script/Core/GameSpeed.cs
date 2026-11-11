using NUnit.Framework.Internal.Commands;
using UnityEditor;
using UnityEngine;

//ゲームの速さとポーズを、一か所で持つ。
//待ちはゲームの時間で数える。UIやBGMのフェードは実際の時間で数える。
public static class GameSpeed
{
    //玉が壁や、ペグをすり抜けないように上限を3倍にする。
    public const float MaxMultiplier = 3f;

    private static float multiplier = 1f;
    private static bool paused;

    public static float Multiplier => multiplier;
    public static bool Paused => paused;

    //速さやポーズが変わった時。
    public static event System.Action Changed;

    //倍速を変える。ポーズ中は、解除したときに反映される。
    public static void SetMultiplier(float value)
    {
        multiplier = Mathf.Clamp(value, 1f, MaxMultiplier);
        Apply();
    }

    public static void SetPaused(bool value)
    {
        paused = value;
        Apply();
    }

    //倍速もポーズも、元に戻す。
    public static void ResetAll()
    {
        multiplier = 1f;
        paused = false;
        Apply();
    }

    private static void Apply()
    {
        Time.timeScale = paused ? 0f : multiplier;
        Changed?.Invoke();
    }

    //再生を始めるたびに、状態をリセットする。
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        multiplier = 1f;
        paused = false;
        Time.timeScale = 1f;
        Changed = null;
    }
}