
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
        style.fontSize = 16;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.MiddleCenter;

        GUI.Label(
            new Rect(x - 50, y - 15, 100, 30),
            enemy.CurrentHp + " / " + enemy.Data.MaxHp,
            style
        );
    }
}
