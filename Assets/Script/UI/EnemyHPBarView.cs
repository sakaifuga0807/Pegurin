
using UnityEngine;

public class EnemyHPBarView : MonoBehaviour
{
    private Enemy enemy;

    private Vector3 originalScale;
    private Transform enemyTransform;
    private int previousHp = -1;

    // “G‚Ì‰æ‘œEƒ‚ƒfƒ‹‚ğæ“¾
    private Renderer[] enemyRenderers;

    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, 0f);

    // •‚¢HP”wŒi‰æ‘œ
    [SerializeField] private Transform backgroundTransform;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    // Enemy‚Æ’Ç]æ‚ğ“o˜^
    public void SetEnemy(Enemy target, Transform targetTransform)
    {
        enemy = target;
        enemyTransform = targetTransform;

        // “G‚ª‚¢‚È‚¢ê‡‚ÍHPƒo[‚Æ”wŒi‚ğ”ñ•\¦
        if (enemy == null)
        {
            gameObject.SetActive(false);

            if (backgroundTransform != null)
            {
                backgroundTransform.gameObject.SetActive(false);
            }

            return;
        }

        // “G‚Ì‰æ‘œEƒ‚ƒfƒ‹‚ğæ“¾
        enemyRenderers = enemyTransform.GetComponentsInChildren<Renderer>();

        // HPƒo[‚Æ”wŒi‚ğÄ•\¦
        gameObject.SetActive(true);

        if (backgroundTransform != null)
        {
            backgroundTransform.gameObject.SetActive(true);
        }

        previousHp = enemy.CurrentHp;
        UpdateHpBar(enemy.CurrentHp, enemy.Data.MaxHp);
    }

    private void Update()
    {
        if (enemy == null) return;

        // “G‚ª“|‚³‚ê‚½‚çHPƒo[‚Æ”wŒi‚ğÁ‚·
        if (enemy.CurrentHp <= 0)
        {
            gameObject.SetActive(false);

            if (backgroundTransform != null)
            {
                backgroundTransform.gameObject.SetActive(false);
            }

            return;
        }

        // HP‚ª•Ï‰»‚µ‚½‚Æ‚«‚ÉXV
        if (previousHp != enemy.CurrentHp)
        {
            previousHp = enemy.CurrentHp;
            UpdateHpBar(enemy.CurrentHp, enemy.Data.MaxHp);
        }
    }

    // Ô‚¢HPƒQ[ƒW‚ğXV
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

        Vector3 targetPosition = enemyTransform.position + offset;

        // “G‚²‚Æ‚ÌHPƒo[‚Ì‚‚³‚ğ‰ÁZ
        targetPosition.y += enemy.Data.HpBarOffsetY;

        // Ô‚¢HPƒQ[ƒW‚ğ’Ç]
        transform.position = targetPosition;

        // •‚¢”wŒi‰æ‘œ‚à’Ç]
        if (backgroundTransform != null)
        {
            backgroundTransform.position = targetPosition;
        }
    }
}
