using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.Lumin;
using UnityEngine.Playables;

//プレイヤー（ナイト）の見た目。Playerなどのイベントを受けて、Animatorの状態を再生する。
//Player などのルール側は、この見た目を知らない。ナイトのオブジェクトに付ける。
public class PlayerView:MonoBehaviour
{
    [Header("つなぐもの(空ならBattleContextから取る)")]
    [SerializeField] private Player player;
    [SerializeField] private BattleStateMachine stateMachine;
    [SerializeField] private RunManager runManager;
    [Tooltip("空なら、このオブジェクトのAnimatorを使う")]
    [SerializeField] private Animator animator;

    [Header("Animatorの状態の名前")]
    [SerializeField] private string idleState = "idle";
    [SerializeField] private string attackState= "";
    [SerializeField] private string hitState = "down";
    [SerializeField] private string dieState = "die";
    [SerializeField] private string victoryState= "jump";

    [Header("動きの設定")]
    [Tooltip("攻撃・ダメージの動きの後に待機状態に戻るまでの秒数")]
    [SerializeField] private float returnToIdleSeconds = 0.5f;
    [Tooltip("動きを切り替えるときの滑らかさ")]
    [SerializeField] private float crossFadeSeconds = 0.05f;

    [Header("確認用")]
    [SerializeField] private bool logStates = false;

    private int lastHp = -1;
    private bool dead;
    private Coroutine returnRoutine;

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        var context = BattleContext.Current;
        if(context!=null)
        {
            if (player == null) player = context.Player;
            if (stateMachine == null) stateMachine = context.BattleStateMachine;
            if (runManager == null) runManager = context.RunManager;
        }

        if (animator==null)
        {
            Debug.LogWarning("PlayerView:Animatorがないです。");
            return;
        }

        if (player != null) { player.OnHpChanged += OnHpChanged;player.OnDied += OnDied; }
        if (stateMachine != null) stateMachine.Launched += OnLaunched;
        if(runManager!=null)
        {
            runManager.OnRunStarted += OnReset;
            runManager.OnReturnedToTitle += OnReset;
            runManager.OnRunCleared += OnCleared;
        }
        PlayLoop(idleState);
    }

    private void OnDestroy()
    {
        if (player!= null) { player.OnHpChanged -= OnHpChanged;player.OnDied -= OnDied; }
        if (stateMachine != null) stateMachine.Launched -= OnLaunched;
        if(runManager!=null)
        {
            runManager.OnRunStarted -= OnReset;
            runManager.OnReturnedToTitle -= OnReset;
            runManager.OnRunCleared -= OnCleared;
        }
    }

    //HPが減った時。
    private void OnHpChanged(int current,int max)
    {
        int previous = lastHp;
        lastHp = current;

        if (previous < 0) return;
        if (current <= 0) return;
        if (current < previous) PlayOneShot(hitState);
    }

    private void OnDied()
    {
        dead = true;
        StopReturn();
        PlayLoop(dieState);
    }

    private void OnLaunched() { PlayOneShot(attackState); }

    private void OnCleared()
    {
        StopReturn();
        PlayLoop(victoryState);
    }

    //ラン開始、タイトルへ戻った時に待機状態に戻す。
    private void OnReset()
    {
        dead = false;
        lastHp = -1;
        StopReturn();
        PlayLoop(idleState);
    }

    //一回だけ再生して、少ししたら待機状態に戻す。
    private void PlayOneShot(string state)
    {
        if (dead || string.IsNullOrEmpty(state)) return;

        PlayState(state);
        StopReturn();
        returnRoutine = StartCoroutine(ReturnToIdleRoutine());
    }

    //待機に戻らない動き（待機、やられ、勝利）。
    private void PlayLoop(string state)
    {
        if (string.IsNullOrEmpty(state)) return;
        PlayState(state);
    }

    private IEnumerator ReturnToIdleRoutine()
    {
        yield return new WaitForSeconds(returnToIdleSeconds);
        returnRoutine = null;
        if (!dead) PlayState(idleState);
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
    private void PlayState(string state)
    {
        if (animator == null || string.IsNullOrEmpty(state)) return;

        animator.CrossFadeInFixedTime(Animator.StringToHash(state), crossFadeSeconds);
        if (logStates) Debug.Log($"[Player]{state}");
    }

    //確認用。
    [ContextMenu("テスト:待機")] private void TestIdle() { dead = false;PlayLoop(idleState); }
    [ContextMenu("テスト:攻撃")] private void TestAttack() { PlayOneShot(attackState); }
    [ContextMenu("テスト:ダメージ")] private void TestHit() { PlayOneShot(hitState); }
    [ContextMenu("テスト:やられ")] private void TesDie() { OnDied(); }
    [ContextMenu("テスト:勝利")] private void TestVictory() { OnCleared(); }
}