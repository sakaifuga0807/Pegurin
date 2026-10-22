using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

//ペグのレイアウトの種類。
public enum PegLayoutType { VShape,Diamond,Grid}

//ランの進行段階。
public enum RunPhase { Title,Battle,Reward,Map,Cleared,GameOver}

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
public class RunManager:MonoBehaviour
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
        if(autoStart) StartRun();
    }

    private void OnDestroy()
    {
        if(BattleContext.Current!=null&&BattleContext.Current.Player!=null)
        {
            BattleContext.Current.Player.OnDied -= HandlePlayerDied;
        }
    }

    //ランとステージの開始。
    public void StartRun()
    {
        //タイトル以外では開始しない。
        if (Phase != RunPhase.Title) return;

        if(stages.Count==0)
        {
            Debug.LogError("RunManagerのStagesが空です。");
            return;
        }
        var context = BattleContext.Current;
        StageIndex = 0;
        context.Player.ResetForRun();
        if(context.PassiveManager!=null)
        {
            context.PassiveManager.ResetForRun();
        }
        context.DeckManager.ResetForRun();

        OnRunStarted?.Invoke();
        StartStage();
    }

    //ゲームクリア・オーバー画面のもう一度から呼ぶ。
    public void RestartRun()
    {
        if (Phase != RunPhase.Cleared && Phase != RunPhase.GameOver) return;

        Phase = RunPhase.Title;
        StartRun();
    }

    //ゲームクリア・オーバー画面のタイトルへから呼ぶ。
    public void ReturnToTitle()
    {
        if (Phase != RunPhase.Cleared && Phase != RunPhase.GameOver) return;

        stateMachine.Stop();
        BattleContext.Current.EnemyLane.Clear();
        Phase = RunPhase.Title;
        Debug.Log("タイトルへ戻りました");
        OnReturnedToTitle?.Invoke();
    }

    //ステージ開始。
    private void StartStage()
    {
        var context = BattleContext.Current;
        var stage = CurrentStage;

        Phase = RunPhase.Battle;
        context.EnemyLane.Setup(stage.enemies);
        context.DeckManager.InitializeDeck();

        Debug.Log($"ステージ{StageIndex + 1}開始:{stage.stageName}(レイアウト：{stage.layout})");
        OnStageStarted?.Invoke(StageIndex, stage);

        stateMachine.StartBattle();
    }

    //クリア判定。
    public bool TryCompleteStage()
    {
        if(Phase!=RunPhase.Battle)
        {
            return false;
        }

        var context = BattleContext.Current;
        var lane = context.EnemyLane;
        var stage = CurrentStage;

        //ボスステージ。ボスだけ倒すとクリア判定。
        bool cleared;
        if(stage.isBoss&&stage.bossData!=null)
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

        SoundPlayer.PlayCue(SeCue.StageClear);

        //ボス撃破。残りの敵を消す。
        if(stage.isBoss)
        {
            lane.Clear();
        }

        //最終ステージクリアで、ゲームクリア。
        if (stage.isBoss || StageIndex + 1 >= stages.Count)
        {
            Phase = RunPhase.Cleared;
            Debug.Log("ゲームクリア");
            OnRunCleared?.Invoke();
            return true;
        }

        BeginReward(context);
        return true;
    }

    //報酬。
    private void BeginReward(BattleContext context)
    {
        //HPの回復をする。
        int before = context.Player.CurrentHp;
        context.Player.HealOnStageClear();
        int healed = context.Player.CurrentHp - before;

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
        if(Phase!=RunPhase.Reward||orbChosen||!currentOrbOptions.Contains(orb))
        {
            return;
        }

        BattleContext.Current.DeckManager.AddOrbToDeck(orb);
        orbChosen = true;
        SoundPlayer.PlayCue(SeCue.RewardSelect);//SE:報酬選択。
        Debug.Log($"オーブを獲得:{orb.orbName}");
        TryFinishReward();
    }

    //報酬画面でパッシブを選んだ時に呼ぶ。
    public void SelectRewardPassive(PassiveEffect passive)
    {
        if(Phase!=RunPhase.Reward||passiveChosen||!currentPassiveOptions.Contains(passive))
        {
            return;
        }

        BattleContext.Current.PassiveManager.Add(passive);
        passiveChosen = true;
        SoundPlayer.PlayCue(SeCue.RewardSelect);//SE:報酬選択。
        Debug.Log($"パッシブを獲得。{passive.PassiveName}");
        TryFinishReward();
    }

    //選ぶべきものを選び終えたら次のステージへ。
    //候補がない場合は選択しなくても良い。
    private void TryFinishReward()
    {
        bool orbDone = orbChosen || currentOrbOptions.Count == 0;
        bool passiveDone = passiveChosen || currentPassiveOptions.Count == 0;
        if(orbDone&&passiveDone)
        {
            GoToNextStage();
        }
    }

    //次のステージへ遷移処理。
    private void GoToNextStage()
    {
        Phase = RunPhase.Map;
        //TODO:マップの演出を入れる。
        StageIndex++;
        StartStage();
    }

    //ゲームオーバー。
    private void HandlePlayerDied()
    {
        if(Phase==RunPhase.GameOver)
        {
            return;
        }

        Phase = RunPhase.GameOver;
        SoundPlayer.PlayCue(SeCue.GameOver);//SE:ゲームオーバー。
        Debug.Log("ゲームオーバー");
        OnRunGameOver?.Invoke();
    }

    [ContextMenu("テスト:報酬の先頭を選ぶ")]
    private void TestPickFirstRewards()
    {
        if(currentOrbOptions.Count>0)
        {
            SelectRewardOrb(currentOrbOptions[0]);
        }
        if(currentPassiveOptions.Count>0)
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
