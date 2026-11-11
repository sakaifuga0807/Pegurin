using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DamageZone : MonoBehaviour
{
    [Header("エリア設定")]
    [SerializeField] private Vector2 zoneSize = new Vector2(50f, 1f);
    [SerializeField] private Vector2 zoneCenter = new Vector2(0f, -8f);

    [Header("安全ネット")]
    [Tooltip("安全ネットで跳ね返すときの。上向きの速さの下限")]
    [SerializeField] private float safetyNetBounceSpeed = 15f;

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
        if (orb == null) return;

        //安全ネット。1ターンに一回落ちた玉を跳ね返す。
        var context = BattleContext.Current;
        var passive = context != null ? context.PassiveManager : null;
        if(passive!=null&&passive.TryUseSafetyNet())
        {
            Rigidbody2D rb = collision.attachedRigidbody;
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x,
                    Mathf.Max(safetyNetBounceSpeed, Mathf.Abs(rb.linearVelocity.y)));
            }
            SE.Play(SeCue.SafetyNet);
            return;
        }
        Debug.Log($"[DamageZone]オーブが落ちました:{orb.name}");
        SE.Play(SeCue.OrbFall);
        OrbTracker.NotifyLanded(collision.gameObject);
        Destroy(collision.gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawCube((Vector2)transform.position + zoneCenter, zoneSize);
    }
}