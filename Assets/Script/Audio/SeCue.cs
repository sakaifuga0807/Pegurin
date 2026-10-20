using UnityEngine;

public enum SeCue
{ 
    Launch,                 //発射。
    PegHitNormal,           //通常ペグ命中。
    PegHitCrit,             //クリティカルペグ命中。
    PegHitRefresh,          //リフレッシュペグ命中。
    WallBounce,             //壁で跳ね返る。
    OrbFall,                //落下ゾーンに入る。
    BombExplode,             //ボムの爆発。
    SafetyNet,              //安全ネットで跳ね返る。

    DamageDealt,            //敵にダメージ。
    EnemyDefeated,          //敵が倒れる。
    EnemyMove,              //敵が移動する。
    EnemyAttack,            //敵が攻撃する。
    EnemyHeavyAttack,       //敵の強攻撃・大技。
    EnemySummon,            //召喚。
    PlayerDamaged,          //プレイヤーがダメージを受ける。
    StageClear,             //ステージクリア。
    GameOver,               //ゲームオーバー。
    RewardSelect            //報酬を選ぶ。
}