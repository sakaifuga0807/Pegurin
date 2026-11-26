
using UnityEngine;

public class EnemyHPText : MonoBehaviour
{
    private Enemy enemy;
    private Transform enemyTransform;

    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, 0f);

    // Enemy‚Æ’Ç]æ‚ğ“o˜^
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

        // “G‚ª“|‚³‚ê‚½‚ç”’l‚ğ”ñ•\¦
        if (enemy.CurrentHp <= 0)
        {
            return;
        }

        // HPƒo[‚Æ“¯‚¶“G‚ÌˆÊ’u‚ğæ“¾
        Vector3 targetPosition = enemyTransform.position + offset;

        // ”’l‚ğ“G‚É’Ç]
        transform.position = targetPosition;
    }

    private void OnGUI()
    {
        if (enemy == null || enemy.Data == null) return;
        if (enemy.CurrentHp <= 0) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // ƒ[ƒ‹ƒhÀ•W‚ğ‰æ–ÊÀ•W‚É•ÏŠ·
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
