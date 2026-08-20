using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//ただのC#クラス。
//GameObjectにアタッチするものではなく、BattleContextがnewして使う。
public class DamageCalculator
{
    private int baseDamageSum = 0;
    private float multiplier = 1f;

    //ペグに当たるたびに呼ばれる。
    public void  AddBaseDamage(int amount)
    {
        baseDamageSum += amount;
    }

    //クリティカルペグに当たった時に呼ばれる想定。
    public void AddMultiplier(float amount)
    {
        multiplier += amount;
    }

    //弾が落ちたタイミングで、最終的な合計ダメージを計算して返す。
    public int GetTotalDamage()
    {
        return Mathf.RoundToInt(baseDamageSum * multiplier);
    }

    //ターン開始時に飛んで次のターンにリセット。
    public void Reset()
    {
        baseDamageSum = 0;
        multiplier = 1f;
    }
}
