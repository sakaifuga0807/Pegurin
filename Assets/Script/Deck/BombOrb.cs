using UnityEngine;

[RequireComponent(typeof(Orb))]
public class BombOrb : MonoBehaviour
{
    [Header("爆発")]
    [SerializeField] private float explosionRadius = 1.5f;

    [Header("爆発エフェクト")]
    [SerializeField] private Color ringColor = new Color32(224, 100, 92, 255);   //#e0645c

    private static Sprite ringSprite;

    private bool hasExploded;
    private Vector2 explodedAt;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded) return;
        if (!collision.gameObject.TryGetComponent<Peg>(out Peg touchedPeg)) return;

        hasExploded = true;
        Explode(touchedPeg);
    }

    private void Explode(Peg touchedPeg)
    {
        explodedAt = touchedPeg.transform.position;

        Orb orb = GetComponent<Orb>();

        Collider2D[] hits = Physics2D.OverlapCircleAll(explodedAt, explosionRadius);

        foreach (Collider2D col in hits)
        {
            Peg neighbor = col.GetComponent<Peg>();

            if (neighbor == null || neighbor == touchedPeg) continue;

            neighbor.HitBy(orb);
        }

        SpawnRing(explodedAt);
    }

    private void SpawnRing(Vector2 center)
    {
        GameObject ring = new GameObject("BombRing");
        ring.transform.position = center;

        SpriteRenderer ringSr = ring.AddComponent<SpriteRenderer>();
        ringSr.sprite = GetRingSprite();
        ringSr.color = ringColor;
        ring.AddComponent<ExpandingFade>().Play(explosionRadius, GetComponent<SpriteRenderer>());
    }

    private static Sprite GetRingSprite()
    {
        if (ringSprite != null) return ringSprite;

        const int size = 256;
        const float thicknessRatio = 0.8f;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        float center = (size - 1) * 0.5f;
        float outer = size * 0.5f - 1f;
        float inner = outer * (1f - thicknessRatio);

        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                float a = Mathf.Clamp01(outer - d + 0.5f) * Mathf.Clamp01(d - inner + 0.5f);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();

        ringSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        return ringSprite;
    }

    private void OnDrawGizmos()
    {
        if (!hasExploded) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(explodedAt, explosionRadius);
    }
}