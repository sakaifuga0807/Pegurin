using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ターン開始自にダメージをリセットし、次の玉を引く。
public class TurnStartState : IBattleState
{
    public void Enter(BattleStateMachine stateMachine)
    {
        //前のターンの合計ダメージをリセット。
        BattleContext.Current.DamageCalculator.Reset();

        //安全ネットの使用済みを戻す。
        var passiveManager = BattleContext.Current.PassiveManager;
        if (passiveManager != null) passiveManager.ResetTurn();

        //玉を引く。引けなかったらエラーを出す。
        if(!BattleContext.Current.DeckManager.DrawNextOrb())
        {
            Debug.LogError("玉が引けなかったのでターンを開始できませんでした。InitialDeckを確認してください。");
            return;
        }
        //玉が引けたらどの玉が引けたかを表示。
        Debug.Log($"ターン開始。引いた玉:{BattleContext.Current.DeckManager.CurrentOrb?.orbName}");
        //入力待ちへ遷移。
        stateMachine.ChangeState(new PlayerInputState());
    }
}
