using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//画面の部品（確認用パネル、UIのボタンなど）の上にマウスがあるときは、ゲームの操作（発射など）を止めるための印。
//押せないUI（背景の画像など）では、止めない。
public static class InputGuard
{
    private static readonly HashSet<object> blockers = new HashSet<object>();
    private static readonly List<RaycastResult> results = new List<RaycastResult>();

    //確認用パネルなどが、「マウスが上にある」ことを知らせる。
    public static void SetBlocked(object owner, bool blocked)
    {
        if (blocked) blockers.Add(owner);
        else blockers.Remove(owner);
    }

    //true のあいだは、ゲームの操作を止める（クリックしたときに調べる）。
    public static bool IsBlocked => blockers.Count > 0 || PointerOverInteractiveUi() != null;

    //止めているものの名前（ログ用）。
    public static string DescribeBlocker()
    {
        if (blockers.Count > 0) return "確認用パネル（サウンドテストなど）";
        var ui = PointerOverInteractiveUi();
        if (ui != null) return $"UIのボタンなど「{ui.name}」";
        return "なし";
    }

    //マウスの下にある、押せるUI（ボタン、スライダーなど）を探す。
    private static GameObject PointerOverInteractiveUi()
    {
        var es = EventSystem.current;
        if (es == null) return null;

        var data = new PointerEventData(es) { position = Input.mousePosition };
        results.Clear();
        es.RaycastAll(data, results);

        foreach (var hit in results)
        {
            if (hit.gameObject == null) continue;
            var selectable = hit.gameObject.GetComponentInParent<Selectable>();
            if (selectable != null && selectable.IsInteractable()) return selectable.gameObject;
        }
        return null;
    }

    //再生を始めるたびに、状態をリセットする。
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        blockers.Clear();
        results.Clear();
    }
}