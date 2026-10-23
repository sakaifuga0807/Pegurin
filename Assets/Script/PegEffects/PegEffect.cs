using UnityEngine;

public abstract class PegEffect : ScriptableObject
{
    public abstract void Execute(PegHitContext context);
}