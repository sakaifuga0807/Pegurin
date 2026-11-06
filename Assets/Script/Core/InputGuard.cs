using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//画面の部品（確認用パネル、UIのボタン）の上にマウスがあるときは、ゲームの操作（発射など）を止めるための印。
//ゲームの入力を読む側は、InputGuard.IsBlocked が true のときは、入力を無視する。
public static class InputGuard
{
    private static readonly HashSet<object> blockers = new HashSet<object>();

    //確認用パネルなどが、「マウスが上にある」ことを知らせる。
    public static void SetBlocked(object owner, bool blocked)
    {
        if (blocked) blockers.Add(owner);
        else blockers.Remove(owner);
    }

    //true のあいだは、ゲームの操作を止める。
    public static bool IsBlocked
    {
        get
        {
            if (blockers.Count > 0) return true;
            //③のUI（Canvas）のボタンの上でも、止める。
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }

    //再生を始めるたびに、状態をリセットする。
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        blockers.Clear();
    }
}