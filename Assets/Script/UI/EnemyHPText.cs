
using UnityEngine;

public class EnemyHPText : MonoBehaviour
{
    private Enemy enemy;
    private Transform enemyTransform;

    [SerializeField] private Vector3 offset = new Vector3(0f, 1.2f, 0f);

    // Enemyと追従先を登録
    public void SetEnemy(Enemy target, Transform targetTransform)
    {
        enemy = target;
        enemyTransform = targetTransform;

        if (enemy == null) return;

        gameObject.SetActive(true);
    }

    private void LateUpdate()
    {
        if (enemy == null || enemyTransform == null) return;

        // 敵が倒されたら数値を非表示
        if (enemy.CurrentHp <= 0)
        {
            return;
        }

        // HPバーと同じ敵の位置を取得
        Vector3 targetPosition = enemyTransform.position + offset;

        // HPバーと同じ高さ調整を適用
        targetPosition.y += enemy.Data.HpBarOffsetY;

        // 数値を敵に追従
        transform.position = targetPosition;
    }

    private void OnGUI()
    {
        if (enemy == null || enemy.Data == null) return;
        if (enemy.CurrentHp <= 0) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // ワールド座標を画面座標に変換
        Vector3 screenPos = cam.WorldToScreenPoint(transform.position);

        if (screenPos.z < 0) return;

        float x = screenPos.x;
        float y = Screen.height - screenPos.y;

        GUIStyle style = new GUIStyle();
        style.fontSize = 25;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.MiddleCenter;

        // HP表示文字列
        string hpText = enemy.CurrentHp + " / " + enemy.Data.MaxHp;


        // 黒い縁取り
        GUIStyle outlineStyle = new GUIStyle(style);
        outlineStyle.normal.textColor = Color.black;

        // 黒い縁取りを太くする
        for (int dx = -2; dx <= 2; dx++)
        {
            for (int dy = -2; dy <= 2; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                GUI.Label(
                    new Rect(x - 50 + dx, y - 15 + dy, 100, 30),
                    hpText,
                    outlineStyle
                );
            }
        }

        // 白い文字を少し重ねて太くする
        GUI.Label(new Rect(x - 51, y - 15, 100, 30), hpText, style);
        GUI.Label(new Rect(x - 49, y - 15, 100, 30), hpText, style);
        GUI.Label(new Rect(x - 50, y - 16, 100, 30), hpText, style);
        GUI.Label(new Rect(x - 50, y - 14, 100, 30), hpText, style);

        // 中央に白い文字を表示
        GUI.Label(new Rect(x - 50, y - 15, 100, 30), hpText, style);

    }
}
