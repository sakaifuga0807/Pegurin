using System.Collections;
using System.Linq.Expressions;
using NUnit.Framework;
using UnityEngine;

//敵のターン:手前の敵から一体ずつ、射程内なら行動、射程外なら移動。
//全員の攻撃が終わってから、攻撃の合計をまとめてプレイヤーのHPから引く。
//節目ごとに演出を再生して、終わるまで待つ。
public class EnemyTurnState : IBattleState
{
    public void Enter(BattleStateMachine stateMachine)
    {
        BattleDirector.Instance.Run(Routine(stateMachine));
    }

    private IEnumerator Routine(BattleStateMachine stateMachine)
    {
        var director = BattleDirector.Instance;
        var context = BattleContext.Current;
        var lane = context.EnemyLane;
        int totalDamage = 0;

        //最初に手前からのリストを固定する。
        //コピーなので、このターンに召喚された敵は処理に混ざらない。
        var enemies = lane.GetEnemiesFrontToBack();

        foreach (var enemy in enemies)
        {
            if (enemy.IsInRange)
            {
                //射程内:行動する。行動パターンを1つ進める。
                var action = enemy.CurrentAction;
                if (action == null) continue;

                //実行する前に予告を取る。
                var intent = action.GetIntent(enemy, lane);

                int damage = action.Execute(enemy, lane);
                totalDamage += damage;
                enemy.AdvanceAction();
                lane.NotifyIntentChanged();
                PlayActionSe(intent);
                Debug.Log($"{enemy.Data.EnemyName}（マス{enemy.CurrentCell}）が行動：ダメージ{damage}");


                yield return director.Play(BeatFor(intent.Type), new EnemyActionBeat(enemy, intent, damage));
                if (stateMachine.CurrentState != this) yield break;
            }
            else
            {
                //射程外:移動する。
                int from = enemy.CurrentCell;
                int moved = lane.Advance(enemy);
                Debug.Log($"{enemy.Data.EnemyName} が{moved}マス移動 → マス{enemy.CurrentCell}");

                if (moved > 0)
                {
                    SE.Play(SeCue.EnemyMove);
                    yield return director.Play(Beat.EnemyMove, new EnemyMoveBeat(enemy, from, enemy.CurrentCell));
                    if (stateMachine.CurrentState != this) yield break;
                }
            }
        }

        lane.NotifyIntentChanged();

        //全員の処理が終わってから、攻撃の合計をまとめてHPから引く。
        var player = context.Player;
        if (totalDamage > 0)
        {
            SE.Play(SeCue.PlayerDamaged);
            player.TakeDamage(totalDamage);

            //HPが0ならここで止まる。
            if (player.IsDead)
            {
                Debug.Log($"敵のターン終了：合計{totalDamage}ダメージ。プレイヤーHP:0");
                yield break;
            }

            yield return director.Play(Beat.PlayerHit, new PlayerHitBeat(totalDamage, player.CurrentHp));
            if (stateMachine.CurrentState != this) yield break;
        }
        Debug.Log($"敵のターン終了：合計{totalDamage}ダメージ。プレイヤーHP:{player.CurrentHp}");

        stateMachine.ChangeState(new TurnEndState());
    }

    //行動の種類に対応する節目。
    private static Beat BeatFor(EnemyIntentType type)
    {
        switch (type)
        {
            case EnemyIntentType.Charge: return Beat.EnemyCharge;
            case EnemyIntentType.Summon: return Beat.EnemySummon;
            default: return Beat.EnemyAttack;
        }
    }

    //行動の種類に合わせてSEを鳴らす。
    private static void PlayActionSe(EnemyIntent intent)
    {
        switch (intent.Type)
        {
            case EnemyIntentType.Attack: SE.Play(SeCue.EnemyAttack); break;
            case EnemyIntentType.HeavyAttack: SE.Play(SeCue.EnemyHeavyAttack); break;
            case EnemyIntentType.Summon: SE.Play(SeCue.EnemySummon); break;
        }
    }
}