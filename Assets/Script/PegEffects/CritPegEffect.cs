using UnityEngine;

[CreateAssetMenu(menuName = "PegEffects/CritPeg")]
public class CritPegEffect : PegEffect
{
    public override void Execute(PegHitContext context)
    {
        PegHitRules.HitCritical(context);
    }
}