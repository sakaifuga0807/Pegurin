using System.Data;
using UnityEngine;


//敵一体ぶんの見た目。プレハブがなくても、敵のモデルを表示できる。
public class EnemyCardView:MonoBehaviour
{
    [Header("モデルを出す場所(空なら、このオブジェクトの直下)")]
    [SerializeField] private Transform modelRoot;

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
        SpawnModel(enemy.Data);
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

    //敵の見た目を表示。モデル、アイコン、仮の立方体の順に行うS。
    private void SpawnModel(EnemyData data)
    {
        Transform parent = modelRoot != null ? modelRoot : transform;

        if(data.ModelPrefab!=null)
        {
            var model = Instantiate(data.ModelPrefab, parent);
            model.transform.localPosition = Vector3.zero;
            return;
        }

        if(data.Icon!=null)
        {
            var icon = new GameObject("Icon");
            icon.transform.SetParent(parent, false);
            icon.AddComponent<SpriteRenderer>().sprite = data.Icon;
            return;
        }

        //モデルも画像も未設定:一の確認用に、仮の立方体を出す。
        var placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(placeholder.GetComponent<Collider>());
        placeholder.transform.SetParent(parent, false);
    }
}