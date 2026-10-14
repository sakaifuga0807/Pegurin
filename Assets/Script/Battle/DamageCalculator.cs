using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * ダメージ=ペグの基礎ダメージ*(オーブの威力/基準値)
 * 累積ダメージ=合計/倍率=初期値+クリティカル回数*加算値
 * 最終ダメージ=累計ダメージ*倍率(四捨五入)
 */
public class DamageCalculator
{
    //累積は小数のまま持つ。
    private float accumulatedDamage = 0f;
    private float multiplier = GameConstants.MultiplierInitial;

    //累積ダメージ(HUD表示用)。
    public float AccumulatedDamage => accumulatedDamage;
    //現在の倍率。
    public float Multiplier => multiplier;

    //値が変わった時。
    public event Action OnChanged;

    /*ペグに当たるたびに呼ぶ。
     * pegBaseDamage:ペグの基礎ダメージ(スパイク適用後の値を呼ぶ側が渡す)。
     * orbPower:当てたオーブの威力。
     */
    public void AddPegHit(int pegBaseDamage,int orbPower)
    {
        float orbRatio = (float)orbPower / GameConstants.OrbPowerBase;
        accumulatedDamage += pegBaseDamage * orbRatio;
        OnChanged?.Invoke();
    }

    //クリティカルペグに当たった時に呼ぶ。
    public void AddCritMultiplier()
    {
        multiplier += GameConstants.CritMultiplierAdd;
        OnChanged?.Invoke();
    }

    //[仮実装]
    public void AddBaseDamage(int amount)
    {
        accumulatedDamage += amount;
        OnChanged?.Invoke();
    }

    //最終ダメージ。
    public int GetTotalDamage()
    {
        return Mathf.FloorToInt(accumulatedDamage * multiplier + 0.5f);
    }

    //ターン開始時にリセットする。
    public void Reset()
    {
        accumulatedDamage = 0f;
        multiplier = GameConstants.MultiplierInitial;
        OnChanged?.Invoke();
    }
}
