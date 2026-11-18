using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ExpandingFade : MonoBehaviour
{
    [Header("動きの設定")]
    [SerializeField] private float duration = 0.33f;            //消えるまでの時間
    [SerializeField] private float startScaleRatio = 0.05f;     //最終サイズに対する開始サイズの割合
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.Linear(0f, 0f, 1.5f, 1.5f); //広がり方

    private SpriteRenderer sr;
    private Color baseColor;    //付けた時点の色。ここのアルファが「最初の半透明さ」になる。
    private float endScale;     //最終的な拡大率
    private float elapsed;
    private bool playing;

    public void Play(float radius, SpriteRenderer reference)
    {
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;

        if (reference != null)
        {
            sr.sortingLayerID = reference.sortingLayerID;
            sr.sortingOrder = reference.sortingOrder - 1;
        }

        float spriteWidth = sr.sprite != null ? sr.sprite.bounds.size.x : 1f;
        endScale = (radius * 2f) / spriteWidth;

        elapsed = 0f;
        playing = true;
        Apply(0f);
    }

    private void Update()
    {
        if (!playing) return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        Apply(t);

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }

    private void Apply(float t)
    {
        float scale = Mathf.Lerp(endScale * startScaleRatio, endScale, scaleCurve.Evaluate(t));
        transform.localScale = Vector3.one * scale;

        Color c = baseColor;
        c.a = Mathf.Lerp(baseColor.a, 0f, t);
        sr.color = c;
    }
}