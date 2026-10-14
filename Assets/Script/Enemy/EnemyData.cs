using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//敵1種類文の不変データ。
//戦闘中に変わる値は持たない。
[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("基本ステータス")]
    [SerializeField] private string enemyName; //敵の名前。
    [SerializeField] private int maxHp = 1; //最大HP。
    [Tooltip("攻撃力(強攻撃・大技はattackPower*強攻撃倍率で計算されるので持たない)")]
    [SerializeField] private int attackPower = 1; //攻撃力。

    [Header("レーン")]
    [Tooltip("ステージ開始時のマス。召喚される敵では使わない。")]
    [SerializeField] private int startCell = 5; //ステージ開始時のマス。召喚される敵では使わない。
    [Tooltip("1回の敵のターンに進める最大マス数")]
    [SerializeField] private int moveSpeed = 1; //1回の敵のターンに進める最大マス数。
    [Tooltip("マス番号がこの値以下なら攻撃できる。")]
    [SerializeField] private int range = 1; //マス番号がこの値以下なら攻撃できる。

    [Header("行動パターン")]
    [SerializeField] private List<EnemyAction> actionPattern = new List<EnemyAction>();

    [Header("見た目")]
    [SerializeField] private Sprite icon; //アイコン。

    //読み込み専用。
    public string EnemyName => enemyName;
    public int MaxHp => maxHp;
    public int AttackPower => attackPower;
    public int StartCell => startCell;
    public int MoveSpeed => moveSpeed;
    public int Range=> range;
    public IReadOnlyList<EnemyAction> ActionPattern => actionPattern;
    public Sprite Icon => icon;

    //インスペクターでの入力ミスを範囲内に補正。
    private void OnValidate()
    {
        maxHp = Mathf.Max(1, maxHp);
        attackPower = Mathf.Max(0, attackPower);
        moveSpeed = Mathf.Max(1, moveSpeed);
        startCell = Mathf.Clamp(startCell, 1, GameConstants.LaneCellCount);
        range = Mathf.Clamp(range, 1, GameConstants.LaneCellCount);
    }
}