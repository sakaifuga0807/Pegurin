using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//合計ダメージを取得して、ターン終了。
public class ResolutionState : IBattleState
{
    public void Enter(BattleStateMachine stateMachine)
    {
        var context = BattleContext.Current;
        var lane = context.EnemyLane;

        //このターンに与えた合計ダメージを取得。
        int totalDamage = BattleContext.Current.DamageCalculator.GetTotalDamage();

        //一番手前の敵一体だけに与える。
        Enemy target = lane.ApplyDamageToFront(totalDamage);

        //取得したダメージを表示。
        Debug.Log($"合計ダメージ{(target!=null?target.Data.EnemyName+"残HP:"+target.CurrentHp:"対象なし")}");

        //全滅していれば敵のターンは行わず、ステージクリア。
        if(lane.IsEmpty)
        {
            //TODO:RunManagerにクリアを通知する。
            Debug.Log("ステージクリア。");
            return;
        }

        //ターン終了。
        stateMachine.ChangeState(new TurnEndState());
    }
}