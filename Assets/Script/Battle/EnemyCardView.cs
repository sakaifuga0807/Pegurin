using UnityEngine;

public class EnemyCardView:MonoBehaviour
{
    [Header("生成時に一回だけ")]
    [SerializeField] private StringUnityEvent onNameText;    //敵の名前。
    [SerializeField] private SpriteUnityEvent OnIcon;        //アイコン。
    [SerializeField] private StringUnityEvent onRangeText;   //射程。

    [Header("変わるたびに")]
    [SerializeField] private FloatUnityEvent onHpRatio;
    [SerializeField] private StringUnityEvent onHpText;
    [SerializeField] private StringUnityEvent onPreviewText;
    [SerializeField] private BoolUnityEvent onInRange;//射程内か。

    public Enemy Enemy { get; private set; }

    //生成直後に一回呼ぶ。
    public void Bind(Enemy enemy)
    {
        Enemy = enemy;
        onNameText?.Invoke(enemy.Data.EnemyName);
        OnIcon?.Invoke(enemy.Data.Icon);
        onRangeText?.Invoke($"射程{enemy.Data.Range}");
    }

    //配置・HP・予告が変わるたびに呼ぶ。
    public void Refresh(EnemyPreview preview)
    {
        float ratio = Enemy.Data.MaxHp > 0 ? (float)Enemy.CurrentHp / Enemy.Data.MaxHp : 0f;
        onHpRatio?.Invoke(ratio);
        onHpText?.Invoke($"{Enemy.CurrentHp}/{Enemy.Data.MaxHp}");
        onPreviewText?.Invoke(preview.Text);
        onInRange?.Invoke(Enemy.IsInRange);
    }
}