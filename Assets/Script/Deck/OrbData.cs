using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//オーブの特殊効果の種類。
public enum OrbEffectType
{
    None,   //特殊効果なし。
    Bomb,   //最初に当たったペグを基点に周囲のペグを巻き込む。
    Vibes,  //発射時に2個に分裂する。
    Giga    //サイズが大きい
}

[CreateAssetMenu(fileName = "OrbData", menuName = "Game/OrbData")]
public class OrbData : ScriptableObject
{
    [Header("基本")]
    public string orbName; //玉の名前。
    [Tooltip("威力。倍率は 威力/基準値")]
    public int power = 10; //威力。
    public OrbEffectType effectType= OrbEffectType.None; //特殊効果の種類。

    [Header("物理の比率")]
    public float sizeRatio = 1f; //サイズの比率。1が基準。
    public float gravityRatio = 1f; //重力の比率。1が基準。
    public float bounceRatio = 1f; //跳ね返りの比率。1が基準。

    [Header("見た目")]
    public GameObject orbPrefab; //玉のプレハブ。
}

