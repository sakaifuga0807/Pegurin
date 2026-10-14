using UnityEngine;

/*
 * 召喚。
 * 空きマスがなければ召喚をせずに通常攻撃をする。
 */

[CreateAssetMenu(fileName ="SummonAction",menuName ="Game/EnemyAction/Summon")]
public class SummonAction:EnemyAction
{
    [Tooltip("召喚する敵")]
    [SerializeField] private EnemyData summonTarget;

    //召喚できるか。
    private bool CanSummon(EnemyLane lane)
    {
        return lane.FindBackmostEmptyCell() != 0;
    }

    public override EnemyIntent GetIntent(Enemy self, EnemyLane lane)
    {
        if(!CanSummon(lane))
        {
            //満員で召喚できなかったら通常攻撃。
            return new EnemyIntent(EnemyIntentType.Attack, self.NormalAttackDamage);
        }
        return new EnemyIntent(EnemyIntentType.Summon);
    }

    public override int Execute(Enemy self, EnemyLane lane)
    {
        if(!CanSummon(lane))
        {
            //召喚せずに攻撃。
            return self.NormalAttackDamage;
        }

        if(summonTarget==null)
        {
            Debug.LogError("SummonActionのSummon Targetが未設定です。");
            return 0;
        }

        lane.Spawn(summonTarget, lane.FindBackmostEmptyCell());
        return 0;
    }
}
