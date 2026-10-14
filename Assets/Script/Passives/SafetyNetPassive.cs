using UnityEngine;

//安全ネット:1ターンに1回落下したオーブを跳ね返す。
[CreateAssetMenu(fileName ="SafetyNetPassive",menuName ="Game/Passive/SafetyNet")]
public class SafetyNetPassive:PassiveEffect
{
    public override bool BlocksFallOncePerTurn => true;
}
