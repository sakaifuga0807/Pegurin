using System.Collections.Generic;
using UnityEngine;


//敵のレーンの表示を更新するクラス。
public class EnemyLaneView:MonoBehaviour
{
    [Header("受け取るイベント")]
    [SerializeField] private VoidEventChannel laneChanged;
    [SerializeField] private VoidEventChannel enemyIntentChanged;
    [SerializeField] private EnemyMovedEventChannel enemyMoved;

    [Header("カード")]
    [SerializeField] private EnemyCardView cardPrefab;
    [SerializeField] private Transform[] cellAnchors = new Transform[5];

    [Header("確認用")]
    [SerializeField] private bool logCards = false;

    private readonly Dictionary<Enemy, EnemyCardView> cards = new Dictionary<Enemy, EnemyCardView>();

    private void OnEnable()
    {
        if(laneChanged!=null)
        {
            laneChanged.OnRaised += SyncCards;
        }
        if(enemyIntentChanged!=null)
        {
            enemyIntentChanged.OnRaised += RefreshAll;
        }
        if(enemyMoved!=null)
        {
            enemyMoved.OnRaised += OnEnemyMoved;
        }
    }

    private void OnDisable()
    {
        if (laneChanged != null)
        {
            laneChanged.OnRaised -= SyncCards;
        }
        if(enemyIntentChanged!=null)
        {
            enemyIntentChanged.OnRaised -= RefreshAll;
        }
        if(enemyMoved!=null)
        {
            enemyMoved.OnRaised -= OnEnemyMoved;
        }
    }

    //敵が移動したとき。
    private void OnEnemyMoved(EnemyMovedInfo info) { }

    //レーンの内容に合わせる。
    private void SyncCards()
    {
        var context = BattleContext.Current;
        if (context==null)
        {
            return;
        }

        var current = context.EnemyLane.GetEnemiesFrontToBack();

        //いなくなった敵を消す。
        var removed = new List<Enemy>();
        foreach(var pair in cards)
        {
            if(!current.Contains(pair.Key))
            {
                removed.Add(pair.Key);
            }
        }
        foreach(var enemy in removed)
        {
            if (cards[enemy]!=null)
            {
                Destroy(cards[enemy].gameObject);
            }
            cards.Remove(enemy);
        }

        //新しい敵を作る。
        foreach(var enemy in current)
        {
            if(!cards.TryGetValue(enemy,out var card))
            {
                card = CreateCard(enemy);
                cards[enemy] = card;
            }
            Place(card, enemy);
        }
        RefreshAll();
    }

    private EnemyCardView CreateCard(Enemy enemy)
    {
        EnemyCardView card;
        if(cardPrefab!=null)
        {
            card = Instantiate(cardPrefab);
        }
        else
        {
            //プレハブがまだないとき。
            card = new GameObject("Card_" + enemy.Data.EnemyName).AddComponent<EnemyCardView>();
        }
        card.Bind(enemy);
        return card;
    }

    //敵の今のマスの置き場所に移す。
    private void Place(EnemyCardView card,Enemy enemy)
    {
        int index = enemy.CurrentCell - 1;
        Transform parent = (index >= 0 && index < cellAnchors.Length && cellAnchors[index] != null)
            ? cellAnchors[index]
            : transform;
        card.transform.SetParent(parent, false);
        card.transform.localPosition = Vector3.zero;
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
}
