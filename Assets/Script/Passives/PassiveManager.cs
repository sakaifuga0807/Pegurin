using System;
using System.Collections.Generic;
using UnityEngine;

//所持パッシブの管理と報酬画面にだす用。
public class PassiveManager:MonoBehaviour
{
    [Header("パッシブ")]
    [SerializeField] private List<PassiveEffect> allPassives = new List<PassiveEffect>();

    private readonly List<PassiveEffect> owned = new List<PassiveEffect>();
    private bool safetyNetUsedThisTurn;//安全ネットをこのターンに使ったか。
    public IReadOnlyList<PassiveEffect> Owned => owned;

    //所持に変更があった時に通知。
    public event Action OnChanged;

    public bool Has(PassiveEffect passive) => owned.Contains(passive);

    //入手する。同じパッシブは重複しないように。
    public void Add(PassiveEffect passive)
    {
        if (passive == null || Has(passive)) return;
        owned.Add(passive);
        OnChanged?.Invoke();
    }

    //報酬の候補。持ってないものから最大3つ。
    public List<PassiveEffect> GetRewardCandidates()
    {
        var pool = new List<PassiveEffect>();
        foreach(var passive in allPassives)
        {
            if (passive != null && !Has(passive)) pool.Add(passive);
        }

        //シャッフルして先頭から最大3つ。
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int rand = UnityEngine.Random.Range(0, i + 1);
            (pool[i], pool[rand]) = (pool[rand], pool[i]);
        }
        if (pool.Count > 3) pool.RemoveRange(3, pool.Count - 3);
        return pool;
    }

    //ゲーム開始時に所持を空にする。
    public void ResetForRun()
    {
        owned.Clear();
        OnChanged?.Invoke();
    }

    //通常ペグの基礎ダメージ。
    public int ModifyNormalPegDamage(int baseDamage)
    {
        foreach(var passive in owned)
        {
            baseDamage = passive.ModifyNormalPegDamage(baseDamage);
        }
        return baseDamage;
    }

    //リフレッシュペグが発動したときに呼ぶ。
    public void NotifyRefreshPegTriggered()
    {
        var context = BattleContext.Current;
        foreach(var passive in owned)
        {
            passive.OnRefreshPegTriggered(context);
        }
    }

    /*
     * 落下したときに跳ね返すか。
     * バイブスは最初に落ちた1個にしか効果がないようにする。
     */
    public bool TryUseSafetyNet()
    {
        if (safetyNetUsedThisTurn) return false;

        foreach(var passive in owned)
        {
            if(passive.BlocksFallOncePerTurn)
            {
                safetyNetUsedThisTurn = true;
                return true;
            }
        }
        return false;
    }

    //マグネットを使っているか。
    public bool HasMagnet
    {
        get
        {
            foreach(var passive in owned)
            {
                if (passive.EnableMagnet) return true;
            }
            return false;
        }
    }

    //ターン開始時に呼ぶ。
    public void ResetTurn()
    {
        safetyNetUsedThisTurn = false;
    }
}
