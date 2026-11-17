//節目で渡す情報。演出を作る側は、(HitEnemyBeat)dataのように取り出して使う。

//DamageTally:最終ダメージの確定。
public readonly struct DamageTallyBeat
{
    public readonly float Accumulated;  //累計ダメージ。
    public readonly float Multiplier;   //倍率。
    public readonly int Total;          //最終ダメージ。
    public DamageTallyBeat(float accumulated, float multiplier, int total)
    { Accumulated = accumulated; Multiplier = multiplier; Total = total; }
}

//HitEnemy・EnemyDefeated:手前の敵への攻撃と、撃破。
public readonly struct HitEnemyBeat
{
    public readonly Enemy Target;
    public readonly int Damage;
    public readonly bool Defeated;  //この攻撃で倒れたか。
    public HitEnemyBeat(Enemy target, int damage, bool defeated)
    { Target = target; Damage = damage; Defeated = defeated; }
}

//EnemyAttack・EnemyCharge・EnemySummon:敵の行動。
public readonly struct EnemyActionBeat
{
    public readonly Enemy Enemy;
    public readonly EnemyIntent Intent;
    public readonly int Damage;
    public EnemyActionBeat(Enemy enemy, EnemyIntent intent, int damage)
    { Enemy = enemy; Intent = intent; Damage = damage; }
}

//EnemyMove:敵の移動。
public readonly struct EnemyMoveBeat
{
    public readonly Enemy Enemy;
    public readonly int FromCell;
    public readonly int ToCell;
    public EnemyMoveBeat(Enemy enemy, int from, int to)
    { Enemy = enemy; FromCell = from; ToCell = to; }
}

//PlayerHit:プレイヤーが被ダメージ。
public readonly struct PlayerHitBeat
{
    public readonly int Damage;
    public readonly int RemainingHp;
    public PlayerHitBeat(int damage, int remainingHp)
    { Damage = damage; RemainingHp = remainingHp; }
}

//HealShown:Hp回復の表示。
public readonly struct HealBeat
{
    public readonly int Healed;
    public readonly int CurrentHp;
    public readonly int MaxHp;
    public HealBeat(int healed, int currentHp, int maxHp)
    { Healed = healed; CurrentHp = currentHp; MaxHp = maxHp; }
}

//StageClear・MapTransition・BossAppear・StageIntro:ステージに関する節目。
public readonly struct StageBeat
{
    public readonly int StageIndex;
    public readonly StageConfig Stage;
    public StageBeat(int stageIndex, StageConfig stage)
    { StageIndex = stageIndex; Stage = stage; }
}