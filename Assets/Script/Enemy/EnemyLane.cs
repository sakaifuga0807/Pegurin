using System;
using System.Collections.Generic;

//敵レーンを管理するクラス。
public class EnemyLane
{
    //マスの番号。0は使わない。nullなら空きマス。
    private readonly Enemy[] cells = new Enemy[GameConstants.LaneCellCount + 1];

    //レーンの内容が変わった時。
    public event Action OnChanged;

    //敵が移動したとき。
    public event Action<Enemy, int, int> OnEnemyMoved;

    //予告が変わるとき。
    public event Action OnIntentChanged;

    //敵が存在しているか。
    public bool IsEmpty 
    {
        get
        {
            for(int cell=1;cell<=GameConstants.LaneCellCount;cell++)
            {
                if (cells[cell] != null) return false;
            }
            return true;
        }
    }

    //ステージ開始時にレーンを作り直す。
    public void Setup(IEnumerable<EnemyData> enemyDatas)
    {
        ClearInternal();
        foreach(var data in enemyDatas)
        {
            if(!TryPlace(new Enemy(data,data.StartCell)))
            {
                UnityEngine.Debug.LogError($"{data.EnemyName}の初期マス{data.StartCell}が他の敵と重なっています。");
            }
        }
        OnChanged?.Invoke();
    }

    //召喚処理。
    public Enemy Spawn(EnemyData data,int cell)
    {
        var enemy = new Enemy(data, cell);
        if(!TryPlace(enemy)) return null;
        OnChanged?.Invoke();
        return enemy;
    }

    //ボスを倒したときに場に出ている敵を消す。
    public void Clear()
    {
        ClearInternal();
        OnChanged?.Invoke();
    }

    public bool IsCellEmpty(int cell)
    {
        return IsValidCell(cell) && cells[cell] == null;
    }

    //一番奥の空きマス。
    public int FindBackmostEmptyCell()
    {
        for(int cell=GameConstants.LaneCellCount;cell>=1;cell--)
        {
            if (cells[cell] == null) return cell;
        }
        return 0;
    }

    //手前の敵リストをコピーして返す。
    public List<Enemy> GetEnemiesFrontToBack()
    {
        var list = new List<Enemy>();
        for(int cell=1; cell<=GameConstants.LaneCellCount;cell++)
        {
            if (cells[cell] != null) list.Add(cells[cell]);
        }
        return list;
    }

    //射程外で、一つ前のマスが埋まっているかの判定。
    public bool IsBlockedAhead(Enemy enemy)
    {
        if (enemy.IsInRange) return false;
        int front = enemy.CurrentCell - 1;
        return front >= 1 && cells[front] != null;
    }

    //指定した種類の敵がレーン上に存在するか。
    public bool Contains(EnemyData data)
    {
        for(int cell=1;cell<=GameConstants.LaneCellCount;cell++)
        {
            if (cells[cell] != null && cells[cell].Data == data) return true;
        }
        return false;
    }

    //予告を計算し直すように通知する。
    public void NotifyIntentChanged()
    {
        OnIntentChanged?.Invoke();
    }

    //前へ進める。進んだマス数を返す。
    public int Advance(Enemy enemy)
    {
        int from = enemy.CurrentCell;
        int steps = enemy.MaxMoveSteps;
        int moved = 0;

        while(moved<steps&&IsCellEmpty(enemy.CurrentCell-1))
        {
            MoveTo(enemy, enemy.CurrentCell - 1);
            moved++;
        }

        if (moved > 0)
        {
            OnEnemyMoved?.Invoke(enemy, from, enemy.CurrentCell);
            OnChanged?.Invoke();
        }
        return moved;
    }

    //一番手前の敵だけにダメージを与える。貫通しない。
    public Enemy ApplyDamageToFront(int damage)
    {
        Enemy front = null;
        for(int cell=1;cell<=GameConstants.LaneCellCount;cell++)
        {
            if (cells[cell] != null) { front = cells[cell];break; }
        }
        if (front == null) return null;

        front.TakeDamage(damage);
        RemoveDeadEnemies();
        OnChanged?.Invoke();
        return front;
    }

    //敵全体にダメージを与える。*爆発。
    public void ApplyDamageToAll(int damage)
    {
        foreach(var enemy in GetEnemiesFrontToBack())
        {
            enemy.TakeDamage(damage);
        }
        RemoveDeadEnemies();
        OnChanged?.Invoke();
    }

    //内部処理。

    private bool IsValidCell(int cell)
    {
        return cell >= 1 && cell <= GameConstants.LaneCellCount;
    }

    private bool TryPlace(Enemy enemy)
    {
        int cell = enemy.CurrentCell;
        if (!IsValidCell(cell) || cells[cell] != null) return false;
        cells[cell] = enemy;
        return true;
    }

    private void MoveTo(Enemy enemy,int newCell)
    {
        cells[enemy.CurrentCell] = null;
        cells[newCell] = enemy;
        enemy.MoveToCell(newCell);
    }

    private void ClearInternal()
    {
        for (int cell = 0; cell < cells.Length; cell++) cells[cell] = null;
    }

    //HPが0になった敵をマスから除外。
    private void RemoveDeadEnemies()
    {
        bool removed = false;
        for(int cell=1;cell<=GameConstants.LaneCellCount; cell++)
        {
            if (cells[cell] != null && cells[cell].IsDead)
            {
                cells[cell] = null;
                removed = true;
            }
        }
        if (removed) OnChanged?.Invoke();
    }
}
