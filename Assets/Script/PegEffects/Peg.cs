using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Peg : MonoBehaviour
{
    [Header("消失アニメーション設定")]
    [SerializeField] private float shrinkDuration = 0.2f;

    [Header("エフェクト定義 (ScriptableObject)")]
    [SerializeField] private PegEffect pegEffect;
    [SerializeField] private GameObject hitEffectPrefab;

    [Header("ダウングレード（変化）設定")]
    [SerializeField] private PegEffect normalPegEffect; // 変化後のノーマルペグ用エフェクト
    [SerializeField] private Color normalPegColor = Color.white; // 変化後の色
    private bool hasBeenDepleted = false; // 一度使われた（枯渇した）かどうかのフラグ

    private CircleCollider2D pegCollider;
    private SpriteRenderer spriteRenderer;
    private Vector3 initialScale;
    private Color initialColor;
    private bool isHit = false;
    private PegBoard parentBoard;

    private void Awake()
    {
        pegCollider = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialScale = transform.localScale;
        if (spriteRenderer != null) initialColor = spriteRenderer.color;
        parentBoard = GetComponentInParent<PegBoard>();
    }

    private void OnEnable() => ResetPegState();

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isHit) return;

        Orb orb = collision.gameObject.GetComponent<Orb>();
        if (orb == null) return;

        isHit = true;
        if (pegCollider != null) pegCollider.enabled = false;

        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        // 効果を実行
        if (pegEffect != null)
        {
            PegHitContext context = new PegHitContext(orb, this, parentBoard);
            pegEffect.Execute(context);
        }

        // ★【重要】一度ヒットしたら、次回からは「ノーマルペグ」に変化させる処理
        if (!hasBeenDepleted && normalPegEffect != null)
        {
            hasBeenDepleted = true;
            pegEffect = normalPegEffect; // エフェクトをノーマルに書き換える
            initialColor = normalPegColor; // 次回復活時の色もノーマル用に更新
        }

        // リフレッシュ等の効果によって自分自身が復活した場合は、消滅処理を行わない
        if (!isHit) return;

        StartCoroutine(ShrinkAndDisableRoutine());
    }

    private IEnumerator ShrinkAndDisableRoutine()
    {
        float elapsedTime = 0f;
        Vector3 startScale = transform.localScale;
        Color startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        while (elapsedTime < shrinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / shrinkDuration;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            if (spriteRenderer != null)
            {
                Color c = startColor;
                c.a = Mathf.Lerp(startColor.a, 0f, t);
                spriteRenderer.color = c;
            }
            yield return null;
        }
        gameObject.SetActive(false);
    }

    // 外部からエフェクトと色を動的に設定するメソッド
    public void SetPegEffect(PegEffect newEffect, Color pegColor)
    {
        pegEffect = newEffect;
        initialColor = pegColor; // 変数にしっかり保持する
        hasBeenDepleted = false;

        // 確実に即座にスプライトの色を反映させる
        ApplyColor(initialColor);
    }

    public void ResetPegState()
    {
        StopAllCoroutines();
        isHit = false;
        if (pegCollider != null) pegCollider.enabled = true;
        transform.localScale = initialScale;

        // 復活時も保持している色で確実に塗り直す
        ApplyColor(initialColor);

        if (!gameObject.activeSelf) gameObject.SetActive(true);
    }

    // 色を適用する専用の小分けメソッド（Nullチェック安全版）
    private void ApplyColor(Color col)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = col;
        }
    }
}