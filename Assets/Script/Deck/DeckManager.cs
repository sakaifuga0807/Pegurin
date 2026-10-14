using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//山札と・捨て札と、弾のドロー処理を担当する。
public class DeckManager : MonoBehaviour
{
    [Header("所持デッキ'(ノーマル*3、ヘビー*1、スライム*1)")]
    [SerializeField] private List<OrbData> initialDeck = new List<OrbData>();

    //所持デッキ。
    private List<OrbData> runDeck=new List<OrbData>();
    private List<OrbData> drawPile = new List<OrbData>();
    private List<OrbData> discardPile = new List<OrbData>();

    public OrbData CurrentOrb { get; private set; }
    public int DrawPileCount => drawPile.Count;
    public int DiscardPileCount => discardPile.Count;

    //山札、捨て札、装填中が変わった時。
    public event System.Action OnChanged;

    private void Awake()
    {
        //初期デッキ。
        runDeck.AddRange(initialDeck);
    }

    //シャッフル。
    private void Shuffle(List<OrbData> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    //戦闘開始時に呼ぶ。デッキを初期化してシャッフルする。
    public void InitializeDeck()
    {
        drawPile.Clear();
        discardPile.Clear();
        CurrentOrb = null;
        drawPile.AddRange(runDeck);
        Shuffle(drawPile);
        OnChanged?.Invoke();
    }

    //次のオーブを一つドローする。ドローできなければfalseを返す。
    public bool DrawNextOrb()
    {
        if(drawPile.Count==0)
        {
            if(discardPile.Count==0)
            {
                Debug.LogWarning("デッキに弾がないです。");
                return false;
            }

            //捨て札を戻してシャッフル。
            drawPile.AddRange(discardPile);
            discardPile.Clear();
            Shuffle(drawPile);
            Debug.Log("捨て札を山札に戻してシャッフルした。");
        }

        CurrentOrb = drawPile[0];
        drawPile.RemoveAt(0);
        OnChanged?.Invoke();
        return true;
    }

    //使い終わった弾を捨て札に。
    public void DiscardCurrentOrb()
    {
        if (CurrentOrb == null) return;
        discardPile.Add(CurrentOrb);
        CurrentOrb = null;
        OnChanged?.Invoke();
    }

    //報酬で新しい弾をデッキに加える。
    public void AddOrbToDeck(OrbData newOrb)
    {
        runDeck.Add(newOrb);
        OnChanged?.Invoke();
    }
}
