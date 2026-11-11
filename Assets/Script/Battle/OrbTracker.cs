using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//発射した弾を数える。全ての玉が落ちたら通知をする。
public static class OrbTracker
{
    private static readonly List<GameObject> flying = new List<GameObject>();

    //飛んでる玉が全部なくなった時。
    public static event Action AllLanded;

    public static int Count => flying.Count;

    //発射した玉を登録する。
    public static void Register(GameObject orb)
    {
        if (orb != null && !flying.Contains(orb)) flying.Add(orb);
    }

    //玉が落ちたとき。全部落ちたら通知をする。
    public static void NotifyLanded(GameObject orb)
    {
        if (!flying.Remove(orb)) return;
        if (flying.Count == 0) AllLanded?.Invoke();
    }

    //他の理由で消えた玉を、落ちたものとして数える。
    public static void Prune()
    {
        if (flying.Count == 0) return;
        bool removed = flying.RemoveAll(o => o == null) > 0;
        if (removed && flying.Count == 0) AllLanded?.Invoke();
    }

    //玉が飛んでいる玉を全部消す。
    public static void DestroyAll()
    {
        foreach(var orb in flying)
        {
            if (orb != null) UnityEngine.Object.Destroy(orb);
        }
        flying.Clear();
    }

    //詰まった玉を強制的に落ちたことにする。
    public static void ForceLandAll()
    {
        if (flying.Count == 0) return;
        DestroyAll();
        AllLanded?.Invoke();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        flying.Clear();
        AllLanded = null;
    }
}