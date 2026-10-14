using System.Reflection;
using UnityEngine;

//スパイク:通常ペグに追加攻撃。
[CreateAssetMenu(fileName ="SpikePassive",menuName ="Game/Passive/Spike")]
public class SpikePassive:PassiveEffect
{
    public override int ModifyNormalPegDamage(int baseDamage)
    {
        return baseDamage + GameConstants.SpikeBonus;
    }
}
