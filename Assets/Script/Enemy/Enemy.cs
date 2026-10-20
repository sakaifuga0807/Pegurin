using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *戦闘中の敵の状態を管理するクラス。種類ごとのデータはEnemyDataがもつ。
 *レーンの状態はEnemyLaneがもつ。
 *このクラスはUnityのイベントや画面を知らない。敵の状態を管理するだけ。
 */

public class  Enemy
{
    //敵の種類ごとのデータ。
    public EnemyData Data { get; }
    //現在のHP。0以下で死亡。
    public int CurrentHp { get; private set; }
    //現在のレーンの位置。0が一番手前。
    public int CurrentCell { get; private set; }
    //現在の行動の現在位置。
    public int ActionIndex { get; private set; }
    public bool IsDead => CurrentHp <= 0;
    public bool IsInRange => CurrentCell <= Data.Range;

    //通常の敵はdata.StartCell、召喚された敵は召喚時の空きマスを渡す。
    public Enemy(EnemyData data,int startCell)
    {
        Data = data;
        CurrentHp = data.MaxHp;
        CurrentCell = Mathf.Clamp(startCell, 1, GameConstants.LaneCellCount);
        ActionIndex = 0;
    }

    /*HPの処理。*/

    //ダメージを受ける。HPは0未満にならない。
    public void TakeDamage(int amount)
    {
        //早期リターン。ダメージが0以下なら処理しない。
        if (amount <= 0) return;
        CurrentHp = Mathf.Max(0, CurrentHp - amount);
    }

    /*行動パターン。*/

    //現在の行動。戻り値はnullの可能性がある。
    public EnemyAction CurrentAction =>
        Data.ActionPattern.Count == 0 ? null : Data.ActionPattern[ActionIndex];

    //行動したときだけ呼ぶ関数(移動・待機では呼ばない。)最後まで行くとループする。
    public void AdvanceAction()
    {
        int count = Data.ActionPattern.Count;
        if (count == 0) return;
        ActionIndex = (ActionIndex + 1) % count;
    }

    /*ダメージ(強攻撃・大技は攻撃力*強攻撃倍率を四捨五入)*/

    public int NormalAttackDamage => Data.AttackPower;

    public int HeavyAttackDamage =>
        Mathf.RoundToInt(Data.AttackPower * GameConstants.HeavyAttackMultiplier);

    /*レーン上の位置。*/

    //移動で進める最大マスス数。
    public int MaxMoveSteps =>
        Mathf.Max(0, Mathf.Min(Data.MoveSpeed, CurrentCell - Data.Range));

    //あと何ターンで攻撃できるか(切り上げ)。予告のために使う。移動しない場合は0。
    public int TurnsToRange
     {
        get
        {
            if (IsInRange) return 0;
            return Mathf.CeilToInt((float)(CurrentCell - Data.Range) / Data.MoveSpeed);
        }
    }

    public void MoveToCell(int cell)
    {
        CurrentCell = Mathf.Clamp(cell, 1, GameConstants.LaneCellCount);
    }
}
