using UnityEngine;
using System.Collections.Generic;

public class PegBoard : MonoBehaviour
{
    [System.Serializable]
    public struct PegEffectData
    {
        public PegEffect effect;
        public Color pegColor;
        [Range(0f, 100f)] public float spawnWeight;
    }

    [Header("ランダム配置の設定")]
    [SerializeField] private List<PegEffectData> effectPool = new List<PegEffectData>();

    [Header("配置ルール")]
    [SerializeField] private float topRowTolerance = 0.1f;

    private List<Peg> allPegs = new List<Peg>();

    private void Awake()
    {
        Peg[] foundPegs = GetComponentsInChildren<Peg>(true);
        allPegs = new List<Peg>(foundPegs);

        SetupPegTags();

        AssignRandomEffectsToAllPegs();
    }

    private void SetupPegTags()
    {
        foreach (var peg in allPegs)
        {
            if (peg != null)
            {
                peg.gameObject.tag = "Peg";
            }
        }
    }

    public void RefreshAllPegs(Peg excludePeg = null)
    {
        foreach (var peg in allPegs)
        {
            if (peg != null && peg != excludePeg)
            {
                peg.ResetPegState();
            }
        }
        EnsureRefreshPeg(excludePeg);
    }

    public void RestoreAllIfEmpty()
    {
        if (ActivePegCount == 0)
        {
            RefreshAllPegs();
        }
    }

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

    public void AssignRandomEffectsToAllPegs()
    {
        if (effectPool == null || effectPool.Count == 0) return;

        float topY = GetTopY();

        foreach (var peg in allPegs)
        {
            if (peg == null) continue;

            bool isTop = IsTopRow(peg, topY);
            PegEffectData chosenData = SelectRandomEffectData(isTop);
            peg.SetPegEffect(chosenData.effect, chosenData.pegColor);
        }

        EnsureRefreshPeg(null);
    }

    private PegEffectData SelectRandomEffectData(bool excludeRefresh)
    {
        float totalWeight = 0f;
        foreach (var data in effectPool)
        {
            if (excludeRefresh && IsRefreshEffect(data.effect)) continue;
            totalWeight += data.spawnWeight;
        }

        // 候補の重みが全て0のときの保険。
        if (totalWeight <= 0f)
        {
            foreach (var data in effectPool)
            {
                if (!excludeRefresh || !IsRefreshEffect(data.effect)) return data;
            }
            return effectPool[0];
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeightSum = 0f;

        foreach (var data in effectPool)
        {
            if (excludeRefresh && IsRefreshEffect(data.effect)) continue;

            currentWeightSum += data.spawnWeight;
            if (randomValue <= currentWeightSum)
            {
                return data;
            }
        }
        return effectPool[0];
    }

    private void EnsureRefreshPeg(Peg excludePeg)
    {
        int refreshIndex = FindRefreshPoolIndex();
        if (refreshIndex < 0) return;

        float topY = GetTopY();
        List<Peg> candidates = new List<Peg>();

        foreach (var peg in allPegs)
        {
            if (peg == null || peg == excludePeg) continue;

            // すでにリフレッシュペグがあるなら、何もしない。
            if (peg.IsRefresh) return;

            if (!IsTopRow(peg, topY)) candidates.Add(peg);
        }

        if (candidates.Count == 0) return;

        Peg chosen = candidates[Random.Range(0, candidates.Count)];
        PegEffectData data = effectPool[refreshIndex];
        chosen.SetPegEffect(data.effect, data.pegColor);

        Debug.Log("[PegBoard] リフレッシュペグが無かったので、1つ補いました。");
    }

    // プールの中で、リフレッシュの項目の番号を返す。無ければ -1。
    private int FindRefreshPoolIndex()
    {
        for (int i = 0; i < effectPool.Count; i++)
        {
            if (IsRefreshEffect(effectPool[i].effect)) return i;
        }
        return -1;
    }

    private static bool IsRefreshEffect(PegEffect effect)
    {
        return effect is RefreshPegEffect;
    }

    // 盤面で一番高いペグの高さ(ワールド座標のY)。
    private float GetTopY()
    {
        float topY = float.MinValue;
        foreach (var peg in allPegs)
        {
            if (peg == null) continue;
            if (peg.transform.position.y > topY) topY = peg.transform.position.y;
        }
        return topY;
    }

    // 「一番上の行」のペグかどうか。
    private bool IsTopRow(Peg peg, float topY)
    {
        return peg.transform.position.y >= topY - topRowTolerance;
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

    // ステージ開始時に、属性を振り直して、ペグを全復活させる。
    private void HandleStageStarted(int stageIndex, StageConfig stage)
    {
        AssignRandomEffectsToAllPegs();   
        RefreshAllPegs();    
    }
}