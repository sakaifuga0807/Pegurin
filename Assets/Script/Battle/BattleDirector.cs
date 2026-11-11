using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;

//演出の役目。ルールのコードはこの名前で、ここで演出を再生して終わるまで待つ。
public enum Beat
{ 
    OrbLanded,      //玉が落ちた直後。
    DamageTally,    //最終ダメージの確定。
    HitEnemy,       //手前の敵にダメージ。
    EnemyDefeated,  //敵が倒れた。
    StageClear,     //ステージクリア。
    HealShown,      //HP回復の表示。
    MapTransition,  //マップ演出。
    StageIntro,     //次のステージ登場。
    BossAppear,     //ボスの登場。
    EnemyMove,      //敵が移動。
    EnemyAttack,    //敵の攻撃。
    EnemyCharge,    //敵の溜め。
    EnemySummon,   //敵の召喚。
    PlayerHit,      //プレイヤーが被ダメージ。
    GameOver        //ゲームオーバー。
}

/*
 *演出の窓口。持つ処理を動かす場所でもあり、やり直し・タイトルへの時はCancelAll()ですべて止める。
 *ルールのコードは演出の中身を知らない。演出を作る側は、節目ごとにRegisterで登録する。
 *登録がない節目は短い基底の待ちだけで進む。
 */
public class BattleDirector : MonoBehaviour
{
    private static BattleDirector instance;

    //シーンになければ、自動で作る。
    public static BattleDirector Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<BattleDirector>();
                if (instance == null)
                {
                    instance = new GameObject("BattleDirector").AddComponent<BattleDirector>();
                }
            }
            return instance;
        }
    }

    //終了時の後片付けよう。
    public static BattleDirector Existing => instance;

    [Header("演出の設定")]
    [Tooltip("待ち時間の倍率。0なら演出を飛ばす。1がデフォルト")]
    [Min(0f)]
    [SerializeField] private float presentationScale = 1f;
    [Tooltip("一つの節目で、演出の終わりを待つ上限")]
    [Min(0.1f)]
    [SerializeField] private float maxBeastSeconds = 8f;

    private readonly Dictionary<Beat, List<Func<object, IEnumerator>>> handlers =
        new Dictionary<Beat, List<Func<object, IEnumerator>>>();

    //中段のたびに増える。待ち終わった後に、自分が始めたときと同じかを確かめるために使う。
    public int Generation { get; private set; }

    //全部の待ち時間を止めたとき。演出を作る側が、見た目をもとに戻すために受け取る。
    public event Action Cancelled;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    //待つ処理を動かす。

    //待つ処理をここで動かす。
    public Coroutine Run(IEnumerator routine)
    {
        return StartCoroutine(routine);
    }

    //待つ。時間の扱いは、ここだけ。
    public IEnumerator Wait(float seconds)
    {
        seconds *= presentationScale;
        if (seconds > 0f) yield return new WaitForSeconds(seconds);
    }

    //動かしている待ちを全部止める。
    public void CancelAll()
    {
        StopAllCoroutines();
        Generation++;
        Cancelled?.Invoke();
    }

    //演出の登録。

    /*
     * 節目に、演出を登録する。コルーチンで、終わったら戻る。
     * 同じ節目に複数登録すると、同時に再生仕手、全部が終わるまで待つ。
     */
    public void Register(Beat beat,Func<object,IEnumerator> handler)
    {
        if (handler == null) return;
        if(!handlers.TryGetValue(beat,out var list))
        {
            list = new List<Func<object, IEnumerator>>();
            handlers[beat] = list;
        }
        if (!list.Contains(handler)) list.Add(handler);
    }

    public void Unregister(Beat beat,Func<object,IEnumerator> handler)
    {
        if (handlers.TryGetValue(beat, out var list)) list.Remove(handler);
    }

    //再生。

    //節目を再生して、終わるまで待つ。登録がなければ、基底の秒数だけをまつ。
    public IEnumerator Play(Beat beat,object data=null)
    {
        if (presentationScale <= 0f) yield break;

        if(!handlers.TryGetValue(beat,out var list)||list.Count==0)
        {
            yield return Wait(DefaultSeconds(beat));
            yield break;
        }

        var copy = new List<Func<object, IEnumerator>>(list);
        int pending = copy.Count;
        foreach(var handler in copy)
        {
            StartCoroutine(RunHandler(handler, data, () => pending--));
        }

        //全部が終わるまで待つ。終わらないときは、上限で先に進む。
        float elapsed = 0f;
        while (pending > 0 && elapsed < maxBeastSeconds) 
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        if(pending>0)
        {
            Debug.LogWarning($"[Director] 演出{beat}が{maxBeastSeconds}秒で終わらなかったので、先に進みます。");
        }
;    }

    private IEnumerator RunHandler(Func<object,IEnumerator> handler,object data,Action done)
    {
        IEnumerator routine = handler(data);
        if (routine != null) yield return routine;
        done();
    }


    //演出の登録がないときの基底の待ち時間。
    private static float DefaultSeconds(Beat beat)
    {
        switch (beat)
        {
            case Beat.OrbLanded:return 0.2f;
            case Beat.DamageTally: return 0.5f;
            case Beat.HitEnemy: return 0.4f;
            case Beat.EnemyDefeated: return 0.5f;
            case Beat.StageClear: return 1.5f;
            case Beat.HealShown: return 0.6f;
            case Beat.MapTransition: return 1.0f;
            case Beat.StageIntro: return 0.8f;
            case Beat.BossAppear: return 2.0f;
            case Beat.EnemyMove: return 0.3f;
            case Beat.EnemyAttack: return 0.5f;
            case Beat.EnemyCharge: return 0.5f;
            case Beat.EnemySummon: return 0.5f;
            case Beat.PlayerHit: return 0.5f;
            case Beat.GameOver: return 2.0f;
            default: return 0.3f;
        }
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
    }
}
