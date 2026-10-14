using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnEndState : IBattleState
{
    public void Enter(BattleStateMachine stateMachine)
    {
        Debug.Log("ターン終了。次のターンへ");
        stateMachine.ChangeState(new TurnStartState());
    }
}
