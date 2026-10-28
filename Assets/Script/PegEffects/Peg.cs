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

        // 効果に渡す情報を構造体（PegHitContext）にまとめる。
        if(pegEffect!=null)
        {
            PegHitContext context = new PegHitContext(orb, this, parentBoard);
            pegEffect.Execute(context);
        }

        //リフレッシュで自分も復活したときは、消さない。
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

    public void ResetPegState()
    {
        StopAllCoroutines();
        isHit = false;
        if (pegCollider != null) pegCollider.enabled = true;
        transform.localScale = initialScale;
        if (spriteRenderer != null) spriteRenderer.color = initialColor;
        if (!gameObject.activeSelf) gameObject.SetActive(true);
    }
}