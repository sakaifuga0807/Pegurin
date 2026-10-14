using UnityEngine;

/*
 * パッシブ1種類ぶんの土台。状態は持たない
 * 各パッシブは、かかわる場面の入り口だけをoverrideする。
 */
public abstract class PassiveEffect : ScriptableObject
{
    [SerializeField] private string passiveName;
    [SerializeField] private Sprite icon;

    public string PassiveName => passiveName;
    public Sprite Icon=>icon;

    //スパイク:通常ペグの基礎ダメージを変える。クリティカル・リフレッシュには使わない。
    public virtual int ModifyNormalPegDamage(int baseDamage) => baseDamage;

    //リフレッシュ爆発:リフレッシュペグが発動したときに呼ばれる。
    public virtual void OnRefreshPegTriggered(BattleContext context) { }

    //安全ネット:落下ゾーンに触れたオーブを、1ターンに1回跳ね返すか。
    public virtual bool BlocksFallOncePerTurn => false;

    //マグネット:落下ソーン手前で、降下中に一回だけ上向きの力を受けるか。
    public virtual bool EnableMagnet => false;
}

