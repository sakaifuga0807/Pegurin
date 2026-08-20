using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//戦闘ターン進行を管理する。
public class BattleStateMachine : MonoBehaviour
{
    //戦闘のステートを定義。
    public enum  BattleState
    {
        TurnStart,      //ターン開始。
        PlayerInput,    //発射待ち。
        OrbInFlight,    //オーブが飛んでいる。
        Resolution,     //ターン結果処理。
        TurnEnd,        //ターン終了判定。
    }

    public BattleState CurrentState { get; private set; } = BattleState.TurnStart;

    private void Start()
    {
        //最初のステートを開始。
        ChangeState(BattleState.TurnStart);
    }

    public void ChangeState(BattleState newState)
    {
        CurrentState = newState;
    }
}
