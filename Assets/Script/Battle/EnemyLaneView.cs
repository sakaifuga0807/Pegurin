using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


//敵のレーンの表示を更新するクラス。
public class EnemyLaneView:MonoBehaviour
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


    [Header("確認用")]
    [SerializeField] private bool logCards = false;


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
    }

    private void OnDestroy()
    {
        if(lane==null) return;
        lane.OnChanged -= SyncCards;
        lane.OnIntentChanged -= RefreshAll;
    }

    //敵を今のマスの位置へ配置。
    private void Update()
    {
        foreach(var pair in cards)
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
        foreach(var pair in cards)
        {
            if (!current.Contains(pair.Key)) removed.Add(pair.Key);
        }
        foreach(var enemy in removed)
        {
            if (cards[enemy] != null) Destroy(cards[enemy].gameObject);
            cards.Remove(enemy);
        }

        //新しい敵の見た目を、そのマスの位置に作る。
        foreach(var enemy in current)
        {
            if (cards.ContainsKey(enemy)) continue;

            var card = CreateCard(enemy);
            card.transform.position = CellPosition(enemy.CurrentCell);
            cards[enemy] = card;
        }

        RefreshAll();
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
        if (index >= 0 && index < cellAnchors.Length && cellAnchors[index]!=null)
        {
            return cellAnchors[index].position;
        }
        return transform.position + Vector3.right * fallbackSpacing * index;
    }

    //全カードの表示を更新する。
    private void RefreshAll()
    {
        var context = BattleContext.Current;
        if(context==null)
        {
            return;
        }

        foreach(var pair in cards)
        {
            var preview = EnemyIntentCalculator.GetPreview(pair.Key, context.EnemyLane);
            pair.Value.Refresh(preview);

            if(logCards)
            {
                Debug.Log($"[Card] {pair.Key.Data.EnemyName} マス{pair.Key.CurrentCell} " +
                          $"HP {pair.Key.CurrentHp}/{pair.Key.Data.MaxHp}：{preview.Text}");
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
}
