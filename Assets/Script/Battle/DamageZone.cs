using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DamageZone : MonoBehaviour
{
    [Header("エリア設定")]
    [SerializeField] private Vector2 zoneSize = new Vector2(50f, 1f);
    [SerializeField] private Vector2 zoneCenter = new Vector2(0f, -8f);

    private void Awake()
    {
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
        boxCollider.size = zoneSize;
        boxCollider.offset = zoneCenter;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Orb orb = collision.GetComponent<Orb>();

        if (orb != null)
        {
            Debug.Log($"[DamageZone] オーブを検知しました: {orb.name}");
            Destroy(collision.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawCube((Vector2)transform.position + zoneCenter, zoneSize);
    }
}