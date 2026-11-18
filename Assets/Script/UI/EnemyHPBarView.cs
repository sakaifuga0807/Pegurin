using UnityEngine;

public class EnemyHPBarView : MonoBehaviour
{
    private Enemy enemy;

    private Vector3 originalScale;
    private Transform enemyTransform;
    private int previousHp = -1;

    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, 0);

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    // Enemy‚ð“o˜^
    public void SetEnemy(Enemy target)
    {
        enemy = target;

        if (enemy == null) return;

        previousHp = enemy.CurrentHp;
        UpdateHpBar(enemy.CurrentHp, enemy.Data.MaxHp);
    }

    private void Update()
    {
        if (enemy == null) return;

        if (previousHp != enemy.CurrentHp)
        {
            previousHp = enemy.CurrentHp;
            UpdateHpBar(enemy.CurrentHp, enemy.Data.MaxHp);
        }
    }

    private void UpdateHpBar(int currentHp, int maxHp)
    {
        if (maxHp <= 0) return;

        float ratio = Mathf.Clamp01((float)currentHp / maxHp);

        Vector3 scale = originalScale;
        scale.x = originalScale.x * ratio;

        transform.localScale = scale;
    }

    private void LateUpdate()
    {
        if (enemy == null || enemyTransform == null) return;

        // “G‚ÌˆÊ’u‚ÉHPƒo[‚ð’Ç]‚³‚¹‚é
        transform.position = enemyTransform.position + offset;

        if (previousHp != enemy.CurrentHp)
        {
            previousHp = enemy.CurrentHp;
            UpdateHpBar(enemy.CurrentHp, enemy.Data.MaxHp);
        }
    }

    public void SetEnemy(Enemy target, Transform targetTransform)
    {
        enemy = target;
        enemyTransform = targetTransform;

        if (enemy == null) return;

        previousHp = enemy.CurrentHp;
        UpdateHpBar(enemy.CurrentHp, enemy.Data.MaxHp);
    }
}
