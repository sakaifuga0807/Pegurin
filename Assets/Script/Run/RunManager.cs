using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//ペグのレイアウトの種類。
public enum PegLayoutType { VShape, Diamond, Grid }

//ランの進行段階。
public enum RunPhase { Title, Battle, Reward, Map, Transition, Cleared, GameOver }

//ステージ一つ分の構成。
[Serializable]
public class StageConfig
{
    public string stageName;
    public PegLayoutType layout;
    public List<EnemyData> enemies = new List<EnemyData>();
    public bool isBoss;
    public EnemyData bossData;
}

//ラン全体の進行管理。
public class RunManager : MonoBehaviour
{
    [Header("ステージ構成")]
    [SerializeField] private List<StageConfig> stages = new List<StageConfig>();

    [Header("報酬画面に並べるオーブ")]
    [SerializeField] private List<OrbData> rewardOrbOptions = new List<OrbData>();

    [SerializeField] private BattleStateMachine stateMachine;

    [Header("確認用")]
    [Tooltip("タイトル画面ができるまでの確認用")]
    [SerializeField] private bool autoStart = true;

    public event Action OnRunStarted;           //ラン開始。
    public event Action OnReturnedToTitle;      //タイトルへ戻る。

    public RunPhase Phase { get; private set; } = RunPhase.Title;
    public int StageIndex { get; private set; }

    public StageConfig CurrentStage =>
        (StageIndex >= 0 && StageIndex < stages.Count) ? stages[StageIndex] : null;

    public event Action<int, StageConfig> OnStageStarted;
    public event Action<List<OrbData>, List<PassiveEffect>, int> OnRewardStarted;//オーブ候補。
    public event Action OnRunCleared;
    public event Action OnRunGameOver;

    private List<OrbData> currentOrbOptions = new List<OrbData>();
    private List<PassiveEffect> currentPassiveOptions = new List<PassiveEffect>();
    private bool orbChosen;
    private bool passiveChosen;

    private void Start()
    {
        BattleContext.Current.Player.OnDied += HandlePlayerDied;
        if (autoStart) BeginRun();
    }

    private void OnDestroy()
    {
        if (BattleContext.Current != null && BattleContext.Current.Player != null)
        {
            BattleContext.Current.Player.OnDied -= HandlePlayerDied;
        }
    }

    //タイトルのスタートボタンから呼ぶ。
    public void StartRun()
    {
        if (Phase != RunPhase.Title) return;
        SE.Play(SeCue.UiConfirm);
        BeginRun();
    }

    //ランとステージの開始。
    public void BeginRun()
    {
        //タイトル以外では開始しない。
        if (Phase != RunPhase.Title) return;

        if (stages.Count == 0)
        {
            Debug.LogError("RunManagerのStagesが空です。");
            return;
        }
        var context = BattleContext.Current;
        StageIndex = 0;
        context.Player.ResetForRun();
        if (context.PassiveManager != null)
        {
            context.PassiveManager.ResetForRun();
        }
        context.DeckManager.ResetForRun();

        OnRunStarted?.Invoke();
        StartStage();
    }

    //今やり直し・タイトルへ戻るができるか。
    public bool CanAbort => Phase != RunPhase.Title;

    //ゲームクリア・オーバー画面のもう一度から呼ぶ。
    public void RestartRun()
    {
        if (!CanAbort) return;

        SE.Play(SeCue.UiConfirm);
        AbortBattle();
        GameSpeed.SetPaused(false);     //ポーズ中に押されたときのため。

        Phase = RunPhase.Title;
        BeginRun();
    }

    //ゲームクリア・オーバー画面のタイトル、ポーズ画面の法規から呼ぶ。どのフェーズでも動く。
    public void ReturnToTitle()
    {
        if (!CanAbort) return;

        SE.Play(SeCue.UiConfirm);
        AbortBattle();
        GameSpeed.ResetAll();       //タイトル時は等倍・ポーズなしに戻す。

        Phase = RunPhase.Title;
        Debug.Log("タイトルへ戻りました");
        OnReturnedToTitle?.Invoke();
    }

