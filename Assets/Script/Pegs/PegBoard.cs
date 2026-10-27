using UnityEngine;
using System.Collections.Generic;

public class PegBoard : MonoBehaviour
{
    private List<Peg> allPegs = new List<Peg>();

    private void Awake()
    {
        Peg[] foundPegs = GetComponentsInChildren<Peg>(true);
        allPegs = new List<Peg>(foundPegs);
    }

    /// <summary>
    /// 盤面のペグを全復活させる
    /// </summary>
    public void RefreshAllPegs()
    {
        foreach (var peg in allPegs)
        {
            if (peg != null)
            {
                peg.ResetPegState();
            }
        }
    }

    //有効なペグの数。
    public int ActivePegCount
    {
        get
        {
            int count = 0;
            foreach (var peg in allPegs)
            {
                if (peg != null && peg.gameObject.activeSelf) count++;
            }
            return allPegs.Count;
        }
    }

    //盤面のペグが0この時だけ全復活させる。
    public void RestoreAllIfEmpty()
    {
        if(ActivePegCount==0)
        {
            RefreshAllPegs();
        }
    }

    private void Start()
    {
        var context = BattleContext.Current;
        if(context!=null&&context.RunManager!=null)
        {
            context.RunManager.OnStageStarted += HandleStageStarted;
        }
    }

    private void OnDestroy()
    {
        var context = BattleContext.Current;
        if(context!=null&&context.RunManager!=null)
        {
            context.RunManager.OnStageStarted -= HandleStageStarted;
        }
    }

    //ステージ開始時にペグを全復活させる。
    private void HandleStageStarted(int stageIndex,StageConfig stage)
    {
        RefreshAllPegs();
    }
}