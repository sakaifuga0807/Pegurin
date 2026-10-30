using UnityEngine;
using System.Collections.Generic;

public class PegBoard : MonoBehaviour
{
    // エフェクトと、それに対応する見た目の色をまとめる構造体
    [System.Serializable]
    public struct PegEffectData
    {
        public PegEffect effect;
        public Color pegColor;
        [Range(0f, 100f)] public float spawnWeight; // 出現の重み（確率）
    }

    [Header("ランダム配置の設定")]
    [SerializeField] private List<PegEffectData> effectPool = new List<PegEffectData>();

    private List<Peg> allPegs = new List<Peg>();

    private void Awake()
    {
        Peg[] foundPegs = GetComponentsInChildren<Peg>(true);
        allPegs = new List<Peg>(foundPegs);

        // ボードが読み込まれたときにランダムでエフェクトを割り振る
        AssignRandomEffectsToAllPegs();
    }

    /// <summary>
    /// 盤面のペグを全復活させる
    /// </summary>
    public void RefreshAllPegs(Peg excludePeg = null)
    {
        foreach (var peg in allPegs)
        {
            if (peg != null && peg != excludePeg)
            {
                peg.ResetPegState();
            }
        }
    }

    /// <summary>
    /// 盤面のペグが0個の時だけ全復活させる
    /// </summary>
    public void RestoreAllIfEmpty()
    {
        if (ActivePegCount == 0)
        {
            RefreshAllPegs();
        }
    }

    // 有効なペグの数。
    public int ActivePegCount
    {
        get
        {
            int count = 0;
            foreach (var peg in allPegs)
            {
                if (peg != null && peg.gameObject.activeSelf) count++;
            }
            return count;
        }
    }

    /// <summary>
    /// 既存の全ペグに対して、重み付けに従ってランダムにエフェクトを割り当てる
    /// </summary>
    public void AssignRandomEffectsToAllPegs()
    {
        if (effectPool == null || effectPool.Count == 0) return;

        foreach (var peg in allPegs)
        {
            if (peg == null) continue;

            PegEffectData chosenData = SelectRandomEffectData();
            peg.SetPegEffect(chosenData.effect, chosenData.pegColor);
        }
    }

    private PegEffectData SelectRandomEffectData()
    {
        float totalWeight = 0f;
        foreach (var data in effectPool) totalWeight += data.spawnWeight;

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeightSum = 0f;

        foreach (var data in effectPool)
        {
            currentWeightSum += data.spawnWeight;
            if (randomValue <= currentWeightSum)
            {
                return data;
            }
        }
        return effectPool[0];
    }

    private void Start()
    {
        var context = BattleContext.Current;
        if (context != null && context.RunManager != null)
        {
            context.RunManager.OnStageStarted += HandleStageStarted;
        }
    }

    private void OnDestroy()
    {
        var context = BattleContext.Current;
        if (context != null && context.RunManager != null)
        {
            context.RunManager.OnStageStarted -= HandleStageStarted;
        }
    }

    // ステージ開始時にペグを全復活させる。
    private void HandleStageStarted(int stageIndex, StageConfig stage)
    {
        RefreshAllPegs();
    }
}