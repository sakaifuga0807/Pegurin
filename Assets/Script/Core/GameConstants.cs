using UnityEngine;

public static class GameConstants
{
    public const int OrbPowerBase = 10;                 //威力/この値=倍率。
    public const int PegBaseDamage = 10;                //ペグの基礎ダメージ。上記の変数とは違う意味がある。
    public const float HeavyAttackMultiplier = 2f;      //強攻撃・大技の倍率。
    public const float MultiplierInitial = 1f;          //倍率の初期化。
    public const float CritMultiplierAdd = 0.5f;        //クリティカルペグ1個当たりの倍率の加算。

    public const int SpikeBonus = 5;                    //スパイク:通常ペグの基礎ダメージの加算。
    public const int RefreshBlastDamage = 30;           //リフレッシュ爆発:敵全体へのダメージ。

    public const int LaneCellCount = 5;                 //レーンのマス数。

    public const int PlayerMaxHp = 100;                 //プレイヤーの最大HP。
    public const int StageClearHeal = 30;               //ステージクリア時の回復量。
}
