using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//行動予告に表示する行動の種類。
public enum EnemyIntentType
{
    Attack,         //攻撃。
    HeavyAttack,    //強攻撃・大技。
    Charge,         //チャージ。
    Summon,         //召喚。
}

//予告一つ分。Valueは攻撃・強攻撃の時だけ意味がある。
public readonly struct EnemyIntent
{
    public readonly EnemyIntentType Type;
    public readonly int Value;

    public EnemyIntent(EnemyIntentType type,int value=0)
    {
        Type = type;
        Value = value;
    }

    //HUDの攻撃予告に数える行動か。
    public bool IsDamage =>
        Type == EnemyIntentType.Attack || Type == EnemyIntentType.HeavyAttack;
}

public abstract class EnemyAction : ScriptableObject
{
    //予告用。
    public abstract EnemyIntent GetIntent(Enemy self, EnemyLane lane);

    //実行用。プレイヤーに与えるダメージを返す。
    public abstract int Execute(Enemy self, EnemyLane lane);
}
