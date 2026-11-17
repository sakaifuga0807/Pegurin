using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

//ダメージ確定:弾が落ちた、ダメージの確定、手前の敵にヒット、撃破、クリア判定、敵のターンへ。
//節目ごとに演出を再生して、終わるまで待つ。(演出の登録がなかったら、短い待ち時間だけ)。
public class ResolutionState : IBattleState
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
        var calculator = context.DamageCalculator;

        //玉が落ちた直後。
        yield return director.Play(Beat.OrbLanded);
        if (stateMachine.CurrentState != this) yield break;

        //最終ダメージの確定。
        int totalDamage = calculator.GetTotalDamage();
        yield return director.Play(Beat.DamageTally,
        new DamageTallyBeat(calculator.AccumulatedDamage, calculator.Multiplier, totalDamage));
        if (stateMachine.CurrentState != this) yield break;

        //一番手前の敵一体だけに与える。
        Enemy target = lane.ApplyDamageToFront(totalDamage);
        Debug.Log($"合計ダメージ{totalDamage} → {(target != null ? target.Data.EnemyName + " 残HP:" + target.CurrentHp : "対象なし")}");

        if (target != null && totalDamage > 0) SE.Play(SeCue.DamageDealt);

        if (target != null)
        {
            yield return director.Play(Beat.HitEnemy, new HitEnemyBeat(target, totalDamage, target.IsDead));
            if (stateMachine.CurrentState != this) yield break;
        }

        //倒れていたら、撃破の演出のあとで、レーンから外す。
        if (target != null && target.IsDead)
        {
            SE.Play(SeCue.EnemyDefeated);
            yield return director.Play(Beat.EnemyDefeated, new HitEnemyBeat(target, totalDamage, true));
            if (stateMachine.CurrentState != this) yield break;

            lane.RemoveDefeated();
        }

        //ステージクリアなら、敵のターンは行わない。
        if (context.RunManager.TryCompleteStage()) yield break;

        stateMachine.ChangeState(new EnemyTurnState());
    }
}