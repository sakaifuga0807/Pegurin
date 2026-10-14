using UnityEngine;

//リフレッシュ爆発:リフレッシュペグ発動時、敵全体に30ダメージ
[CreateAssetMenu(fileName ="RefreshBlastPassive",menuName ="Game/Passive/RefreshBlast")]
public class RefreshBlastPassive:PassiveEffect
{
    public override void OnRefreshPegTriggered(BattleContext context)
    {
        //倍率は掛けない。
        context.EnemyLane.ApplyDamageToAll(GameConstants.RefreshBlastDamage);
    }
}
