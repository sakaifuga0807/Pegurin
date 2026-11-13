using System.Collections;
using UnityEngine;

//バイブス:最初にペグに触れた時に2個に分裂する玉。
[RequireComponent(typeof(Orb))]
public class VibesOrb : MonoBehaviour
{
    [Header("分裂の幅")]
    [SerializeField] private float splitAngle = 45f;

    private Rigidbody2D rb;
    private Collider2D col;
    private Vector3 baseScale;
    private float baseGravityScale;

    //分裂で生まれた側かどうか。これが無いと分裂した玉がさらに分裂して無限に増える。
    private bool isTwin;
    //すでに分裂したか。分裂は1回だけ。
    private bool hasSplit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        baseScale = transform.localScale;
        baseGravityScale = rb.gravityScale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isTwin || hasSplit) return;

        //当たった相手がペグ以外ならそのまま。
        if (!collision.gameObject.TryGetComponent<Peg>(out _)) return;

        hasSplit = true;

        StartCoroutine(SplitAfterBounce(collision.collider));
    }

    private IEnumerator SplitAfterBounce(Collider2D touchedPegCollider)
    {
        //次の物理ステップまで待つ。再開した時点では、Orb側の跳ね返り処理が終わっている。
        yield return new WaitForFixedUpdate();

        //跳ね返った後の速度を読む。
        Vector2 velocity = rb.linearVelocity;

        Vector3 scaledScale = transform.localScale;
        float scaledGravity = rb.gravityScale;

        transform.localScale = baseScale;
        rb.gravityScale = baseGravityScale;

        //分裂オーブを生成する。
        GameObject twinObj = Instantiate(gameObject, transform.position, transform.rotation);

        //本体を補正済みの値に戻す。
        transform.localScale = scaledScale;
        rb.gravityScale = scaledGravity;

        VibesOrb twin = twinObj.GetComponent<VibesOrb>();
        twin.isTwin = true;

        //コピーの見た目と重力を本体と同じにする。
        twinObj.transform.localScale = scaledScale;
        twin.rb.gravityScale = scaledGravity;

        Collider2D twinCol = twinObj.GetComponent<Collider2D>();

        //2個がお互いに弾き合わないようにする。
        Physics2D.IgnoreCollision(col, twinCol);

        //コピーは最初のペグと重なった位置に生まれる。
        //そのペグとはぶつからないようにして、押し出されて挙動が変になるのを防ぐ。
        if (touchedPegCollider != null && touchedPegCollider.enabled)
        {
            Physics2D.IgnoreCollision(twinCol, touchedPegCollider);
        }

        //今の進行方向を、左右に半分ずつ(splitAngle/2)回して2方向に分ける。
        float half = splitAngle * 0.5f;
        rb.linearVelocity = Quaternion.Euler(0f, 0f, -half) * velocity;
        twin.rb.linearVelocity = Quaternion.Euler(0f, 0f, half) * velocity;
    }
}