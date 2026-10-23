using UnityEngine;

[CreateAssetMenu(menuName = "PegEffects/NormalPeg")]
public class NormalPegEffect : PegEffect
{
    public override void Execute(PegHitContext context)
    {
        PegHitRules.HitNormal(context);
    }
}