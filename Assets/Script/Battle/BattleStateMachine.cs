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

    //ステージの敵リスト。
    [SerializeField] private List<EnemyData> testEnemies = new List<EnemyData>();

    private void Start()
    {
        //戦闘開始時に山札を初期化。
        BattleContext.Current.DeckManager.InitializeDeck();

        //敵れーを初期マスで作り直す。
        BattleContext.Current.EnemyLane.Setup(testEnemies);

        //最初にTurnStartステートに遷移。
        ChangeState(new TurnStartState());
    }

    //ステートを切り替える。新しいステートのEnterを呼び出す。
    public void ChangeState(IBattleState newState)
    {
        CurrentState = newState;
        CurrentState.Enter(this);
    }

    //仮実装としてステートの遷移を確かめる。
    private void Update()
    {
        //左クリックで発射。
        if(CurrentState is PlayerInputState&&Input.GetMouseButtonDown(0))
        {
            //発射した時点でオーブを捨て札へ送る。
            BattleContext.Current.DeckManager.DiscardCurrentOrb();
            //ステートを玉が飛んでるステートに遷移。
            ChangeState(new OrbInFlightState());
        }
        //玉が飛んでる間に左クリックでダメージを与える。
        else if (CurrentState is OrbInFlightState&& Input.GetMouseButtonDown(0))
        {
            //[仮実装] ダメージの計算は後で作るので、ここでは固定値を足す。
            BattleContext.Current.DamageCalculator.AddBaseDamage(10);
            //ステートを合計ダメージステートに遷移。
            ChangeState(new ResolutionState());
        }
    }
}
