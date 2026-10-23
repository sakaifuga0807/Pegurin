using UnityEngine;

public static class PegHitRules
{
    public static void HitNormal(PegHitContext context)
    {
        Debug.Log($"[PegHitRules] ノーマルペグヒット (Orb: {context.orb.name})");
    }

    public static void HitCritical(PegHitContext context)
    {
        Debug.Log($"[PegHitRules] クリティカルペグヒット！");
    }

    public static void HitRefresh(PegHitContext context)
    {
        Debug.Log($"[PegHitRules] リフレッシュペグ発動：盤面を全復活");
        if (context.board != null)
        {
            context.board.RefreshAllPegs();
        }
    }
}