    //進行中のものを全部止める。
    private void AbortBattle()
    {
        //演出の待ちを全部止める。
        var director = BattleDirector.Existing;
        if (director != null) director.CancelAll();

        OrbTracker.DestroyAll();
        stateMachine.Stop();
        BattleContext.Current.EnemyLane.Clear();

        //報酬の選択の途中だった文を捨てる。
        currentOrbOptions.Clear();
        currentPassiveOptions.Clear();
        orbChosen = false;
        passiveChosen = false;
    }

    //ステージ開始。
    private void StartStage()
    {
        var context = BattleContext.Current;
        var stage = CurrentStage;

        Phase = RunPhase.Transition;
        context.EnemyLane.Setup(stage.enemies);
        context.DeckManager.InitializeDeck();

        Debug.Log($"ステージ{StageIndex + 1}開始:{stage.stageName}(レイアウト：{stage.layout})");

        //ボスのステージの開始は、登場のジングルを鳴らす。
        if (stage.isBoss) SE.Play(SeCue.BossAppear);
        OnStageStarted?.Invoke(StageIndex, stage);

        BattleDirector.Instance.Run(StageIntroRoutine(stage));
    }

    private IEnumerator StageIntroRoutine(StageConfig stage)
    {
        var director = BattleDirector.Instance;
        if (stage.isBoss) yield return director.Play(Beat.BossAppear, new StageBeat(StageIndex, stage));
        yield return director.Play(Beat.StageIntro, new StageBeat(StageIndex, stage));

        Phase = RunPhase.Battle;
        stateMachine.StartBattle();
    }

    //クリア判定。クリアなら、クリア演出から報酬までの進行を初めて、Trueを返す。
    public bool TryCompleteStage()
    {
        if (Phase != RunPhase.Battle)
        {
            return false;
        }

        var context = BattleContext.Current;
        var lane = context.EnemyLane;
        var stage = CurrentStage;

        //ボスステージ。ボスだけ倒すとクリア判定。
        bool cleared;
        if (stage.isBoss && stage.bossData != null)
        {
            cleared = !lane.Contains(stage.bossData);
        }
        else
        {
            if (stage.isBoss)
            {
                Debug.LogError("ボスステージなのにBossDataが未設定なため全滅判定を返します。");
            }
            cleared = lane.IsEmpty;
        }
        if (!cleared)
        {
            return false;
        }

        SE.Play(SeCue.StageClear);

        //ボス撃破。残りの敵を消す。
        if (stage.isBoss)
        {
            lane.Clear();
        }

        //遷移中にする。
        Phase = RunPhase.Transition;
        BattleDirector.Instance.Run(CompleteStageRoutine(stage));
        return true;
    }

    private IEnumerator CompleteStageRoutine(StageConfig stage)
    {
        var director = BattleDirector.Instance;
        var context = BattleContext.Current;

        //クリア演出。
        yield return director.Play(Beat.StageClear, new StageBeat(StageIndex, stage));

        //最終ステージ、または最後のステージ→ゲームクリア。
        if (stage.isBoss || StageIndex + 1 >= stages.Count)
        {
            Phase = RunPhase.Cleared;
            Debug.Log("ゲームクリア");
            OnRunCleared?.Invoke();
            yield break;
        }

        //HP回復と、その表示。
        int before = context.Player.CurrentHp;
        context.Player.HealOnStageClear();
        int healed = context.Player.CurrentHp - before;
        yield return director.Play(Beat.HealShown, new HealBeat(healed, context.Player.CurrentHp, context.Player.MaxHp));

        BeginReward(context, healed);
    }
    //報酬。
    private void BeginReward(BattleContext context, int healed)
    {
        //報酬の候補。オーブは必ず並ぶ。
        currentOrbOptions = new List<OrbData>(rewardOrbOptions);
        currentPassiveOptions = context.PassiveManager != null
            ? context.PassiveManager.GetRewardCandidates()
            : new List<PassiveEffect>();
        orbChosen = false;
        passiveChosen = false;

        Phase = RunPhase.Reward;
        Debug.Log($"ステージ{StageIndex + 1}クリア。HP+{healed}（{context.Player.CurrentHp}/{context.Player.MaxHp}）。報酬を選んでください。");
        OnRewardStarted?.Invoke(currentOrbOptions, currentPassiveOptions, healed);

        //選べるものが一つもなければ、そのまま次へ。
        TryFinishReward();
    }

