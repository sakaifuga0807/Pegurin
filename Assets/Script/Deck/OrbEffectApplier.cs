using UnityEngine;

//OrbDataの効果種別を見て、玉に効果コンポーネントを付ける。
public static class OrbEffectApplier
{
    public static void Apply(GameObject orbObj, OrbData data)
    {
        if (data == null) return;

        switch (data.effectType)
        {
            case OrbEffectType.Vibes:
                orbObj.AddComponent<VibesOrb>();
                break;
        }
    }
}