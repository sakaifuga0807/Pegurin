using System.Threading;
using UnityEngine;

//イベントで使用する値。
public readonly struct HpInfo
{
    public readonly int Current;
    public readonly int Max;
    public HpInfo(int current, int max) { Current = current;Max = max;  }
}

public readonly struct DamageInfo
{
    public readonly float Accumulated;      //累計ダメージ。
    public readonly float Multiplier;       //倍率。
    public readonly int Total;              //最終ダメージ。
    public DamageInfo(float accumulated, float multiplier, int total)
    { Accumulated = accumulated;Multiplier = multiplier;Total = total; }
}

public readonly struct EnemyMovedInfo
{
    public readonly Enemy Enemy;
    public readonly int FromCell;
    public readonly int ToCell;
    public EnemyMovedInfo(Enemy enemy,int from,int to)
    { Enemy = enemy;FromCell = from;ToCell = to; }
}