using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//戦闘ターン進行を管理するステートマシン。
// TurnStart→PlayerInput→OrbInFlight→Resolution→EnemyTurn→NextTurn。
public class BattleStateMachine : MonoBehaviour
{
    //現在のステート。外部からは読み取り用。
    public IBattleState CurrentState { get; private set; }

    //戦闘開始。RunManagerがステージ開始のたびに呼ぶ。
    public void StartBattle()
    {
        ChangeState(new TurnStartState());
    }

    //ステートを切り替える。新しいステートのEnterを呼び出す。
    public void ChangeState(IBattleState newState)
    {
        CurrentState = newState;
        CurrentState.Enter(this);
    }



    //弾を発射したとき、発射した時点で、引いていたオーブを捨て札へ送る。
    public void OnOrbLaunched()
    {
        if(!(CurrentState is PlayerInputState))
        {
            return;
        }

        BattleContext.Current.DeckManager.DiscardCurrentOrb();
        ChangeState(new OrbInFlightState());
    }

    //このターンの弾が落ちたとき、玉が全て落ちたかの判定。
    public void OnOrbLanded()
    {
        if(!(CurrentState is OrbInFlightState))
        {
            return;
        }

        ChangeState(new ResolutionState());
    }

    //戦闘を止める。
    public void Stop()
    {
        CurrentState = null;
    }

    //消してねー
    [ContextMenu("テスト：撃たずに1ターン進める")]
    private void TestSkipTurn()
    {
        OnOrbLaunched();
        OnOrbLanded();
    }

    [ContextMenu("テスト：10ダメージを与える")]
    private void TestDamage10()
    {
        OnOrbLaunched();
        BattleContext.Current.DamageCalculator.AddPegHit(10, 10);
        OnOrbLanded();
    }

    [ContextMenu("テスト：130ダメージを与える")]
    private void TestDamage130()
    {
        OnOrbLaunched();
        BattleContext.Current.DamageCalculator.AddPegHit(130, 10);
        OnOrbLanded();
    }

}