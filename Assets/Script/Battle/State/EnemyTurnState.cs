using UnityEngine;

//敵のターン:手前の敵から順に、射程内なら行動、射程外なら移動。
public class EnemyTurnState : IBattleState
{
    public void Enter(BattleStateMachine stateMachine)
    {
        var context = BattleContext.Current;
        var lane = context.EnemyLane;
        int totalDamage = 0;

        //最初に手前からのリストを固定する。
        //コピーなので、このターンに召喚された敵は処理に混ざらない。
        var enemies = lane.GetEnemiesFrontToBack();

        foreach(var enemy in enemies)
        {
            if(enemy.IsInRange)
            {
                //射程内:行動する。行動パターンを一つ進める。
                var action = enemy.CurrentAction;
                if (action == null) continue;

                int damage = action.Execute(enemy, lane);
                totalDamage += damage;
                enemy.AdvanceAction();
                Debug.Log($"{enemy.Data.EnemyName}（マス{enemy.CurrentCell}）が行動：ダメージ{damage}");
            }
            else
            {
                //射程外:移動する。
                int moved = lane.Advance(enemy);
                Debug.Log($"{enemy.Data.EnemyName} が{moved}マス移動 → マス{enemy.CurrentCell}");
            }

        }

        //行動パターンが進んだので、予告を計算し直すよう通知する。
        lane.NotifyIntentChanged();

        //全員の処理が終わってから、攻撃の合計をまとめてHPから引く。
        context.Player.TakeDamage(totalDamage);
        Debug.Log($"敵のターン終了：合計{totalDamage}ダメージ。プレイヤーHP:{context.Player.CurrentHp}");

        //HPが0ならゲームオーバー。
        if(context.Player.IsDead)
        {
            return;
        }
        stateMachine.ChangeState(new TurnEndState());
    }
}

