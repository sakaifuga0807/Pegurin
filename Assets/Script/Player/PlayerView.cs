using System.Collections;
using UnityEngine;
using Spine.Unity;


//プレイヤー（ナイト）の見た目。SpineのSkeletonAnimationを操作する。
public class PlayerView : MonoBehaviour
{
    [Header("つなぐもの(空ならBattleContextから取る)")]
    [SerializeField] private Player player;
    [SerializeField] private BattleStateMachine stateMachine;
    [SerializeField] private RunManager runManager;
    [Tooltip("空なら、このオブジェクトのSkeletonAnimationを使う")]
    [SerializeField] private SkeletonAnimation skeletonAnimation;

    [Header("Spineのアニメーション名")]
    [SerializeField] private string idleAnimation = "idle";
    [SerializeField] private string attackAnimation = "jump2";
    [SerializeField] private string hitAnimation = "down";
    [SerializeField] private string dieAnimation = "die2";
    [SerializeField] private string victoryAnimation = "jump";

    [Header("動きの設定")]
    [Tooltip("攻撃・ダメージの動きの後に待機状態に戻るまでの秒数")]
    [SerializeField] private float returnToIdleSeconds = 0.5f;

    [Header("確認用")]
    [SerializeField] private bool logStates = false;

    private int lastHp = -1;
    private bool dead;
    private Coroutine returnRoutine;

    private void Awake()
    {
        if (skeletonAnimation == null) skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
    }

    private void Start()
    {
        var context = BattleContext.Current;
        if (context != null)
        {
            if (player == null) player = context.Player;
            if (stateMachine == null) stateMachine = context.BattleStateMachine;
            if (runManager == null) runManager = context.RunManager;
        }

        if (skeletonAnimation == null)
        {
            Debug.LogWarning("PlayerView:SkeletonAnimationがないです。");
            return;
        }

        if (player != null) { player.OnHpChanged += OnHpChanged; player.OnDied += OnDied; }
        if (stateMachine != null) stateMachine.Launched += OnLaunched;
        if (runManager != null)
        {
            runManager.OnRunStarted += OnReset;
            runManager.OnReturnedToTitle += OnReset;
            runManager.OnRunCleared += OnCleared;
        }
        PlayLoop(idleAnimation);
    }

    private void OnDestroy()
    {
        if (player != null) { player.OnHpChanged -= OnHpChanged; player.OnDied -= OnDied; }
        if (stateMachine != null) stateMachine.Launched -= OnLaunched;
        if (runManager != null)
        {
            runManager.OnRunStarted -= OnReset;
            runManager.OnReturnedToTitle -= OnReset;
            runManager.OnRunCleared -= OnCleared;
        }
    }

    //HPが減った時。
    private void OnHpChanged(int current, int max)
    {
        int previous = lastHp;
        lastHp = current;

        if (previous < 0) return;
        if (current <= 0) return;
        if (current < previous) PlayOneShot(hitAnimation);
    }

    private void OnDied()
    {
        dead = true;
        StopReturn();
        PlayLoop(dieAnimation);
    }

    private void OnLaunched() { PlayOneShot(attackAnimation); }

    private void OnCleared()
    {
        StopReturn();
        PlayLoop(victoryAnimation);
    }

    //ラン開始、タイトルへ戻った時に待機状態に戻す。
    private void OnReset()
    {
        dead = false;
        lastHp = -1;
        StopReturn();
        PlayLoop(idleAnimation);
    }

    //一回だけ再生して、少ししたら待機状態に戻す。
    private void PlayOneShot(string animName)
    {
        if (dead || string.IsNullOrEmpty(animName)) return;

        PlayAnimation(animName, false);
        StopReturn();
        returnRoutine = StartCoroutine(ReturnToIdleRoutine());
    }

    //待機に戻らない動き（待機、やられ、勝利）。
    private void PlayLoop(string animName)
    {
        if (string.IsNullOrEmpty(animName)) return;
        PlayAnimation(animName, true);
    }

    private IEnumerator ReturnToIdleRoutine()
    {
        yield return new WaitForSeconds(returnToIdleSeconds);
        returnRoutine = null;
        if (!dead) PlayLoop(idleAnimation);
    }

    private void StopReturn()
    {
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }
    }

    //Animatorの状態を、名前で再生する。
    private void PlayAnimation(string animName, bool loop)
    {
        if (skeletonAnimation == null || string.IsNullOrEmpty(animName)) return;

        skeletonAnimation.AnimationState.SetAnimation(0, animName, loop);
        if (logStates) Debug.Log($"[Player Animation]{animName}(Loop:{loop})");
    }

    //確認用。
    [ContextMenu("テスト:待機")] private void TestIdle() { dead = false; PlayLoop(idleAnimation); }
    [ContextMenu("テスト:攻撃")] private void TestAttack() { PlayOneShot(attackAnimation); }
    [ContextMenu("テスト:ダメージ")] private void TestHit() { PlayOneShot(hitAnimation); }
    [ContextMenu("テスト:やられ")] private void TesDie() { OnDied(); }
    [ContextMenu("テスト:勝利")] private void TestVictory() { OnCleared(); }
}