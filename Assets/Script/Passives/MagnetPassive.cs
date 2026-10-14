using Unity.VisualScripting;
using UnityEngine;

//マグネット:落下する手前で1回だけ上方向に力を加える。
[CreateAssetMenu(fileName ="MagnetPassive",menuName ="Game/Passive/Magnet")]
public class MagnetPassive:PassiveEffect
{
    public override bool EnableMagnet => true;
}