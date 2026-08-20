using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//シーン内の主要参照をまとめておくコンテナ。(シングルトンにはせずにシーンに一つだけ)
public class BattleContext : MonoBehaviour
{
    public static BattleContext Current { get; private set; }
    public DamageCalculator DamageCalculator { get; private set; }

    [field: SerializeField] public PegBoard PegBoard { get; private set; }
    [field: SerializeField] public DeckManager DeckManager { get; private set; }
    [field: SerializeField] public PassiveManager PassiveManager { get; private set; }

    private void Awake()
    {
        Current = this;
        DamageCalculator = new DamageCalculator();
    }

    private void OnDestroy()
    {
        if(Current==this)
        {
            Current = null;
        }
    }
}
