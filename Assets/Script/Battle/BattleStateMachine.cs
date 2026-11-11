using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

//戦闘ターン進行を管理するステートマシン。
// TurnStart→PlayerInput→OrbInFlight→Resolution→EnemyTurn→NextTurn。
public class BattleStateMachine : MonoBehaviour
{
    //現在のステート。外部からは読み取り用。
    public IBattleState CurrentState { get; private set; }

    //今玉を撃ってい良い状態か。
    public bool CanLaunch => CurrentState is PlayerInputState;

    //見た目の受け口。
    public event Action Launched;   //玉を発射したとき。
    public event Action Landed;     //玉が全部落ちたとき。

    //全部の玉が落ちたら、OrbTrackerが知らせてくれる。
    private void OnEnable() { OrbTracker.AllLanded += OnOrbLanded; }
    private void OnDisable() { OrbTracker.AllLanded -= OnOrbLanded; }

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
        if (!CanLaunch) return;

        BattleContext.Current.DeckManager.DiscardCurrentOrb();
        ChangeState(new OrbInFlightState());
        Launched?.Invoke();
    }

    //このターンの弾が落ちたとき、玉が全て落ちたかの判定。
    public void OnOrbLanded()
    {
        if(!(CurrentState is OrbInFlightState)) return;

        ChangeState(new ResolutionState());
        Landed?.Invoke();
    }

    //戦闘を止める。
    public void Stop()
    {
        CurrentState = null;
    }

    //テスト用。
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