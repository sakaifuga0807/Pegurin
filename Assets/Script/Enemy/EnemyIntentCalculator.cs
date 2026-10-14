using UnityEngine;

//予告の種類。
public enum EnemyPreviewKind
{
    None,              //表示なし。
    Approaching,       //射程外で前が開いている。
    Waiting,           //射程外で前が塞がっているので待機。
    Action             //射程内で次の行動を表示。
}

//敵の行動予告。表示する文はTextにまとめられている。
public readonly struct EnemyPreview
{
    public readonly EnemyPreviewKind Kind;
    public readonly int TurnsToRange;       //Approachingの時だけ
    public readonly EnemyIntent Intent;     //Actionの時だけ意味を持つ。

    public EnemyPreview(EnemyPreviewKind kind,int turnsToRange=0,EnemyIntent intent=default)
    {
        Kind = kind;
        TurnsToRange = turnsToRange;
        Intent = intent;
    }

    //敵の行動文
    public string Text
    {
        get
        {
            switch (Kind)
            {
                case EnemyPreviewKind.Approaching:
                    return $"あと{TurnsToRange}ターンで射程内";
                case EnemyPreviewKind.Waiting:
                    return "待機中";
                case EnemyPreviewKind.Action:
                    switch (Intent.Type)
                    {
                        case EnemyIntentType.Attack: return $"攻撃{Intent.Value}";
                        case EnemyIntentType.HeavyAttack: return $"強攻撃{Intent.Value}";
                        case EnemyIntentType.Charge: return "溜め";
                        case EnemyIntentType.Summon: return "召喚";
                    }
                    return "";
                default:
                    return "";
                   
            }

        }
    }
}

//予告ダメージの計算。
public static class EnemyIntentCalculator
{
    //敵一体の予告。
    public static EnemyPreview GetPreview(Enemy enemy,EnemyLane lane)
    {
        //前のマスに敵がいれば待機。
        if(!enemy.IsInRange)
        {
            if(lane.IsBlockedAhead(enemy))
            {
                return new EnemyPreview(EnemyPreviewKind.Waiting);
            }
            return new EnemyPreview(EnemyPreviewKind.Approaching, enemy.TurnsToRange);
        }

        //次の行動の予告。
        var action = enemy.CurrentAction;
        if (action == null)
        {
            return new EnemyPreview(EnemyPreviewKind.None);
        }
        return new EnemyPreview(EnemyPreviewKind.Action, 0, action.GetIntent(enemy, lane));
    }

    //HUDの攻撃予告:次の敵のターンに受けるダメージの合計。
    //射程内かつ次の行動が攻撃・強攻撃・大技の敵だけ。
    public static int GetTotalIncomingDamage(EnemyLane lane)
    {
        int total = 0;
        foreach(var enemy in lane.GetEnemiesFrontToBack())
        {
            var preview = GetPreview(enemy, lane);
            if(preview.Kind==EnemyPreviewKind.Action && preview.Intent.IsDamage)
            {
                total += preview.Intent.Value;
            }
        }
        return total;
    }
}