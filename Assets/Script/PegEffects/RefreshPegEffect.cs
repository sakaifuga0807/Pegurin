using UnityEngine;

[CreateAssetMenu(menuName = "PegEffects/RefreshPeg")]
public class RefreshPegEffect : PegEffect
{
    public override void Execute(PegHitContext context)
    {
        PegHitRules.HitRefresh(context);
    }
}