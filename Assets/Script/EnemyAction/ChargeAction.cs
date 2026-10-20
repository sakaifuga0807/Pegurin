using System.IO;
using UnityEngine;


//溜め攻撃。ダメージはここではまだ与えない。
[CreateAssetMenu(fileName = "ChargeAction", menuName = "Game/EnemyAction/Charge")]
public class ChargeAction : EnemyAction
{
    public override EnemyIntent GetIntent(Enemy self, EnemyLane lane)
    {
        return new EnemyIntent(EnemyIntentType.Charge);
    }

    public override int Execute(Enemy self, EnemyLane lane)
    {
        return 0;
    }
}