    //報酬画面でオーブを選んだ時に呼ぶ。
    public void SelectRewardOrb(OrbData orb)
    {
        if (Phase != RunPhase.Reward || orbChosen || !currentOrbOptions.Contains(orb))
        {
            return;
        }

        BattleContext.Current.DeckManager.AddOrbToDeck(orb);
        orbChosen = true;
        SE.Play(SeCue.RewardSelect);//SE:報酬選択。
        Debug.Log($"オーブを獲得:{orb.orbName}");
        TryFinishReward();
    }

    //報酬画面でパッシブを選んだ時に呼ぶ。
    public void SelectRewardPassive(PassiveEffect passive)
    {
        if (Phase != RunPhase.Reward || passiveChosen || !currentPassiveOptions.Contains(passive))
        {
            return;
        }

        BattleContext.Current.PassiveManager.Add(passive);
        passiveChosen = true;
        SE.Play(SeCue.RewardSelect);//SE:報酬選択。
        Debug.Log($"パッシブを獲得。{passive.PassiveName}");
        TryFinishReward();
    }

    //選ぶべきものを選び終えたら次のステージへ。
    //候補がない場合は選択しなくても良い。
    private void TryFinishReward()
    {
        bool orbDone = orbChosen || currentOrbOptions.Count == 0;
        bool passiveDone = passiveChosen || currentPassiveOptions.Count == 0;
        if (orbDone && passiveDone)
        {
            GoToNextStage();
        }
    }

    //次のステージへ遷移処理。
    private void GoToNextStage()
    {
        Phase = RunPhase.Transition;
        BattleDirector.Instance.Run(NextStageRoutine());
    }

    private IEnumerator NextStageRoutine()
    {
        //マップ演出。今は待ち時間だけ。
        yield return BattleDirector.Instance.Play(Beat.MapTransition, new StageBeat(StageIndex, CurrentStage));

        StageIndex++;
        StartStage();
    }

    //ゲームオーバー。
    private void HandlePlayerDied()
    {
        if (Phase == RunPhase.GameOver || Phase == RunPhase.Title)
        {
            return;
        }

        Phase = RunPhase.GameOver;
        BattleDirector.Instance.Run(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        SE.Play(SeCue.GameOver);
        yield return BattleDirector.Instance.Play(Beat.GameOver);

        Debug.Log("ゲームオーバー");
        OnRunGameOver?.Invoke();
    }

    [ContextMenu("テスト:報酬の先頭を選ぶ")]
    private void TestPickFirstRewards()
    {
        if (currentOrbOptions.Count > 0)
        {
            SelectRewardOrb(currentOrbOptions[0]);
        }
        if (currentPassiveOptions.Count > 0)
        {
            SelectRewardPassive(currentPassiveOptions[0]);
        }
    }

    [ContextMenu("テスト:スタート")]
    private void TestStart() { StartRun(); }
    [ContextMenu("テスト:ゲームオーバーにする")]
    private void TestGameOver() { BattleContext.Current.Player.TakeDamage(99999); }
    [ContextMenu("テスト:もう一度")]
    private void TestRestart() { RestartRun(); }
    [ContextMenu("テスト:タイトルへ")]
    private void TestReturnToTitle() { ReturnToTitle(); }
}
