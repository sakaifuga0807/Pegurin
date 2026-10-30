using Unity.Multiplayer.PlayMode;
using UnityEngine;


//ペグに当たった時のルール。
public static class PegHitRules
{ 
    //通常ペグ。
    public static void HitNormal(PegHitContext contexet)
    {
        var battle = BattleContext.Current;
        if (battle == null) return;

        int damage = GameConstants.PegBaseDamage;
        if(battle.PassiveManager!=null)
        {
            damage = battle.PassiveManager.ModifyNormalPegDamage(damage);
        }
        battle.DamageCalculator.AddPegHit(damage, contexet.orb.Power);
        SoundPlayer.PlayCue(SeCue.PegHitNormal);
        Debug.Log($"[Peg]通常+{damage}(威力{contexet.orb.Power})");
    }

    //クリティカルペグ
    public static void HitCritical(PegHitContext context)
    {
        var battle = BattleContext.Current;
        if (battle == null) return;

        battle.DamageCalculator.AddPegHit(GameConstants.PegBaseDamage, context.orb.Power);
        battle.DamageCalculator.AddCritMultiplier();
        SoundPlayer.PlayCue(SeCue.PegHitCrit);
        Debug.Log("[peg]クリティカル倍率+0.5");
    }

    //リフレッシュペグ
    public static void HitRefresh(PegHitContext context)
    {
        var battle = BattleContext.Current;
        if (battle == null) return;

        battle.DamageCalculator.AddPegHit(GameConstants.PegBaseDamage, context.orb.Power);
        if (context.board != null)
        {
            context.board.RefreshAllPegs(context.peg);
        }
        if (battle.PassiveManager != null)
        {
            battle.PassiveManager.NotifyRefreshPegTriggered();
        }
        SoundPlayer.PlayCue(SeCue.PegHitRefresh);
        Debug.Log("[peg]リフレッシュ");
    }
}