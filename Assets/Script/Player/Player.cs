using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//プレイヤーのHPを管理するクラス。
//HPはステージを跨いでも引き継がれる。
public class Player : MonoBehaviour
{
    public int MaxHp => GameConstants.PlayerMaxHp;
    public int CurrentHp { get; private set; }
    public bool IsDead => CurrentHp <= 0;

    //引数はCurrentHp,MaxHp。戻り値は持たない。
    public event Action<int, int> OnHpChanged;
    //死亡時に呼ばれるイベント。
    public event Action OnDied;

    private void Awake()
    {
        ResetForRun();
    }

    //ラン開始時だけ呼び、HPを最大値にする。
    public void ResetForRun()
    {
        CurrentHp = MaxHp;
        OnHpChanged?.Invoke(CurrentHp, MaxHp);
    }

    //ダメージを受ける処理。HPは0未満にならない。死亡時にOnDiedイベントを呼ぶ。
    public void TakeDamage(int amount)
    {
        //早期リターン。ダメージが0以下、またはすでに死亡している場合は処理しない。
        if (amount <= 0 || IsDead) return;

        CurrentHp = Mathf.Max(0, CurrentHp - amount);
        OnHpChanged?.Invoke(CurrentHp, MaxHp);

        if(IsDead)
        {
            OnDied?.Invoke();
        }
    }

    //回復処理。HPは最大値を超えない。
    public void Heal(int amount)
    {
        //早期リターン。回復量が0以下、またはすでに死亡している場合は処理しない。
        if (amount <= 0 || IsDead) return;

        CurrentHp = Mathf.Min(MaxHp, CurrentHp + amount);
        OnHpChanged?.Invoke(CurrentHp, MaxHp);
    }

    //ステージクリアーで回復。
    public void HealOnStageClear()
    {
        Heal(GameConstants.StageClearHeal);
    }
}

