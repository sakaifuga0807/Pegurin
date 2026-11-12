
using UnityEngine;

public class HPBarView : MonoBehaviour
{
    [SerializeField] private Player player;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (player == null) return;

        player.OnHpChanged += UpdateHpBar;
        UpdateHpBar(player.CurrentHp, player.MaxHp);
    }

    private void OnDisable()
    {
        if (player != null)
            player.OnHpChanged -= UpdateHpBar;
    }

    private void UpdateHpBar(int currentHp, int maxHp)
    {
        if (maxHp <= 0) return;

        float ratio = Mathf.Clamp01((float)currentHp / maxHp);

        Vector3 scale = originalScale;
        scale.x = originalScale.x * ratio;

        transform.localScale = scale;
    }
}
