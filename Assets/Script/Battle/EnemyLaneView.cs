using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


//敵のレーンの表示を更新するクラス。
public class EnemyLaneView : MonoBehaviour
{
    [Header("カード(任意)")]
    [Tooltip("敵カードのプレハブ。空なら、敵のモデルだけを置く。")]
    [SerializeField] private EnemyCardView cardPrefab;

    [Header("マスの位置")]
    [Tooltip("マス(1~5)の置き場所")]
    [SerializeField] private Transform[] cellAnchors = new Transform[5];
    [Tooltip("置き場所が未設定のときの、マス間隔")]
    [SerializeField] private float fallbackSpacing = 2.3f;
    [Tooltip("敵が隣のマスへ移動する速さ")]
    [SerializeField] private float slideSpeed = 8f;

    [Header("敵HPバー")]
    [SerializeField] private EnemyHPBarView[] hpBarViews;


    [Header("確認用")]
    [SerializeField] private bool logCards = false;

    [Header("敵HP数値")]
    [SerializeField] private EnemyHPText[] hpTextViews;


    private readonly Dictionary<Enemy, EnemyCardView> cards = new Dictionary<Enemy, EnemyCardView>();
    private EnemyLane lane;

    private void Start()
    {
        var context = BattleContext.Current;
        if (context == null) return;

        lane = context.EnemyLane;
        lane.OnChanged += SyncCards;
        lane.OnIntentChanged += RefreshAll;

        //Startの順序によっては最初の通知を取りこぼすかもしれないので現状維持。
        SyncCards();

        var director = BattleDirector.Instance;
        director.Register(Beat.EnemyMove, OnEnemyMoveBeat);
        director.Register(Beat.EnemyAttack, OnEnemyAttackBeat);
        director.Register(Beat.HitEnemy, OnHitEnemyBeat);
        director.Register(Beat.EnemyDefeated, OnEnemyDefeatedBeat);
    }

    private void OnDestroy()
    {
        if (lane == null) return;
        lane.OnChanged -= SyncCards;
        lane.OnIntentChanged -= RefreshAll;

        var director = BattleDirector.Existing;
        if (director != null)
        {
            director.Unregister(Beat.EnemyMove, OnEnemyMoveBeat);
            director.Unregister(Beat.EnemyAttack, OnEnemyAttackBeat);
            director.Unregister(Beat.HitEnemy, OnHitEnemyBeat);
            director.Unregister(Beat.EnemyDefeated, OnEnemyDefeatedBeat);
        }
    }

    //敵を今のマスの位置へ配置。
    private void Update()
    {
        foreach (var pair in cards)
        {
            if (pair.Value == null) continue;
            Vector3 target = CellPosition(pair.Key.CurrentCell);
            pair.Value.transform.position = Vector3.MoveTowards(
                pair.Value.transform.position, target, slideSpeed * Time.deltaTime);
        }
    }

    //レーンの内容に合わせて、見た目を作る。
    private void SyncCards()
    {
        var current = lane.GetEnemiesFrontToBack();

        //いなくなった敵の見た目を消す。
        var removed = new List<Enemy>();
        foreach (var pair in cards)
        {
            if (!current.Contains(pair.Key)) removed.Add(pair.Key);
        }
        foreach (var enemy in removed)
        {
            if (cards[enemy] != null) Destroy(cards[enemy].gameObject);
            cards.Remove(enemy);
        }

        //新しい敵の見た目を、そのマスの位置に作る。
        foreach (var enemy in current)
        {
            if (cards.ContainsKey(enemy)) continue;

            var card = CreateCard(enemy);
            card.transform.position = CellPosition(enemy.CurrentCell);
            cards[enemy] = card;
        }

        RefreshAll();
        BindHpBars(current);
        BindHpTexts(current);
    }

    private EnemyCardView CreateCard(Enemy enemy)
    {
        EnemyCardView card = cardPrefab != null
            ? Instantiate(cardPrefab, transform)
            : new GameObject("Enemy_" + enemy.Data.EnemyName).AddComponent<EnemyCardView>();

        card.transform.SetParent(transform, true);
        card.Bind(enemy);
        return card;
    }

    //マス番号の位置。
    private Vector3 CellPosition(int cell)
    {
        int index = cell - 1;
        if (index >= 0 && index < cellAnchors.Length && cellAnchors[index] != null)
        {
            return cellAnchors[index].position;
        }
        return transform.position + Vector3.right * fallbackSpacing * index;
    }

    //全カードの表示を更新する。
    private void RefreshAll()
    {
        var context = BattleContext.Current;
        if (context == null)
        {
            return;
        }

        foreach (var pair in cards)
        {
            var preview = EnemyIntentCalculator.GetPreview(pair.Key, context.EnemyLane);
            pair.Value.Refresh(preview);

            if (logCards)
            {
                Debug.Log($"[Card] {pair.Key.Data.EnemyName} マス{pair.Key.CurrentCell} " +
                          $"HP {pair.Key.CurrentHp}/{pair.Key.Data.MaxHp}：{preview.Text}");
            }
        }
    }

    private void BindHpBars(List<Enemy> enemies)
    {
        for (int i = 0; i < hpBarViews.Length; i++)
        {
            if (hpBarViews[i] == null) continue;

            if (i < enemies.Count)
            {
                Enemy enemy = enemies[i];
                hpBarViews[i].SetEnemy(enemy, cards[enemy].transform);
            }
        }
    }

    // 敵HP数値を敵に登録
    private void BindHpTexts(List<Enemy> enemies)
    {
        for (int i = 0; i < hpTextViews.Length; i++)
        {
            if (hpTextViews[i] == null) continue;

            if (i < enemies.Count)
            {
                Enemy enemy = enemies[i];

                hpTextViews[i].SetEnemy(
                    enemy,
                    cards[enemy].transform
                );
            }
            else
            {
                hpTextViews[i].SetEnemy(null, null);
            }
        }
    }


    //Sceneビューに、マスの位置を可視化。
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int cell = 1; cell <= GameConstants.LaneCellCount; cell++)
        {
            Gizmos.DrawWireSphere(CellPosition(cell), 0.4f);
        }
    }

    private IEnumerator OnEnemyMoveBeat(object data)
    {
        if (data is EnemyMoveBeat beat && cards.TryGetValue(beat.Enemy, out var card) && card != null)
        {
            card.SetTrigger("Move");
        }
        //移動演出の待ち時間。
        yield return BattleDirector.Instance.Wait(0.45f);
    }

    private IEnumerator OnEnemyAttackBeat(object data)
    {
        if (data is EnemyActionBeat beat && cards.TryGetValue(beat.Enemy, out var card) && card != null)
        {
            card.SetTrigger("Attack");
        }

        //攻撃演出の待ち時間。
        yield return BattleDirector.Instance.Wait(0.5f);
    }

    private IEnumerator OnHitEnemyBeat(object data)
    {
        if (data is HitEnemyBeat beat && beat.Damage > 0 &&
        cards.TryGetValue(beat.Target, out var card) && card != null)
        {
            card.SetTrigger("Hit");
        }
        yield return BattleDirector.Instance.Wait(0.25f);
    }

    private IEnumerator OnEnemyDefeatedBeat(object data)
    {
        if (data is HitEnemyBeat beat && cards.TryGetValue(beat.Target, out var card) && card != null)
        {
            card.SetTrigger("Death");
        }
        yield return BattleDirector.Instance.Wait(0.65f);
    }
}
