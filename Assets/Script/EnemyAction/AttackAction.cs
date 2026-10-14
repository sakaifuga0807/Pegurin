using UnityEngine;

//UŒ‚BisHeavy‚ªtrue‚È‚ç‹­UŒ‚E‘å‹Z(UŒ‚—Í*‹­UŒ‚”{—¦)
[CreateAssetMenu(fileName ="AttackAction",menuName ="Game/EnemyAction/Attack")]
public class AttackAction:EnemyAction
{
    [Tooltip("true‚È‚ç‹­UŒ‚E‘å‹Z")]
    [SerializeField] private bool isHeavy = false;

    private int GetDamage(Enemy self)
    {
        return isHeavy ? self.HeavyAttackDamage : self.NormalAttackDamage;
    }

    public override EnemyIntent GetIntent(Enemy self, EnemyLane lane)
    {
        var type = isHeavy ? EnemyIntentType.HeavyAttack : EnemyIntentType.Attack;
        return new EnemyIntent(type, GetDamage(self));
    }

    public override int Execute(Enemy self, EnemyLane lane)
    {
        return GetDamage(self);
    }
}
